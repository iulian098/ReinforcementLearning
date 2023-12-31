using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System;
using Cinemachine;
using System.Threading.Tasks;
using System.Text;
using NPOI.HSSF.Record;
using TMPro;
using StateMachine.Player;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Runner.RL {

    public class RunnerEnvironment : MonoBehaviour {

        [Serializable]
        public class RunnerEnvironmentParams {
            public int state_size;
            public int action_size;
            public List<string> action_descriptions;
            public int num_agents;
            public RunnerState[] states;
        }

        public static RunnerEnvironment Instance;

        [Header("States settings")]
        [SerializeField] Vector2Int horizontalLimits;
        [SerializeField] int maxForwardCheck;

        [Header("Camera settings")]
        [SerializeField] CinemachineVirtualCamera cvcCamera;

        [Header("Data")]
        [SerializeField] bool loadData;
        [SerializeField] bool saveData;
        [SerializeField] string bestScoreDataName;
        [SerializeField] int saveEveryEpisode = 500;
        [SerializeField] int dataNr;
        [Space]

        [Header("Settings")]
        [SerializeField] bool regenerateLevelOnDeath;
        [SerializeField] float startingEpisodeReward;
        [SerializeField] GenerateGrid gridGenerator;
        [SerializeField] Transform spawnPoint;
        [SerializeField] RunnerBaseAgent playerPrefab;

        [SerializeField] int maxSteps;
        [SerializeField] int numberOfAgents;

        [SerializeField] DefaultAgent agentSettings;
        [SerializeField] List<RunnerBaseAgent> agentsList;

        [SerializeField] float waitTime;

        [Header("UI")]
        [SerializeField] TMP_Text eValue;
        [SerializeField] TMP_Text agentRewardText;
        [SerializeField] TMP_Text agentEpisodeRewardText;
        [SerializeField] TMP_Text episodeCountText;
        [SerializeField] TMP_Text actionText;

        int currentAction;
        int episodeCount;
        int maxScore = 0;

        bool initialized;

        RunnerEnvironmentParams envParameters;
        Dictionary<RunnerState, float[]> bestQTable;
        List<(int, int)> bestScores = new List<(int, int)>();
        RunnerBaseAgent bestAgent;
        Obstacle currentObstacle;

        public Dictionary<RunnerState, float[]> BestQTable => bestQTable;
        public bool RegenerateLevelOnDeath { get => regenerateLevelOnDeath; set => regenerateLevelOnDeath = value; }

        void Awake() {
            Instance = this;
            episodeCount = 0;
        }

        async void Start() {
            
            Application.targetFrameRate = 60;

            RunnerManager.Instance.Init();
        }

        public async void BeginNewGame() {

            Debug.Log("Begin!!!!!!!!!!!!");
            SetUp();

            //Setting up agent
            for (int i = 0; i < numberOfAgents; i++) {
                RunnerBaseAgent a = Instantiate(playerPrefab, spawnPoint.position, spawnPoint.rotation);
                cvcCamera.Follow = a.transform;
                cvcCamera.LookAt = a.transform;
                a.Init(envParameters, agentSettings);

                //a.SetAnnealingSteps(maxSteps * 20);
                a.gameObject.name = "Agent" + i;
                if (loadData) {
                    //a.qTable = loadedQTable;
                    await a.LoadData();
                    a.dataNr = dataNr;
                    a.SetEValue(0.5f);
                }
                agentsList.Add(a);
            }

            Reset(true);
            initialized = true;
        }

        void SetUp() {
            agentsList = new List<RunnerBaseAgent>(numberOfAgents);
            gridGenerator.Init();
            List<RunnerState> states = GenerateStates();
            List<string> actionNames = new List<string>() { 
                "DoNothing",
                "Left",
                "Right",
                "Roll",
                "Jump"
            };
            envParameters = new RunnerEnvironmentParams() {
                state_size = states.Count,
                action_descriptions = actionNames,
                action_size = actionNames.Count,
                num_agents = 1,
                states = states.ToArray()
            };
        }

        void FixedUpdate() {
            if (!initialized) return;
            for (int i = 0; i < agentsList.Count; i++) {
                Run(agentsList[i], i == agentsList.Count - 1);
            }
        }

        void UpdateUI(RunnerBaseAgent agent, int action) {
            eValue.text = $"E: {agent.E}";
            agentRewardText.text = $"Reward: {agent.reward}";
            agentEpisodeRewardText.text = $"Episode Reward: {agent.episodeReward}";
            episodeCountText.text = $"Episode: {episodeCount}";
            actionText.text = $"Action: {envParameters.action_descriptions[action]}";
        }

        void Run(RunnerBaseAgent agent, bool isLast = false) {
            if (agent.acceptingSteps && agent.RunnerPlayer.AcceptingSteps) {
                if (!agent.done)
                    Step(agent, isLast);
                else
                    Reset();
            }
        }

        void Step(RunnerBaseAgent agent, bool isLast) {
            if (isLast)
                agent.acceptingSteps = false;
            agent.currentStep += 1;

            if (agent.currentStep >= maxSteps - 1) {
                agent.done = true;
            }

            agent.reward = 0;
            currentAction = (int)agent.GetAction();

            int sendAction = Mathf.FloorToInt(currentAction);
            MiddleStep(sendAction, agent);

            StartCoroutine(WaitStep());
        }

        IEnumerator WaitStep() {
            yield return new WaitForSeconds(waitTime);
            foreach (RunnerBaseAgent a in agentsList) {
                EndStep(a);
                yield return new WaitUntil(() => a.RunnerPlayer.AcceptingSteps);
                a.acceptingSteps = true;
            }

        }

        void MiddleStep(int action, RunnerBaseAgent agent) {
            /*
            0 - DoNothing, 
            1 - Left, 
            2 - Right,  
            3 - Roll,
            4 - Jump
            */
            agent.reward = 0.01f;


            switch (action) {
                case 0:
                    //DoNothing
                    break;
                case 1:
                    agent.RunnerPlayer.SendInput(new StateMachine.Player.PlayerInput() {
                        direction = new Vector2(-1, 0)
                    });
                    break;
                case 2:
                    agent.RunnerPlayer.SendInput(new StateMachine.Player.PlayerInput() {
                        direction = new Vector2(1, 0)
                    });
                    break;
                case 3:
                    agent.RunnerPlayer.SwitchState(new RollState(agent.RunnerPlayer));
                    break;
                case 4:
                    agent.RunnerPlayer.SwitchState(new JumpState(agent.RunnerPlayer));
                    break;
                default:
                    break;
            }


            currentObstacle = RunnerManager.Instance.Obstacles[agent.RunnerPlayer.CurrentObstacle];

            if (currentObstacle.ObstacleType == ObstacleType.Wall && action >= 3)
                agent.reward = -0.01f;
            else if (currentObstacle.ObstacleType == ObstacleType.Slide && (action != 0 && action != 3))
                agent.reward = -0.01f;
            else if (currentObstacle.ObstacleType == ObstacleType.Jump && (action != 0 && action != 4)) {
                agent.reward = -0.01f;
            }
            else if (currentObstacle.ObstacleType == ObstacleType.Wall && (action == 1 || action == 2)) {
                int currentDistance = (int)Mathf.Abs(agent.transform.position.x - currentObstacle.transform.position.x);

                if (currentDistance > agent.RunnerPlayer.LastXDistance)
                    agent.reward = -0.015f;
                else if (currentDistance < agent.RunnerPlayer.LastXDistance)
                    agent.RunnerPlayer.LastXDistance = currentDistance;
            }

            agent.episodeReward += agent.reward;

            UpdateUI(agent, action);
        }

        void EndStep(RunnerBaseAgent agent) {
            Obstacle obs = RunnerManager.Instance.Obstacles[agent.RunnerPlayer.CurrentObstacle];
            agent.SendState(new RunnerState() {
                XDistance = (int)(agent.transform.position.x - obs.transform.position.x),
                YDistance = (int)(obs.transform.position.z - agent.transform.position.z),
                ObstacleType = obs.ObstacleType
            });
        }

        void Reset(bool firstTime = false) {
            if (!firstTime) {
                foreach (RunnerBaseAgent a in agentsList)
                    if (!a.done) return;
            }

            if (regenerateLevelOnDeath)
                RunnerManager.Instance.RegenerateLevel();

            /*if (!firstTime) {

                if (bestAgent == null)
                    bestAgent = agentsList[0];
                if (episodeCount <= 1 || bestQTable == null) {
                    bestQTable = new Dictionary<RunnerState, float[]>(agentsList[0].qTable);
                }

                bool newBest = false;
                if (agentsList.Count > 1) {

                    for (int i = 1; i < agentsList.Count; i++) {

                        if (agentsList[i].Finished && agentsList[i].episodeReward > bestReward) {
                            bestAgent = agentsList[i];
                            agentFinished = true;
                            newBest = true;
                            Debug.Log($"<color=green>New best agent is {agentsList[i].name} with reward: {agentsList[i].episodeReward} finished</color>");
                        }
                        else if (agentsList[i].episodeReward > bestReward && !agentFinished) {
                            bestAgent = agentsList[i];
                            newBest = true;
                            if (agentsList[i] != null)
                                Debug.Log($"<color=green>New best agent is {agentsList[i].name} with reward: {agentsList[i].episodeReward}</color>");
                        }
                    }
                    if (bestAgent != null && newBest) {
                        if (bestAgent != null)
                            Debug.Log($"<color=yellow>Best agent is : {bestAgent.name} with episodeReward: {bestAgent.episodeReward}</color>");
                        bestReward = bestAgent.episodeReward;
                        bestQTable = new Dictionary<RunnerState, float[]>(bestAgent.qTable);
                    }
                }

            }*/

            //acceptingSteps = true;
            episodeCount++;

            if (RunnerManager.Instance.Score > maxScore) maxScore = (int)RunnerManager.Instance.Score;

            RunnerManager.Instance.ResetScore();

            for (int i = 0; i < agentsList.Count; i++) {

                agentsList[i].episodeReward = startingEpisodeReward;
                agentsList[i].PlayerTransform.position = spawnPoint.position;
                agentsList[i].PlayerTransform.rotation = spawnPoint.rotation;
                agentsList[i].ResetAgent();


                /*if (loadData)
                    System.Array.Copy(loadedQ, agentsList[i].q_table, loadedQ.Length);
                else*/
                /*if (episodeCount > 0 && bestQTable != null && useBestQTable) {  //Set best Q table
                    agentsList[i].qTable = new Dictionary<RunnerState, float[]>(bestQTable);
                }*/


                if (episodeCount > 0 && episodeCount % saveEveryEpisode == 0 && saveData) {
                    agentsList[i].SaveData();
                }
                EndReset(agentsList[i]);
            }

            Debug.LogError("Reset");

            if (episodeCount > 0 && episodeCount % saveEveryEpisode == 0 && saveData) {
                bestScores.Add((episodeCount, maxScore));

                string rewards = "";

                foreach (var item in bestAgent.RewardList) {
                    rewards += item + ",";
                }

                rewards.Remove(rewards.Length - 1, 1);
                File.WriteAllText($"Data/Rewards{bestAgent.name}_{bestScoreDataName}_{episodeCount}.csv", rewards);




            }
        }

        void EndReset(RunnerBaseAgent agent) {
            Obstacle obs = RunnerManager.Instance.Obstacles[agent.RunnerPlayer.CurrentObstacle];
            agent.SendState(new RunnerState() {
                XDistance = (int)(agent.transform.position.x - obs.transform.position.x),
                YDistance = (int)(obs.transform.position.z - agent.transform.position.z),
                ObstacleType = obs.ObstacleType
            });

            agent.acceptingSteps = true;

        }

        List<RunnerState> GenerateStates() {
            List<RunnerState> states = new List<RunnerState>();
            for (int type = 0; type < Enum.GetValues(typeof(ObstacleType)).Length; type++) {
                for (int i = horizontalLimits.x; i <= horizontalLimits.y; i++) {
                    for (int j = 0; j <= maxForwardCheck; j++) {
                        states.Add(new RunnerState() {
                            XDistance = i,
                            YDistance = j,
                            ObstacleType = (ObstacleType)type
                        });
                    }
                }
            }

            return states;
        }

        public void SetAgent(RunnerBaseAgent agent) {
            playerPrefab = agent;
        }

        public void StopTraining() {
            initialized = false;
            foreach (var agent in agentsList) {
                Destroy(agent.gameObject);
            }

            agentsList.Clear();
        }

        public void ToggleLoadData(bool value) {
            loadData = value;
        }

        public void SaveData() {
            foreach (var agent in agentsList) {
                agent.SaveData();
            }
        }
    }

}

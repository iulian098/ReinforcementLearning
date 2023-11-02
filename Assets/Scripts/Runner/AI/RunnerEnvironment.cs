using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System;
using Cinemachine;
using System.Threading.Tasks;
using System.Text;

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
        [SerializeField] string dataFileName;
        [SerializeField] bool saveData;
        [SerializeField] string bestScoreDataName;
        [SerializeField] int saveEveryEpisode = 500;
        [SerializeField] int dataNr;
        [Space]

        [Header("Settings")]
        [SerializeField] float startingEpisodeReward;
        [SerializeField] GenerateGrid gridGenerator;
        [SerializeField] Transform spawnPoint;
        [SerializeField] RunnerAgent playerPrefab;

        [SerializeField] int maxSteps;
        [SerializeField] int numberOfAgents;

        [SerializeField] bool useBestQTable;
        [SerializeField] DefaultAgent agentSettings;
        [SerializeField] List<RunnerAgent> agentsList;

        [SerializeField] float waitTime;

        int currentAction;
        int episodeCount;
        int maxScore = 0;

        float bestReward;

        bool agentFinished;
        bool initialized;

        RunnerEnvironmentParams envParameters;
        Dictionary<RunnerState, float[]> bestQTable;
        List<(int, int)> bestScores = new List<(int, int)>();
        RunnerAgent bestAgent;
        Obstacle currentObstacle;

        public Dictionary<RunnerState, float[]> BestQTable => bestQTable;
        Dictionary<RunnerState, float[]> loadedQTable = new Dictionary<RunnerState, float[]>();

        void Awake() {
            Instance = this;
            bestReward = float.MinValue;
            episodeCount = 0;
            if (numberOfAgents == 1)
                useBestQTable = false;
        }

        async void Start() {
            
            Application.targetFrameRate = 60;

            RunnerManager.instance.Init();

            if (loadData) {
                Debug.Log("Start loading data");
               loadedQTable = await LoadData();
                Debug.Log("Done loading data");
            }
            
            BeginNewGame();
        }

        async Task<Dictionary<RunnerState, float[]>> LoadData() {
            int bufferSize = 128;
            Dictionary<RunnerState, float[]> loadedQ = new Dictionary<RunnerState, float[]>();

            using (FileStream fs = File.OpenRead($"Data/{dataFileName}.csv")) {

                using (var streamReader = new StreamReader(fs, Encoding.UTF8, true, bufferSize)) {

                    string line = await streamReader.ReadLineAsync();
                    while (true) {

                        string[] data = line.Split(',');

                        RunnerState s = RunnerState.StringToState(data[0]);
                        float[] actions = new float[] {
                            float.Parse(data[1]),
                            float.Parse(data[2]),
                            float.Parse(data[3]),
                            float.Parse(data[4])
                        };

                        loadedQ.Add(s, actions);

                        await Task.Yield();
                        line = await streamReader.ReadLineAsync();

                        if (line == null) break;
                    }
                }
            }


            return loadedQ;
        }

        void BeginNewGame() {

            Debug.Log("Begin!!!!!!!!!!!!");
            SetUp();

            //Setting up agent
            for (int i = 0; i < numberOfAgents; i++) {
                RunnerAgent a = Instantiate(playerPrefab, spawnPoint.position, spawnPoint.rotation);
                cvcCamera.Follow = a.transform;
                cvcCamera.LookAt = a.transform;
                a.Init(envParameters, agentSettings);

                //a.SetAnnealingSteps(maxSteps * 20);
                a.gameObject.name = "Agent" + i;
                if (loadData) {
                    a.qTable = loadedQTable;
                    //a.loadData = loadData;
                    //loadedQ = a.LoadData(dataNr);
                    //a.q_table = loadedQ;
                    a.dataNr = dataNr;
                    a.SetEValue(0.5f);
                }
                agentsList.Add(a);
            }

            Reset(true);
            initialized = true;
        }

        void SetUp() {
            agentsList = new List<RunnerAgent>(numberOfAgents);
            gridGenerator.Init();
            List<RunnerState> states = GenerateStates();

            /*for (int i = 0; i < Enum.GetValues(typeof(ObstacleType)).Length; i++) {
                for (int cell = 0; cell < gridGenerator.Cells.Length; cell++) {
                    states.Add(new RunnerState() {
                        XDistance = gridGenerator.Cells[cell].x,
                        YDistance = gridGenerator.Cells[cell].y,
                        ObstacleType = (ObstacleType)i
                    });
                }
            }*/

            envParameters = new RunnerEnvironmentParams() {
                state_size = states.Count,
                action_descriptions = new List<string>() { "DoNothing", "Left", "Right", "Roll" },
                action_size = 4,
                num_agents = 1,
                states = states.ToArray()
                // grid = gridGenerator.Cells
            };
        }

        void FixedUpdate() {
            if (!initialized) return;
            for (int i = 0; i < agentsList.Count; i++) {
                if (i == agentsList.Count - 1)
                    Run(agentsList[i], true);
                else
                    Run(agentsList[i]);
            }
        }

        void Run(RunnerAgent agent, bool isLast = false) {
            //if (acceptingSteps == true) {
            if (agent.acceptingSteps && agent.RunnerPlayer.AcceptingSteps) {
                if (!agent.done)
                    Step(agent, isLast);
                else
                    Reset();
            }
        }

        void Step(RunnerAgent agent, bool isLast) {
            if (isLast)
                agent.acceptingSteps = false;
            agent.currentStep += 1;

            if (agent.currentStep >= maxSteps - 1) {
                agent.done = true;
                //agent.Player.SendInput();
                //return;
            }

            agent.reward = 0;
            currentAction = (int)agent.GetAction();

            int sendAction = Mathf.FloorToInt(currentAction);
            MiddleStep(sendAction, agent);

            StartCoroutine(WaitStep());
        }

        IEnumerator WaitStep() {
            yield return new WaitForSeconds(waitTime);
            foreach (RunnerAgent a in agentsList) {
                yield return new WaitUntil(() => a.RunnerPlayer.AcceptingSteps);
                EndStep(a);
                a.acceptingSteps = true;
            }

        }

        void MiddleStep(int action, RunnerAgent agent) {
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
                    agent.RunnerPlayer.ReceiveInput(-1, false, false);
                    break;
                case 2:
                    agent.RunnerPlayer.ReceiveInput(1, false, false);
                    break;
                case 3:
                    agent.RunnerPlayer.ReceiveInput(0, false, true);
                    break;
                case 4:
                    agent.RunnerPlayer.ReceiveInput(0, true, false);
                    break;
                default:
                    break;
            }

            /* Collider[] detectedColls = Physics.OverlapSphere(agent.PlayerTransform.position, 1);

             if (detectedColls.Length > 0) {
                 foreach (Collider coll in detectedColls) {
                     if (coll.CompareTag("Finish")) {
                         agent.reward = 1;
                         agent.done = true;
                         agent.Finished = true;
                         Debug.LogWarning("<color=green>Finish</color>");
                         //agent.Player.SendInput();
                     }
                     else if (coll.CompareTag("Obstacle")) {
                         agent.done = true;
                         agent.reward -= 10;
                         Debug.LogWarning("<color=red>Obstacle Hit</color>");
                         //agent.Player.SendInput();
                     }
                 }
             }*/

            currentObstacle = RunnerManager.instance.Obstacles[agent.RunnerPlayer.CurrentObstacle];

            if (currentObstacle.ObstacleType == ObstacleType.Wall && action == 3)
                agent.reward = -0.01f;
            else if (currentObstacle.ObstacleType == ObstacleType.Slide && (action == 1 || action == 2))
                agent.reward = -0.01f;
            else if (currentObstacle.ObstacleType == ObstacleType.Wall && (action == 1 || action == 2)) {
                int currentDistance = (int)Mathf.Abs(agent.transform.position.x - currentObstacle.transform.position.x);

                if (currentDistance > agent.RunnerPlayer.LastXDistance)
                    agent.reward = -0.015f;
                else if(currentDistance < agent.RunnerPlayer.LastXDistance)
                    agent.RunnerPlayer.LastXDistance = currentDistance;
            }

            agent.episodeReward += agent.reward;
        }

        void EndStep(RunnerAgent agent) {
            Obstacle obs = RunnerManager.instance.Obstacles[agent.RunnerPlayer.CurrentObstacle];
            agent.SendState(new RunnerState() {
                XDistance = (int)(agent.transform.position.x - obs.transform.position.x),
                YDistance = (int)(obs.transform.position.z - agent.transform.position.z),
                ObstacleType = obs.ObstacleType
            });
        }

        void Reset(bool firstTime = false) {
            if (!firstTime) {
                foreach (RunnerAgent a in agentsList)
                    if (!a.done) return;
            }

            if (!firstTime) {

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

            }

            //acceptingSteps = true;
            episodeCount++;

            if (RunnerManager.instance.Score > maxScore) maxScore = (int)RunnerManager.instance.Score;

            RunnerManager.instance.ResetScore();

            for (int i = 0; i < agentsList.Count; i++) {

                agentsList[i].ResetAgent();
                agentsList[i].episodeReward = startingEpisodeReward;
                agentsList[i].PlayerTransform.position = spawnPoint.position;
                agentsList[i].PlayerTransform.rotation = spawnPoint.rotation;


                /*if (loadData)
                    System.Array.Copy(loadedQ, agentsList[i].q_table, loadedQ.Length);
                else*/
                if (episodeCount > 0 && bestQTable != null && useBestQTable) {  //Set best Q table
                    agentsList[i].qTable = new Dictionary<RunnerState, float[]>(bestQTable);
                }


                if (episodeCount > 0 && episodeCount % saveEveryEpisode == 0 && saveData) {
                    agentsList[i].SaveData(i, episodeCount);
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

        void EndReset(RunnerAgent agent) {
            Obstacle obs = RunnerManager.instance.Obstacles[agent.RunnerPlayer.CurrentObstacle];
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

#if UNITY_EDITOR

        private void OnDrawGizmos() {
            Gizmos.color = Color.blue;

            for (int i = 0; i < agentsList.Count; i++) {
                if (agentsList[i].PlayerTransform != null && agentsList[i].PlayerTransform.gameObject.activeInHierarchy) {
                    Gizmos.DrawWireSphere(agentsList[i].PlayerTransform.position, 1);
                    Handles.Label(agentsList[i].PlayerTransform.position, $"{agentsList[i].episodeReward}\nE:{agentsList[i].E}");
                }
            }
        }

        [ContextMenu("Save Max Scores")]
        void SaveMaxScores() {
            string data = "";

            foreach (var item in bestScores) {
                data += item.Item1 + "," + item.Item2;
                data += "\n";
            }

            File.WriteAllText($"Data/{bestScoreDataName}.csv", data);

        }

#endif

        private void OnDrawGizmosSelected() {
            Gizmos.DrawWireSphere(spawnPoint.position, 1);

        }
    }

}

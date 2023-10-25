using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using Cinemachine;
using System.IO;

#if UNITY_EDITOR
using UnityEditor;
#endif
public class RunnerEnvironment : MonoBehaviour {
    public static RunnerEnvironment instance;

    [Header("Data")]
    public bool loadData;
    public bool saveData;
    [SerializeField] string bestScoreDataName;
    [SerializeField] int saveEveryEpisode = 500;
    public int dataNr;
    [Space]

    [Header("Settings")]
    public float startingEpisodeReward;
    [SerializeField] GenerateGrid gridGenerator;
    [SerializeField] Transform spawnPoint;
    [SerializeField] RunnerAgent playerPrefab;

    public int maxSteps;
    public int numberOfAgents;

    public bool useBestQTable;
    public DefaultAgent agentSettings;
    public List<RunnerAgent> agentsList;

    public float waitTime;

    int currentAction;
    int episodeCount;
    int maxScore = 0;

    float bestReward;

    bool agentFinished;

    EnvironmentParams envParameters;
    Dictionary<Vector2Int, float[]> bestQTable;
    List<(int, int)> bestScores = new List<(int, int)>();
    RunnerAgent bestAgent;

    public Dictionary<Vector2Int, float[]> BestQTable => bestQTable;

    private void Awake() {
        instance = this;
        bestReward = float.MinValue;
        episodeCount = 0;
        if (numberOfAgents == 1)
            useBestQTable = false;
    }

    void Start() {
        RunnerManager.instance.Init();
        BeginNewGame();
    }

    void BeginNewGame() {

        Debug.Log("Begin!!!!!!!!!!!!");
        SetUp();
        Debug.Log($"Env param : state size {envParameters.state_size}, action size {envParameters.action_size}");

        //Setting up agent
        for (int i = 0; i < numberOfAgents; i++) {
            RunnerAgent a = Instantiate(playerPrefab, spawnPoint.position, spawnPoint.rotation);
            a.Init(envParameters, agentSettings);
            //a.SetAnnealingSteps(maxSteps * 20);
            a.gameObject.name = "Agent" + i;
            if (loadData) {
                a.loadData = loadData;
                //loadedQ = a.LoadData(dataNr);
                //a.q_table = loadedQ;
                a.dataNr = dataNr;
            }
            agentsList.Add(a);
        }

        Reset(true);

    }

    void SetUp() {
        agentsList = new List<RunnerAgent>(numberOfAgents);
        gridGenerator.Init();
        envParameters = new EnvironmentParams() {
            state_size = gridGenerator.Cells.Length,
            action_descriptions = new List<string>() { "DoNothing", "Left", "Right" },
            action_size = 3,
            num_agents = 1,
            grid = gridGenerator.Cells
        };
    }

    void FixedUpdate() {

        for (int i = 0; i < agentsList.Count; i++) {
            if (i == agentsList.Count - 1)
                Run(agentsList[i], true);
            else
                Run(agentsList[i]);
        }
    }

    void Run(RunnerAgent agent, bool isLast = false) {
        //if (acceptingSteps == true) {
        if (agent.acceptingSteps) {
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
        yield return new WaitForSeconds(waitTime / Time.timeScale);
        foreach (RunnerAgent a in agentsList) {
            EndStep(a);
            a.acceptingSteps = true;
        }

    }

    void MiddleStep(int action, RunnerAgent agent) {
        /*
        0 - DoNothing, 
        1 - Left, 
        2 - Right,  
        */
        agent.reward = 0.01f;


        switch (action) {
            case 0:
                //DoNothing
                break;
            case 1:
                agent.RunnerPlayer.ReceiveInput(-1);
                break;
            case 2:
                agent.RunnerPlayer.ReceiveInput(1);
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

        agent.episodeReward += agent.reward;
    }

    void EndStep(RunnerAgent agent) {
        agent.SendState(new Vector2Int(
                (int)(agent.transform.position.x - RunnerManager.instance.Obstacles[agent.RunnerPlayer.CurrentObstacle].transform.position.x),
                (int)(RunnerManager.instance.Obstacles[agent.RunnerPlayer.CurrentObstacle].transform.position.z - agent.transform.position.z))
            );
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
                bestQTable = new Dictionary<Vector2Int, float[]>(agentsList[0].qTable);
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
                    bestQTable = new Dictionary<Vector2Int, float[]>(bestAgent.qTable);
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
                agentsList[i].qTable = new Dictionary<Vector2Int, float[]>(bestQTable);
            }


            if (episodeCount > 0 && episodeCount % 50 == 0 && saveData) {
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
        agent.SendState(new Vector2Int(
                (int)(agent.transform.position.x - RunnerManager.instance.Obstacles[agent.RunnerPlayer.CurrentObstacle].transform.position.x),
                (int)(RunnerManager.instance.Obstacles[agent.RunnerPlayer.CurrentObstacle].transform.position.z - agent.transform.position.z))
            );
        agent.acceptingSteps = true;

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

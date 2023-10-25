using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif
public class MultipleAgentsEnvironment : Environment
{
    public static MultipleAgentsEnvironment instance;

    [Header("Data")]
    public bool loadData;
    public bool saveData;
    public int dataNr;
    [Space]


    public float startingEpisodeReward;

    [SerializeField] GenerateGrid grid;
    public Transform finish;
    public Transform spawnPoint;
    public InternalAgent playerPrefab;
    public Vector3 detectionSize;
    public int finishedTimes = 0;

    [Header("Sensors")]
    public LayerMask obstaclesLayer;
    public LayerMask rewardsLayer;
    public float lrDistance;
    public Vector3 lrOffset;
    public float frDistance;
    [Space]

    public List<GameObject> actorObjs;
    public string[] players;

    float[][] loadedQ;
    float[][] bestQTable;
    float bestReward;
    bool agentFinished;
    InternalAgent bestAgent;

    public float[][] BestQTable => bestQTable;

    private void Awake()
    {
        instance = this;
        bestReward = float.MinValue;
        episodeCount = 0;
    }

    void Start()
    {
        SaveToExcel.CreateWorkbook();
        BeginNewGame();
    }

    public void BeginNewGame() {

        Debug.Log("Begin!!!!!!!!!!!!");
        SetUp();
        Debug.Log($"Env param : state size {envParameters.state_size}, action size {envParameters.action_size}");

        //Setting up agent
        for (int i = 0; i < numberOfAgents; i++) {
            InternalAgent a = Instantiate(playerPrefab, spawnPoint.position, spawnPoint.rotation);//new InternalAgent();
            a.rewardList = new List<float>();
            a.eMin = agentSettings.eMin;
            a.learning_rate = agentSettings.learningRate;
            a.gamma = agentSettings.gamma;
            a.SendParameters(envParameters);
            a.SetAnnealingSteps(maxSteps * 20);
            a.gameObject.name = "Agent" + i;
            if (loadData) {
                a.loadData = loadData;
                loadedQ = a.LoadData(dataNr);
                a.q_table = loadedQ;
                a.dataNr = dataNr;
            }
            agentsList.Add(a);
        }

        Reset(true);

    }

    public override void SetUp()
    {
        agentsList = new List<InternalAgent>(numberOfAgents);

        envParameters = new EnvironmentParams() {
            state_size = maxSteps + 2,
            action_descriptions = new List<string>() { "Fwd", "Bwd", "Left", "Right" },
            action_size = 4,
            num_agents = 1,
            //grid = grid.Cells
        };
    }

    void FixedUpdate()
    {

        for (int i = 0; i < agentsList.Count; i++) {
            if(i == agentsList.Count - 1)
                Run(agentsList[i], true);
            else
                Run(agentsList[i]);
        }
    }

    public override List<float> collectState(InternalAgent agent)
    {
        List<float> state = new List<float>();
        float point = agent.currentStep;
        state.Add(point);
        return state;
    }

    public override void MiddleStep(int action, InternalAgent agent)
    {
        /*
        0 - Acc, 
        1 - Brake, 
        2 - Left, 
        3 - Right, 
        4 - Do Nothing
        */
        agent.reward = 0;
        //Reset input every step

        if (action == 3) { //Turn right
            agent.player.SendInput(1);
            agent.reward -= 0.1f;
        }

        if (action == 2) { //Turn left
            agent.player.SendInput(-1);
            agent.reward -= 0.1f;
        }

        if (action == 1) // Move backward
        {
            Ray r = new Ray(agent.playerTransform.transform.position, -agent.playerTransform.transform.forward);

            if (!Physics.Raycast(r, frDistance, obstaclesLayer)) {
                agent.player.SendInput(v: -1);
            }
        }

        
        if (action == 0) //Move forward
        {
            Ray r = new Ray(agent.playerTransform.transform.position, agent.playerTransform.transform.forward);

            if (!Physics.Raycast(r, frDistance, obstaclesLayer)) {
                agent.player.SendInput(v: 1);
            }
        }

        if (Vector3.Distance(agent.player.transform.position, finish.position) < agent.previousDistance) {
            agent.reward += 0.1f;
            agent.previousDistance = Vector3.Distance(agent.player.transform.position, finish.position);
        }
        else
            agent.reward -= 0.05f;
       


        Collider[] detectedColls = Physics.OverlapSphere(agent.playerTransform.position, 1);

        if (detectedColls.Length > 0) {
            foreach (Collider coll in detectedColls) {
                if (coll.CompareTag("Finish")) {
                    agent.reward += 100;
                    agent.done = true;
                    agent.finished = true;
                    Debug.LogWarning("<color=green>Finish</color>");
                    agent.player.SendInput();
                }
                else if (coll.CompareTag("Checkpoint") && coll.name.Contains(agent.playerTransform.name)) {
                    agent.reward += 100;
                    coll.gameObject.SetActive(false);
                    Debug.LogWarning("<color=cyan>Checkpoint</color>");
                }
                else if (coll.CompareTag("Obstacle")) {
                    agent.done = true;
                    agent.reward -= 10;
                    Debug.LogWarning("<color=red>Obstacle Hit</color>");
                    agent.player.SendInput();
                }
            }
        }

        agent.episodeReward += agent.reward;

        //GameObject.Find("RTxt").GetComponent<Text>().text = "Episode Reward: " + episodeReward.ToString("F2");

    }

    public override void EndStep(InternalAgent agent) {
        agent.SendState(agent.currentStep, done);
        skippingFrames = false;
        acceptingSteps = true;
    }

    public void AddReward(int _reward, int agentIndex, bool _done = true)
    {
        agentsList[agentIndex].episodeReward += _reward;
        agentsList[agentIndex].done = _done;
        if (agentsList[agentIndex].done && saveData) {
            finishedTimes++;
            agentsList[agentIndex].SaveData(episodeCount);
            SetEValue();
        }
        if (agentsList[agentIndex].done)
            Debug.LogWarning("<color=green>Finish</color>");
        else
            Debug.LogWarning("<color=cyan>Checkpoint</color>");
    }

    public void RemoveReward(int agentIndex)
    {
        if (!agentsList[agentIndex].done) {
            agentsList[agentIndex].episodeReward = -(maxSteps + 100);
            agentsList[agentIndex].done = true;
            Debug.LogWarning("<color=red>Obstacle Hit</color>");
        }
    }

    public override void Reset(bool firstTime = false) {
        if (!firstTime) {
            foreach (InternalAgent a in agentsList)
                if (!a.done) return;
        }

        if (!firstTime) {

            if(bestAgent == null)
                bestAgent = agentsList[0];
            if (episodeCount <= 1 || bestQTable == null) {
                bestQTable = agentsList[0].q_table;
            }

            bool newBest = false;
            if (agentsList.Count > 1) {

                for (int i = 1; i < agentsList.Count; i++) {

                    if (agentsList[i].finished && agentsList[i].episodeReward > bestReward) {
                        bestAgent = agentsList[i];
                        agentFinished = true;
                        newBest = true;
                        Debug.Log($"<color=green>New best agent is {agentsList[i].player.name} with reward: {agentsList[i].episodeReward} finished</color>");
                    }
                    else if (agentsList[i].episodeReward > bestReward && !agentFinished) {
                        bestAgent = agentsList[i];
                        newBest = true;
                        if (agentsList[i] != null && agentsList[i].player != null)
                            Debug.Log($"<color=green>New best agent is {agentsList[i].player.name} with reward: {agentsList[i].episodeReward}</color>");
                    }
                }
                if (bestAgent != null && newBest) {
                    if (bestAgent.player != null)
                        Debug.Log($"<color=yellow>Best agent is : {bestAgent.player.name} with episodeReward: {bestAgent.episodeReward}</color>");
                    bestReward = bestAgent.episodeReward;
                    bestQTable = new float[bestAgent.q_table.Length][];

                    for (int j = 0; j < bestQTable.Length; j++) {
                        bestQTable[j] = new float[bestAgent.q_table[j].Length];
                        System.Array.Copy(bestAgent.q_table[j], bestQTable[j], bestAgent.q_table[j].Length);
                    }
                }
            }

        }
        base.Reset();

        for (int i = 0; i < agentsList.Count; i++) {

            if (firstTime) {
                if (agentsList[i].player == null || agentsList[i].playerTransform == null) {
                    agentsList[i].player = agentsList[i].GetComponent<PlayerController>();
                    agentsList[i].player.id = i;
                    agentsList[i].playerTransform = agentsList[i].transform;
                }
            }
            agentsList[i].ResetAgent();
            agentsList[i].previousDistance = Vector3.Distance(agentsList[i].player.transform.position, finish.position);
            agentsList[i].episodeReward = startingEpisodeReward;
            //agentsList[i].rewardList.Add(agentsList[i].episodeReward);
            //agentsList[i].reward = 0;
            //agentsList[i].currentStep = 0;
            //agentsList[i].done = false;
            agentsList[i].playerTransform.position = spawnPoint.position;
            agentsList[i].playerTransform.rotation = spawnPoint.rotation;

            //agentsList[i].player.SendInput();


            if (loadData)
                System.Array.Copy(loadedQ, agentsList[i].q_table, loadedQ.Length);
            else if (episodeCount > 0 && bestQTable != null) {  //Set best Q table

                for (int j = 0; j < bestQTable.Length; j++) {
                    if (j >= agentsList[i].q_table.Length) break;
                    System.Array.Copy(bestQTable[j], agentsList[i].q_table[j], bestQTable[j].Length);

                }
                //System.Array.Copy(bestQTable, agentsList[i].q_table, bestQTable.Length);
            }


            EndReset(agentsList[i]);
        }

        Debug.LogError("Reset");

        if (episodeCount == 150 && saveData) {
            SaveToExcel.AddSheet();
            for (int i = 0; i < agentsList.Count; i++) {
                SaveToExcel.AddData(agentsList[i].rewardList.ToArray(), i);
            }
            SaveToExcel.Save("Data/Rewards.xls");
            Debug.LogError("Data saved");
        }
    }

    public override void EndReset(InternalAgent agent) {
        agent.SendState(agent.currentStep, done);
        skippingFrames = false;
        acceptingSteps = true;
        begun = true;
        framesSinceAction = 0;

    }

#if UNITY_EDITOR

    private void OnDrawGizmos() {
        Gizmos.color = Color.blue;

        for (int i = 0; i < agentsList.Count; i++) {
            if (agentsList[i].playerTransform != null && agentsList[i].playerTransform.gameObject.activeInHierarchy) {
                Gizmos.DrawWireSphere(agentsList[i].playerTransform.position, 1);
                Handles.Label(agentsList[i].playerTransform.position, $"{agentsList[i].episodeReward}\nE:{agentsList[i].E}");
            }
        }
    }

#endif

    private void OnDrawGizmosSelected()
    {
        Gizmos.DrawWireSphere(spawnPoint.position, 1);
        Gizmos.color = new Color(0, 1f, 0, 0.25f);
        Gizmos.DrawCube(spawnPoint.position, detectionSize * 2);

        Gizmos.color = Color.blue;

        Gizmos.DrawLine(spawnPoint.position, spawnPoint.position + (spawnPoint.forward * frDistance));
        Gizmos.DrawLine(spawnPoint.position, spawnPoint.position + (-spawnPoint.forward * frDistance));

        Gizmos.DrawLine(spawnPoint.position, spawnPoint.right * lrDistance  + spawnPoint.position);
        Gizmos.DrawLine(spawnPoint.position, -spawnPoint.right * lrDistance + spawnPoint.position);

    }
}

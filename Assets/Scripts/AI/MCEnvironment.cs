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
public class MCEnvironment : MonoBehaviour
{
    public static MCEnvironment instance;

    [Header("Data")]
    public bool loadData;
    public bool saveData;
    public int dataNr;
    [Space]


    public float startingEpisodeReward;

    [SerializeField] GenerateGrid gridGen;
    public Transform finish;
    public Transform spawnPoint;
    public MCAgent playerPrefab;
    public Vector3 detectionSize;
    public int finishedTimes = 0;

    [Header("Sensors")]
    public LayerMask obstaclesLayer;
    public LayerMask rewardsLayer;
    public float lrDistance;
    public Vector3 lrOffset;
    public float frDistance;
    [Space]

    public bool done;
    public int maxSteps;
    public bool begun;
    public bool acceptingSteps;

    public int numberOfAgents;
    public bool useBestQTable;
    public DefaultAgent agentSettings;
    public List<MCAgent> agentsList;

    public int framesSinceAction;
    public bool skippingFrames;
    public int actions;
    public float waitTime;
    public int episodeCount;

    public EnvironmentParams envParameters;

    float[][] loadedQ;
    Dictionary<Vector2Int, float[]> bestQTable;
    float bestReward;
    bool agentFinished;
    MCAgent bestAgent;

    public Dictionary<Vector2Int, float[]> BestQTable => bestQTable;

    private void Awake()
    {
        instance = this;
        bestReward = float.MinValue;
        episodeCount = 0;
    }

    void Start()
    {
        
        BeginNewGame();
    }

    public void BeginNewGame() {

        Debug.Log("Begin!!!!!!!!!!!!");
        SetUp();
        Debug.Log($"Env param : state size {envParameters.state_size}, action size {envParameters.action_size}");

        //Setting up agent
        for (int i = 0; i < numberOfAgents; i++) {
            MCAgent a = Instantiate(playerPrefab, spawnPoint.position, spawnPoint.rotation);//new InternalAgent();
            a.Init(envParameters, agentSettings);
            a.SetAnnealingSteps(maxSteps * 20);
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

    public void SetUp()
    {
        agentsList = new List<MCAgent>(numberOfAgents);
        gridGen.Init();
        envParameters = new EnvironmentParams() {
            state_size = gridGen.Cells.Length,
            action_descriptions = new List<string>() { "Fwd", "Bwd", "Left", "Right" },
            action_size = 4,
            num_agents = 1,
            grid = gridGen.Cells
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

    public void Run(MCAgent agent, bool isLast = false) {
        //if (acceptingSteps == true) {
        if (agent.acceptingSteps) { 
            if (!agent.done)
                Step(agent, isLast);
            else
                Reset();
        }
    }

    public void Step(MCAgent agent, bool isLast) {
        if (isLast)
            agent.acceptingSteps = false;
        agent.currentStep += 1;

        if (agent.currentStep >= maxSteps - 1) {
            agent.done = true;
            agent.Player.SendInput();
            //return;
        }

        agent.reward = 0;
        actions = (int)agent.GetAction();
        framesSinceAction = 0;

        int sendAction = Mathf.FloorToInt(actions);
        MiddleStep(sendAction, agent);

        StartCoroutine(WaitStep());
    }

    public void SetEValue() {
        foreach (MCAgent agent in agentsList)
            agent.SetEValue(0.1f);
    }

    public IEnumerator WaitStep() {
        yield return new WaitForSeconds(waitTime);
        foreach (MCAgent a in agentsList) {
            EndStep(a);
            a.acceptingSteps = true;
        }
        skippingFrames = false;

    }

    public void MiddleStep(int action, MCAgent agent)
    {
        /*
        0 - Acc, 
        1 - Brake, 
        2 - Left, 
        3 - Right, 
        */
        agent.reward = -0.01f;
        

        switch (action) {
            case 0:
                agent.transform.position += Vector3.forward;
                break;
            case 1:
                agent.transform.position += Vector3.back;
                break;
            case 2:
                agent.transform.position += Vector3.left;
                break;
            case 3:
                agent.transform.position += Vector3.right;
                break;
            default:
                break;
        }

        if (Vector3.Distance(agent.Player.transform.position, finish.position) < agent.previousDistance) {
            agent.reward += 0.1f;
            agent.previousDistance = Vector3.Distance(agent.Player.transform.position, finish.position);
        }
        else
            agent.reward -= 0.05f;

        Collider[] detectedColls = Physics.OverlapSphere(agent.PlayerTransform.position, 1);

        if (detectedColls.Length > 0) {
            foreach (Collider coll in detectedColls) {
                if (coll.CompareTag("Finish")) {
                    agent.reward += 1;
                    agent.done = true;
                    agent.Finished = true;
                    Debug.LogWarning("<color=green>Finish</color>");
                    agent.Player.SendInput();
                }
                else if (coll.CompareTag("Checkpoint") && coll.name.Contains(agent.PlayerTransform.name)) {
                    agent.reward += 100;
                    coll.gameObject.SetActive(false);
                    Debug.LogWarning("<color=cyan>Checkpoint</color>");
                }
                else if (coll.CompareTag("Obstacle")) {
                    agent.done = true;
                    agent.reward -= 10;
                    Debug.LogWarning("<color=red>Obstacle Hit</color>");
                    agent.Player.SendInput();
                }
            }
        }

        agent.episodeReward += agent.reward;
    }

    public void EndStep(MCAgent agent) {
        agent.SendState(new Vector2Int((int)agent.transform.position.x, (int)agent.transform.position.z), done);
    }

    public void AddReward(int _reward, int agentIndex, bool _done = true)
    {
        agentsList[agentIndex].episodeReward += _reward;
        agentsList[agentIndex].done = _done;
        if (agentsList[agentIndex].done && saveData) {
            finishedTimes++;
            //agentsList[agentIndex].SaveData(episodeCount);
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

    public void Reset(bool firstTime = false) {
        if (!firstTime) {
            foreach (MCAgent a in agentsList)
                if (!a.done) return;
        }

        if (!firstTime) {

            if(bestAgent == null)
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
                        Debug.Log($"<color=green>New best agent is {agentsList[i].Player.name} with reward: {agentsList[i].episodeReward} finished</color>");
                    }
                    else if (agentsList[i].episodeReward > bestReward && !agentFinished) {
                        bestAgent = agentsList[i];
                        newBest = true;
                        if (agentsList[i] != null && agentsList[i].Player != null)
                            Debug.Log($"<color=green>New best agent is {agentsList[i].Player.name} with reward: {agentsList[i].episodeReward}</color>");
                    }
                }
                if (bestAgent != null && newBest) {
                    if (bestAgent.Player != null)
                        Debug.Log($"<color=yellow>Best agent is : {bestAgent.Player.name} with episodeReward: {bestAgent.episodeReward}</color>");
                    bestReward = bestAgent.episodeReward;
                    bestQTable = new Dictionary<Vector2Int, float[]>(bestAgent.qTable);
                }
            }

        }

        //acceptingSteps = true;
        episodeCount++;


        for (int i = 0; i < agentsList.Count; i++) {

            agentsList[i].Player.id = i;
            agentsList[i].ResetAgent();
            agentsList[i].previousDistance = Vector3.Distance(agentsList[i].Player.transform.position, finish.position);
            agentsList[i].episodeReward = startingEpisodeReward;
            agentsList[i].PlayerTransform.position = spawnPoint.position;
            agentsList[i].PlayerTransform.rotation = spawnPoint.rotation;


            /*if (loadData)
                System.Array.Copy(loadedQ, agentsList[i].q_table, loadedQ.Length);
            else*/
            if (episodeCount > 0 && bestQTable != null && useBestQTable) {  //Set best Q table
                agentsList[i].qTable = new Dictionary<Vector2Int, float[]>(bestQTable);
            }


            if (episodeCount > 0 && episodeCount % 50 == 0 && saveData)
                agentsList[i].SaveData(i, episodeCount);
            EndReset(agentsList[i]);
        }

        Debug.LogError("Reset");

        if (episodeCount > 0 && episodeCount % 100 == 0 && saveData) {
            string rewards = "";

            foreach (var item in bestAgent.rewardList) {
                rewards += item + ",";
            }

            rewards.Remove(rewards.Length - 1, 1);
            File.WriteAllText($"Data/Rewards{bestAgent.name}_{episodeCount}.csv", rewards);



        }
    }

    public void EndReset(MCAgent agent) {
        agent.SendState(new Vector2Int((int)agent.transform.position.x, (int)agent.transform.position.z), done);
        skippingFrames = false;
        agent.acceptingSteps = true;
        begun = true;
        framesSinceAction = 0;

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

using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

public class MCAgent : AgentBase
{
    [SerializeField] PlayerController player;
    [SerializeField] bool useLastActionSetIfFinished;
    [SerializeField] Transform playerTransform;
    public List<float> rewardList;
    public Dictionary<Vector2Int, float[]> qTable = new Dictionary<Vector2Int, float[]>();
    public Dictionary<Vector2Int, int> visits = new Dictionary<Vector2Int, int>();
    public float previousDistance;
    public int maxSteps;
    public float waitTime;

    Vector2Int lastVecState;

    public bool Finished { get => finished; set => finished = value; }
    public PlayerController Player => player;
    public Transform PlayerTransform => playerTransform;

    public override void Init(EnvironmentParams env, DefaultAgent agentSettings) {
        rewardList = new List<float>();
        learning_rate = agentSettings.learningRate;
        gamma = agentSettings.gamma;
        eMin = agentSettings.eMin;

        if(player == null || playerTransform == null) {
            player = GetComponent<PlayerController>();
            playerTransform = transform;
        }

        //Initialize QTable
        for (int i = 0; i < env.state_size; i++) {
            float[] actions = new float[env.action_size];

            for (int j = 0; j < actions.Length; j++)
                actions[j] = 0.0f;

            qTable.Add(env.grid[i], actions);
        }

    }

    private void FixedUpdate() {
        if (acceptingSteps) {
            if (!done)
                Step();
            else
                ResetAgent();
        }
    }

    void Step() {
        currentStep++;

        if(currentStep >= maxSteps) {
            done = true;
            player.SendInput();
        }

        reward = 0;
        MiddleStep();

        StartCoroutine(WaitStep());
    }

    void MiddleStep() {
        reward = -0.01f;


        switch (action) {
            case 0:
                transform.position += Vector3.forward;
                break;
            case 1:
                transform.position += Vector3.back;
                break;
            case 2:
                transform.position += Vector3.left;
                break;
            case 3:
                transform.position += Vector3.right;
                break;
            default:
                break;
        }

        if (Vector3.Distance(Player.transform.position, MCEnvironment.instance.finish.position) < previousDistance) {
            reward += 0.1f;
            previousDistance = Vector3.Distance(Player.transform.position, MCEnvironment.instance.finish.position);
        }
        else
            reward -= 0.05f;

        Collider[] detectedColls = Physics.OverlapSphere(PlayerTransform.position, 1);

        if (detectedColls.Length > 0) {
            foreach (Collider coll in detectedColls) {
                if (coll.CompareTag("Finish")) {
                    reward += 1;
                    done = true;
                    Finished = true;
                    Debug.LogWarning("<color=green>Finish</color>");
                    Player.SendInput();
                }
                else if (coll.CompareTag("Checkpoint") && coll.name.Contains(playerTransform.name)) {
                    reward += 100;
                    coll.gameObject.SetActive(false);
                    Debug.LogWarning("<color=cyan>Checkpoint</color>");
                }
                else if (coll.CompareTag("Obstacle")) {
                    done = true;
                    reward -= 10;
                    Debug.LogWarning("<color=red>Obstacle Hit</color>");
                    Player.SendInput();
                }
            }
        }

        episodeReward += reward;

    }

    public IEnumerator WaitStep() {
        yield return new WaitForSeconds(waitTime);
        EndStep();
        acceptingSteps = true;

    }

    public void EndStep() {
        SendState(new Vector2Int((int)transform.position.x, (int)transform.position.z), done);
    }

    public override void SetEValue(float val) {
        if (e - val > eMin)
            e = val;
    }

    public override object GetAction() {

        float maxValue = float.MinValue;
        int maxValueIndex = -1;

        for (int i = 0; i < qTable[lastVecState].Length; i++) {

            if (qTable[lastVecState][i] > maxValue) {
                maxValue = qTable[lastVecState][i];
                maxValueIndex = i;
            }

        }

        action = maxValueIndex;

        if ((useLastActionSetIfFinished && e <= eMin) || loadData)
            return new float[1] { action };

        if (Random.Range(0f, 1f) < e) {
            action = Random.Range(0, 4);
        }

        if (e > eMin) {
            e = e - ((1f - eMin) / annealingSteps);
        }
        

        return action;
    }

    public override void SendState(Vector2Int state, bool done) {

        Vector2Int nextState = state;
        float nextStateMax = float.MinValue;

        if (useLastActionSetIfFinished && Finished) {
            lastVecState = nextState;
            return;
        }

        if (!qTable.ContainsKey(nextState)) return;

        foreach (var item in qTable[nextState])
            if (item > nextStateMax) nextStateMax = item;

        if (action != -1 && !loadData) {
            if (done == true)
                qTable[lastVecState][action] += learning_rate * (reward - qTable[lastVecState][action]);
            else
                qTable[lastVecState][action] += learning_rate * (reward + gamma * nextStateMax - qTable[lastVecState][action]);
        }
        lastVecState = nextState;
    }

    public override void ResetAgent() {
        rewardList.Add(episodeReward);
        reward = 0;
        currentStep = 0;
        done = false;
        Player.SendInput();
    }

    public override void SetAnnealingSteps(int val) {
        annealingSteps = val;
    }

    public void SaveData(int agentID, int epCount = 0) {
        string text = "";
        foreach (var item in qTable) {

            for (int i = 0; i < item.Value.Length; i++)
                text += $"{item.Value[i]}{(i != item.Value.Length - 1 ? "," : "")}";
            text += "\n";
        }

        File.WriteAllText($"Data/Data{agentID}_{epCount}.csv", text);

        string rewards = "";

        foreach (var item in rewardList) {
            rewards += item + ",";
        }

        rewards.Remove(rewards.Length - 1, 1);
        File.WriteAllText($"Data/Rewards{agentID}_{epCount}.csv", rewards);

    }
}

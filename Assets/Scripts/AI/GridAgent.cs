using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

public class GridAgent : AgentBase
{

    public List<float> rewardList;
    public Dictionary<Vector2Int, float[]> qTable = new Dictionary<Vector2Int, float[]>();
    public float previousDistance;

    Vector2Int lastVecState;
    int actions;

    public bool Finished { get => finished; set => finished = value; }
    public PlayerController Player => player;
    public Transform PlayerTransform => playerTransform;

    public override void Init(EnvironmentParams env, DefaultAgent agentSettings) {
        rewardList = new List<float>();
        learning_rate = agentSettings.learningRate;
        gamma = agentSettings.gamma;
        eMin = agentSettings.eMin;
        actions = env.action_size;

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

    public override void SetEValue(float val) {
        if (e - val > eMin)
            e = val;
    }

    public override object GetAction() {

        float maxValue = float.MinValue;
        int maxValueAction = -1;

        for (int i = 0; i < qTable[lastVecState].Length; i++) {

            if (qTable[lastVecState][i] > maxValue) {
                maxValue = qTable[lastVecState][i];
                maxValueAction = i;
            }

        }

        action = maxValueAction;

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

        float nextStateMax = float.MinValue;

        if (useLastActionSetIfFinished && Finished) {
            lastVecState = state;
            return;
        }

        if (!qTable.ContainsKey(state)) {
            qTable.Add(state, new float[actions]);
            return;
        }

        foreach (var item in qTable[state])
            if (item > nextStateMax) nextStateMax = item;

        if (action != -1 && !loadData) {
            if (done == true)
                qTable[lastVecState][action] += learning_rate * (reward - qTable[lastVecState][action]);
            else
                qTable[lastVecState][action] += learning_rate * (reward + gamma * nextStateMax - qTable[lastVecState][action]);
        }
        lastVecState = state;
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

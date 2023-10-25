using System.Collections.Generic;
using UnityEngine;

public class GridAgentDoubleQ : GridAgent
{

    public Dictionary<Vector2Int, float[]> qTableB = new Dictionary<Vector2Int, float[]>();

    Vector2Int lastVecState;
    int actions;

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

        //Initialize QTables
        for (int i = 0; i < env.state_size; i++) {
            float[] actions = new float[env.action_size];

            for (int j = 0; j < actions.Length; j++)
                actions[j] = 0.0f;

            qTable.Add(env.grid[i], actions);
            qTableB.Add(env.grid[i], actions);
        }
    }

    public override object GetAction() {

        float maxValue = float.MinValue;
        int maxValueAction = -1;
        Dictionary<Vector2Int, float[]> selectedQTable;

        if (Random.value < 0.5f)
            selectedQTable = qTable;
        else
            selectedQTable = qTableB;


        for (int i = 0; i < selectedQTable[lastVecState].Length; i++) {

            if (selectedQTable[lastVecState][i] > maxValue) {
                maxValue = selectedQTable[lastVecState][i];
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

        Dictionary<Vector2Int, float[]> selectedQTable;

        if (Random.value < 0.5f)
            selectedQTable = qTable;
        else
            selectedQTable = qTableB;

        if (!selectedQTable.ContainsKey(state)) {
            selectedQTable.Add(state, new float[actions]);
            return;
        }

        foreach (var item in qTable[state])
            if (item > nextStateMax) nextStateMax = item;

        if (action != -1 && !loadData) {
            if (done == true)
                selectedQTable[lastVecState][action] += learning_rate * (reward - selectedQTable[lastVecState][action]);
            else
                selectedQTable[lastVecState][action] += learning_rate * (reward + gamma * nextStateMax - selectedQTable[lastVecState][action]);
        }
        lastVecState = state;
    }

}

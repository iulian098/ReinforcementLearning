using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class RunnerAgentDoubleQ : RunnerAgent {

    public Dictionary<Vector2Int, float[]> qTableA = new Dictionary<Vector2Int, float[]>();
    public Dictionary<Vector2Int, float[]> qTableB = new Dictionary<Vector2Int, float[]>();

    public override void Init(EnvironmentParams env, DefaultAgent agentSettings) {
        rewardList = new List<int>();
        learning_rate = agentSettings.learningRate;
        gamma = agentSettings.gamma;
        eMin = agentSettings.eMin;
        actions = env.action_size;

        //Initialize QTable
        for (int i = 0; i < env.state_size; i++) {
            float[] actions = new float[env.action_size];

            for (int j = 0; j < actions.Length; j++)
                actions[j] = 0.0f;

            qTableA.Add(env.grid[i], actions);
            qTableB.Add(env.grid[i], actions);
        }

        lastVecState = new Vector2Int(
            (int)RunnerManager.instance.Obstacles[0].transform.position.x,
            (int)RunnerManager.instance.Obstacles[0].transform.position.z
            );
        finishTransform = GameObject.FindGameObjectWithTag("Finish").transform;
    }

    public override object GetAction() {

        float maxValue = float.MinValue;
        int maxValueAction = -1;

        float randVal = Random.value;
        Dictionary<Vector2Int, float[]> selectedTable = randVal < 0.5f ? qTableA : qTableB;

        for (int i = 0; i < selectedTable[lastVecState].Length; i++) {

            if (selectedTable[lastVecState][i] > maxValue) {
                maxValue = selectedTable[lastVecState][i];
                maxValueAction = i;
            }

        }

        action = maxValueAction;

        if ((useLastActionSetIfFinished && e <= eMin) || loadData)
            return new float[1] { action };

        if (Random.Range(0f, 1f) < e) {
            action = Random.Range(0, actions);
        }

        if (e > eMin) {
            e = e - ((1f - eMin) / annealingSteps);
        }


        return action;
    }

    public override void SendState(Vector2Int state) {

        float nextStateMax = float.MinValue;
        float randVal = Random.value;

        Dictionary<Vector2Int, float[]> selectedTable = randVal < 0.5f ? qTableA : qTableB;
        Dictionary<Vector2Int, float[]> otherTable = randVal < 0.5f ? qTableB : qTableA;

        /*if (useLastActionSetIfFinished && Finished) {
            lastVecState = state;
            return;
        }*/

        if (!selectedTable.ContainsKey(state))
            selectedTable.Add(state, new float[actions]);
        if (!otherTable.ContainsKey(state))
            otherTable.Add(state, new float[actions]);



        foreach (var item in otherTable[state])
            if (item > nextStateMax) nextStateMax = item;

        if (action != -1 && !loadData) {
            if (done == true)
                selectedTable[lastVecState][action] += learning_rate * (reward - selectedTable[lastVecState][action]);
            else
                selectedTable[lastVecState][action] += learning_rate * (reward + gamma * nextStateMax - selectedTable[lastVecState][action]);
        }
        lastVecState = state;
    }

    private void OnDrawGizmosSelected() {
        Gizmos.DrawSphere(PlayerTransform.position, 0.8f);
    }
}


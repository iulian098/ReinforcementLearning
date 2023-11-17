using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Runner.RL {

    public class RunnerAgentSARSA : RunnerBaseAgent {
        Dictionary<RunnerState, float[]> qTable = new Dictionary<RunnerState, float[]>();
        RunnerState previousState;
        int previousAction;

        public override void Init(RunnerEnvironment.RunnerEnvironmentParams env, DefaultAgent agentSettings) {
            learning_rate = agentSettings.learningRate;
            gamma = agentSettings.gamma;
            eMin = agentSettings.eMin;
            actions = env.action_size;

            //Initialize QTable
            for (int i = 0; i < env.state_size; i++) {
                float[] actions = new float[env.action_size];

                for (int j = 0; j < actions.Length; j++)
                    actions[j] = 0.0f;

                qTable.Add(env.states[i], actions);
            }

            Obstacle firstObstacle = RunnerManager.Instance.Obstacles[0];

            previousState = lastState = new RunnerState() {
                XDistance = (int)(transform.position.x - firstObstacle.transform.position.x),
                YDistance = (int)(firstObstacle.transform.position.z - transform.position.z),
                ObstacleType = firstObstacle.ObstacleType
            };
            previousAction = action = 0;
            finishTransform = GameObject.FindGameObjectWithTag("Finish").transform;

        }

        public override object GetAction() {
            float maxValue = float.MinValue;
            int maxValueAction = -1;

            for (int i = 0; i < qTable[lastState].Length; i++) {
                if (qTable[lastState][i] > maxValue) {
                    maxValue = qTable[lastState][i];
                    maxValueAction = i;
                }
            }

            action = maxValueAction;

            if ((useLastActionSetIfFinished && e <= eMin) || loadData)
                return action;

            if (Random.Range(0f, 1f) < e)
                action = Random.Range(0, actions);

            if (e > eMin)
                e = e - ((1f - eMin) / annealingSteps);

            return action;
        }

        public override void SendState(RunnerState state) {

            //Prev state -> Current state

            float nextStateMax = float.MinValue;

            if (!qTable.ContainsKey(state)) {
                qTable.Add(state, new float[actions]);
                return;
            }

            foreach (var item in qTable[state])
                if (item > nextStateMax) nextStateMax = item;

            if (action != -1 && !loadData) {
                /*if (done)
                    qTable[lastState][action] += learning_rate * (reward - qTable[lastState][action]);
                else
                    qTable[lastState][action] += learning_rate * (reward + gamma * nextStateMax - qTable[lastState][action]);*/

                qTable[previousState][previousAction] += learning_rate * (reward + gamma * qTable[state][action] - qTable[previousState][previousAction]);
            }
            previousAction = action;
            previousState = lastState;
            lastState = state;

        }

        public override void ResetAgent() {
            base.ResetAgent();
            Obstacle firstObstacle = RunnerManager.Instance.Obstacles[0];

            previousState = lastState = new RunnerState() {
                XDistance = (int)(transform.position.x - firstObstacle.transform.position.x),
                YDistance = (int)(firstObstacle.transform.position.z - transform.position.z),
                ObstacleType = firstObstacle.ObstacleType
            };

            previousAction = 0;

        }

        public override void SaveData(int agentID, int epCount = 0) {
            
        }

    }

}

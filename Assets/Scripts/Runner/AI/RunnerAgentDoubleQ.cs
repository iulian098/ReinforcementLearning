using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace Runner.RL {

    public class RunnerAgentDoubleQ : RunnerBaseAgent {

        public Dictionary<RunnerState, float[]> qTableA = new Dictionary<RunnerState, float[]>();
        public Dictionary<RunnerState, float[]> qTableB = new Dictionary<RunnerState, float[]>();

        public override void Init(RunnerEnvironment.RunnerEnvironmentParams env, DefaultAgent agentSettings) {
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

                qTableA.Add(env.states[i], actions);
                qTableB.Add(env.states[i], actions);
            }

            lastState = new RunnerState() {
                XDistance = (int)RunnerManager.Instance.Obstacles[0].transform.position.x,
                YDistance = (int)RunnerManager.Instance.Obstacles[0].transform.position.z,
                ObstacleType = RunnerManager.Instance.Obstacles[0].ObstacleType
            };

            finishTransform = GameObject.FindGameObjectWithTag("Finish").transform;
        }

        public override object GetAction() {

            float maxValue = float.MinValue;
            int maxValueAction = -1;

            float randVal = Random.value;
            Dictionary<RunnerState, float[]> selectedTable = randVal < 0.5f ? qTableA : qTableB;

            for (int i = 0; i < selectedTable[lastState].Length; i++) {

                if (selectedTable[lastState][i] > maxValue) {
                    maxValue = selectedTable[lastState][i];
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

            float nextStateMax = float.MinValue;
            float randVal = Random.value;

            Dictionary<RunnerState, float[]> selectedTable = randVal < 0.5f ? qTableA : qTableB;
            Dictionary<RunnerState, float[]> otherTable = randVal < 0.5f ? qTableB : qTableA;

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

            if (action != -1) {
                if (done == true)
                    selectedTable[lastState][action] += learning_rate * (reward - selectedTable[lastState][action]);
                else
                    selectedTable[lastState][action] += learning_rate * (reward + gamma * nextStateMax - selectedTable[lastState][action]);
            }
            lastState = state;
        }

        protected override void Update() {
            if (done) return;
            Collider[] detectedColls = Physics.OverlapCapsule(playerTransform.position + new Vector3(0, 0.5f, 0), playerTransform.position + new Vector3(0, runnerPlayer.Coll.height - 0.5f, 0), runnerPlayer.Coll.radius);

            if (transform.position.z > finishTransform.position.z) {
                reward = 1;
                episodeReward += reward;
                done = true;
                Finished = true;
                RewardList.Add((int)RunnerManager.Instance.Score);
                RunnerPlayer.Stopped = true;

                Debug.LogWarning("<color=green>Finish</color>");

            }

            if (detectedColls.Length > 0) {
                foreach (Collider coll in detectedColls) {
                    if (coll.CompareTag("Finish")) {
                        reward = 1;
                        episodeReward += reward;
                        done = true;
                        Finished = true;
                        RewardList.Add((int)RunnerManager.Instance.Score);
                        RunnerPlayer.Stopped = true;
                        RunnerPlayer.AcceptingSteps = true;
                        Debug.LogWarning("<color=green>Finish</color>");
                    }
                    else if (coll.CompareTag("Obstacle")) {
                        done = true;
                        reward = -0.5f;
                        episodeReward += reward;
                        RewardList.Add((int)RunnerManager.Instance.Score);
                        RunnerPlayer.Stopped = true;
                        RunnerPlayer.AcceptingSteps = true;
                        Debug.LogWarning("<color=red>Obstacle Hit</color>");
                    }
                    else if (coll.CompareTag("Checkpoint") && !checkpointsReached.Contains(coll)) {
                        reward = 0.05f;
                        episodeReward += reward;
                        checkpointsReached.Add(coll);
                        Debug.LogWarning("<color=yellow>Checkpoint Hit</color>");
                    }
                }
            }
        }

        public override void SaveData(int agentID, int epCount = 0) {
            Debug.Log("Save data not implemented");
        }
    }

}


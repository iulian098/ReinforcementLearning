using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
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

            //Initial state
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
            float nextStateMax = float.MinValue;

            if (!qTable.ContainsKey(state)) {
                qTable.Add(state, new float[actions]);
                return;
            }

            foreach (var item in qTable[state])
                if (item > nextStateMax) nextStateMax = item;

            if (action != -1 && !loadData) {
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

        public override void SaveData() {

            string text = "";
            foreach (var item in qTable) {
                text += item.Key.ToString() + ",";
                for (int i = 0; i < item.Value.Length; i++)
                    text += $"{item.Value[i]}{(i != item.Value.Length - 1 ? "," : "")}";
                text += "\n";
            }

            File.WriteAllText($"Data/SARSA_Data.csv", text);

        }

        public async override Task LoadData() { 
            int bufferSize = 128;
            Dictionary<RunnerState, float[]> loadedQ = new Dictionary<RunnerState, float[]>();

            using (FileStream fs = File.OpenRead($"Data/SARSA_Data.csv")) {

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

            qTable = loadedQ;
        }

    }

}

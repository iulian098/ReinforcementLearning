using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
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

            Obstacle firstObstacle = RunnerManager.Instance.Obstacles[0];

            //Initial state
            lastState = new RunnerState() {
                XDistance = (int)(transform.position.x - firstObstacle.transform.position.x),
                YDistance = (int)(firstObstacle.transform.position.z - transform.position.z),
                ObstacleType = firstObstacle.ObstacleType
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

            if (!selectedTable.ContainsKey(state))
                selectedTable.Add(state, new float[actions]);
            if (!otherTable.ContainsKey(state))
                otherTable.Add(state, new float[actions]);



            foreach (var item in otherTable[state])
                if (item > nextStateMax) nextStateMax = item;

            if (action != -1) {
                if (done)
                    selectedTable[lastState][action] += learning_rate * (reward - selectedTable[lastState][action]);
                else
                    selectedTable[lastState][action] += learning_rate * (reward + gamma * nextStateMax - selectedTable[lastState][action]);
            }
            lastState = state;
        }

        public override void ResetAgent() {
            base.ResetAgent();

            Obstacle firstObstacle = RunnerManager.Instance.Obstacles[0];

            lastState = new RunnerState() {
                XDistance = (int)(transform.position.x - firstObstacle.transform.position.x),
                YDistance = (int)(firstObstacle.transform.position.z - transform.position.z),
                ObstacleType = firstObstacle.ObstacleType
            };

        }

        public override void SaveData() {
            string text = "";
            foreach (var item in qTableA) {
                text += item.Key.ToString() + ",";
                for (int i = 0; i < item.Value.Length; i++)
                    text += $"{item.Value[i]}{(i != item.Value.Length - 1 ? "," : "")}";
                text += "\n";
            }

            File.WriteAllText($"Data/DQ_Data_A.csv", text);

            string textB = "";
            foreach (var item in qTableB) {
                textB += item.Key.ToString() + ",";
                for (int i = 0; i < item.Value.Length; i++)
                    textB += $"{item.Value[i]}{(i != item.Value.Length - 1 ? "," : "")}";
                textB += "\n";
            }

            File.WriteAllText($"Data/DQ_Data_B.csv", textB);

        }

        public async override Task LoadData() {
            qTableA = await LoadTable("DQ_Data_A");
            qTableB = await LoadTable("DQ_Data_B");

        }

        async Task<Dictionary<RunnerState, float[]>> LoadTable(string fileName) {
            int bufferSize = 128;
            Dictionary<RunnerState, float[]> loadedQ = new Dictionary<RunnerState, float[]>();

            using (FileStream fs = File.OpenRead(Application.dataPath + $"Data/{fileName}.csv")) {

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

            return loadedQ;

        }
    }

}


using StateMachine.Player;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Runner.RL {

    public class RunnerAgent : RunnerBaseAgent {
        public Dictionary<RunnerState, float[]> qTable = new Dictionary<RunnerState, float[]>();

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

            if (useLastActionSetIfFinished && Finished) {
                lastState = state;
                return;
            }

            if (!qTable.ContainsKey(state)) {
                qTable.Add(state, new float[actions]);
                return;
            }

            foreach (var item in qTable[state])
                if (item > nextStateMax) nextStateMax = item;

            if (action != -1 && !loadData) {
                if (done)
                    qTable[lastState][action] += learning_rate * (reward - qTable[lastState][action]);
                else
                    qTable[lastState][action] += learning_rate * (reward + gamma * nextStateMax - qTable[lastState][action]);
            }
            lastState = state;
        }

        public override void SaveData(int agentID, int epCount = 0) {
            string text = "";
            foreach (var item in qTable) {
                text += item.Key.ToString() + ",";
                for (int i = 0; i < item.Value.Length; i++)
                    text += $"{item.Value[i]}{(i != item.Value.Length - 1 ? "," : "")}";
                text += "\n";
            }

            File.WriteAllText($"Data/Data{agentID}_{epCount}.csv", text);

            string rewards = "";

            foreach (var item in RewardList) {
                rewards += item + ",";
            }

            rewards.Remove(rewards.Length - 1, 1);
            File.WriteAllText($"Data/Rewards{agentID}_{epCount}.csv", rewards);

        }

        public async override Task LoadData(string fileName) {
            int bufferSize = 128;
            Dictionary<RunnerState, float[]> loadedQ = new Dictionary<RunnerState, float[]>();

            using (FileStream fs = File.OpenRead($"Data/{fileName}.csv")) {

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

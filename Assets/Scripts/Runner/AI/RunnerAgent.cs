using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace Runner.RL {

    public class RunnerAgent : MonoBehaviour {
        [SerializeField] protected RunnerPlayer runnerPlayer;
        [SerializeField] protected bool useLastActionSetIfFinished;
        [SerializeField] protected int annealingSteps = 2000; // Number of steps to lower e to eMin.

        public Dictionary<RunnerState, float[]> qTable = new Dictionary<RunnerState, float[]>();
        public bool acceptingSteps;
        public bool done;
        public bool loadData;
        public int dataNr;
        public int currentStep;
        public float reward;
        public float episodeReward;

        protected RunnerState lastState;
        protected int actions;
        protected bool finished;
        protected List<int> rewardList;
        protected float learning_rate = 0.05f; // The rate at which to update the value estimates given a reward. Default: 0.5f
        protected float gamma = 0.99f; // Discount factor for calculating Q-target. Defualt = 0.99f
        protected float eMin = 0.1f; // Lower bound of epsilon.
        protected float e = 1;  // Initial epsilon value for random action selection.
        protected int action = -1;
        protected List<Collider> checkpointsReached = new List<Collider>();

        public bool Finished { get => finished; set => finished = value; }
        public float E => e;
        public RunnerPlayer RunnerPlayer => runnerPlayer;
        public Transform PlayerTransform => playerTransform;
        public List<int> RewardList => rewardList;

        protected Transform playerTransform;
        protected Transform finishTransform;

        private void Awake() {
            playerTransform = transform;
            rewardList = new List<int>();
        }

        public virtual void Init(RunnerEnvironment.RunnerEnvironmentParams env, DefaultAgent agentSettings) {
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
                XDistance = (int)RunnerManager.instance.Obstacles[0].transform.position.x,
                YDistance = (int)RunnerManager.instance.Obstacles[0].transform.position.z,
                ObstacleType = RunnerManager.instance.Obstacles[0].ObstacleType
            };
            finishTransform = GameObject.FindGameObjectWithTag("Finish").transform;
        }

        protected virtual void Update() {
            if (done) return;
            Collider[] detectedColls = Physics.OverlapCapsule(playerTransform.position + new Vector3(0, 0.5f, 0), playerTransform.position + new Vector3(0, runnerPlayer.Coll.height - 0.5f, 0), runnerPlayer.Coll.radius);

            if (transform.position.z > finishTransform.position.z) {
                reward = 1;
                episodeReward += reward;
                done = true;
                Finished = true;
                RewardList.Add((int)RunnerManager.instance.Score);
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
                        RewardList.Add((int)RunnerManager.instance.Score);
                        RunnerPlayer.Stopped = true;

                        Debug.LogWarning("<color=green>Finish</color>");
                    }
                    else if (coll.CompareTag("Obstacle")) {
                        done = true;
                        reward = -10;
                        episodeReward += reward;
                        RewardList.Add((int)RunnerManager.instance.Score);
                        RunnerPlayer.Stopped = true;
                        Debug.LogWarning("<color=red>Obstacle Hit</color>");
                    }
                    else if (coll.CompareTag("Checkpoint") && !checkpointsReached.Contains(coll)) {
                        reward = 0.02f;
                        episodeReward += reward;
                        checkpointsReached.Add(coll);
                        Debug.LogWarning("<color=yellow>Checkpoint Hit</color>");
                    }
                }
            }
        }

        public void SetEValue(float val) {
            if (e - val > eMin)
                e = val;
        }

        public virtual object GetAction() {

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
                return new float[1] { action };

            if (Random.Range(0f, 1f) < e) {
                action = Random.Range(0, actions);
            }

            if (e > eMin) {
                e = e - ((1f - eMin) / annealingSteps);
            }


            return action;
        }

        public virtual void SendState(RunnerState state) {

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
                if (done == true)
                    qTable[lastState][action] += learning_rate * (reward - qTable[lastState][action]);
                else
                    qTable[lastState][action] += learning_rate * (reward + gamma * nextStateMax - qTable[lastState][action]);
            }
            lastState = state;
        }

        /// <summary>
        /// Reset agent data
        /// </summary>
        public void ResetAgent() {
            RunnerPlayer.ResetData();
            checkpointsReached.Clear();
            reward = 0;
            currentStep = 0;
            done = false;
        }

        public void SetAnnealingSteps(int val) {
            annealingSteps = val;
        }

        public void SaveData(int agentID, int epCount = 0) {
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

    }

}

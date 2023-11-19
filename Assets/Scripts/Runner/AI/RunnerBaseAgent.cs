using StateMachine.Player;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using UnityEngine;

namespace Runner.RL {

    public abstract class RunnerBaseAgent : MonoBehaviour {
        [SerializeField] protected PlayerStateMachine runnerPlayer;
        [SerializeField] protected bool useLastActionSetIfFinished;
        [SerializeField] protected int annealingSteps = 2000; // Number of steps to lower e to eMin.

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
        public PlayerStateMachine RunnerPlayer => runnerPlayer;
        public Transform PlayerTransform => playerTransform;
        public List<int> RewardList => rewardList;

        protected Transform playerTransform;
        protected Transform finishTransform;

        protected void Awake() {
            playerTransform = transform;
            rewardList = new List<int>();
        }

        public abstract void Init(RunnerEnvironment.RunnerEnvironmentParams env, DefaultAgent agentSettings);

        protected virtual void Update() {
            if (done) return;
            Collider[] detectedColls = Physics.OverlapCapsule(playerTransform.position + new Vector3(0, 0.5f, 0), playerTransform.position + new Vector3(0, runnerPlayer.Coll.height - 0.5f, 0), runnerPlayer.Coll.radius);

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
                        Debug.Log("<color=green>Finish</color>");
                    }
                    else if (coll.CompareTag("Obstacle")) {
                        done = true;
                        reward = -0.5f;
                        episodeReward += reward;
                        RewardList.Add((int)RunnerManager.Instance.Score);
                        RunnerPlayer.Stopped = true;
                        RunnerPlayer.AcceptingSteps = true;
                        Debug.Log("<color=red>Obstacle Hit</color>");
                    }
                    else if (coll.CompareTag("Checkpoint") && !checkpointsReached.Contains(coll)) {
                        reward = 0.05f;
                        episodeReward += reward;
                        checkpointsReached.Add(coll);
                        Debug.Log("<color=yellow>Checkpoint Hit</color>");
                    }
                }
            }

        }

        public void SetEValue(float val) {
            if (e - val > eMin)
                e = val;
        }

        public abstract object GetAction();

        public abstract void SendState(RunnerState state);

        public virtual void ResetAgent() {
            RunnerPlayer.ResetData();
            checkpointsReached.Clear();
            reward = 0;
            currentStep = 0;
            done = false;
        }

        public abstract void SaveData();

        public virtual Task LoadData() { return null; }
    }
}

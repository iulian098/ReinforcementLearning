using Unity.MLAgents.Policies;
using UnityEngine;

namespace Unity.MLAgents {

    public class AgentsManager : MonoBehaviour {
        public Academy academy;
        [SerializeField] RaceManager raceManager;
        [SerializeField] Vehicle_Agent[] vehicleAgents;

        int selfPlayAgentIndex = 0;

        private void Awake() {

            academy = Academy.Instance;
            //vehicleAgents[selfPlayAgentIndex].GetComponent<BehaviorParameters>().TeamId = 1;
            academy.OnEnvironmentReset += Academy_OnEnvironmentReset;
        }

        private void Academy_OnEnvironmentReset() {
            Debug.Log("OnReset");

            /*vehicleAgents[selfPlayAgentIndex].GetComponent<BehaviorParameters>().TeamId = 0;
            selfPlayAgentIndex = (selfPlayAgentIndex + 1) % vehicleAgents.Length;
            vehicleAgents[selfPlayAgentIndex].GetComponent<BehaviorParameters>().TeamId = 1;

            raceManager.StopUpdate = true;

            foreach (var agent in vehicleAgents)
                agent.ResetVehicleData();

            raceManager.UpdateVehiclesPlacements(false);

            raceManager.StopUpdate = false;*/
        }
    }
}

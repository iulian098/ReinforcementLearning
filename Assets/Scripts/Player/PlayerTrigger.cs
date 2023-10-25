using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerTrigger : MonoBehaviour
{
    public int agentIndex;

    public PlayerController player;

    private void Start() {
        agentIndex = player.id;
    }

    private void OnTriggerEnter(Collider other) {
        if (other.CompareTag("Finish"))
            MultipleAgentsEnvironment.instance.AddReward(100, agentIndex);
        else if (other.CompareTag("Checkpoint") && other.name.Contains(player.name)) {
            MultipleAgentsEnvironment.instance.AddReward(10, agentIndex, false);
            other.gameObject.SetActive(false);
        }
        else if (other.CompareTag("Obstacle"))
            MultipleAgentsEnvironment.instance.RemoveReward(agentIndex);

    }

    private void OnTriggerStay(Collider other) {
    }
}

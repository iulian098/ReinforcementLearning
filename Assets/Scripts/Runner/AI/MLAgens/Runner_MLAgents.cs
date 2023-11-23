using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;
using StateMachine.Player;
using Runner;

public class Runner_MLAgens : Agent
{
    [SerializeField] PlayerStateMachine player;
    [SerializeField] Transform spawnPoint;

    public override void OnEpisodeBegin() {
        
        player.transform.localPosition = spawnPoint.localPosition;
        player.SwitchState(new MovementState(player));
    }

    public override void CollectObservations(VectorSensor sensor) {
        Obstacle currentObstacle = RunnerManager.Instance.Obstacles[player.CurrentObstacle];
        sensor.AddObservation((int)(player.transform.localPosition.x - currentObstacle.transform.localPosition.x));
        sensor.AddObservation((int)(currentObstacle.transform.localPosition.z - player.transform.localPosition.z));
        sensor.AddObservation((int)currentObstacle.ObstacleType);
    }

    public override void OnActionReceived(ActionBuffers actions) {
        Debug.Log($"[Action] {actions.DiscreteActions[0]}");
        if (!player.AcceptingSteps) return;
        int action = actions.DiscreteActions[0];
        switch (action) {
            case 0:
                //DoNothing
                break;
            case 1:
                player.SendInput(new StateMachine.Player.PlayerInput() {
                    direction = new Vector2(-1, 0)
                });
                break;
            case 2:
                player.SendInput(new StateMachine.Player.PlayerInput() {
                    direction = new Vector2(1, 0)
                });
                break;
            case 3:
                player.SwitchState(new RollState(player));
                break;
            case 4:
                player.SwitchState(new JumpState(player));
                break;
            default:
                break;
        }



        Obstacle currentObstacle = RunnerManager.Instance.Obstacles[player.CurrentObstacle];

        if (currentObstacle.ObstacleType == ObstacleType.Wall && action >= 3) {
            //agent.reward = -0.01f;
            AddReward(-0.001f);
        }
        else if (currentObstacle.ObstacleType == ObstacleType.Slide && (action != 0 && action != 3)) {
            //agent.reward = -0.01f;
            AddReward(-0.001f);
        }
        else if (currentObstacle.ObstacleType == ObstacleType.Jump && (action != 0 && action != 4)) {
            //agent.reward = -0.01f;
            AddReward(-0.001f);
        }
        else if (currentObstacle.ObstacleType == ObstacleType.Wall && (action == 1 || action == 2)) {
            int currentDistance = (int)Mathf.Abs(player.transform.position.x - currentObstacle.transform.position.x);

            if (currentDistance > player.LastXDistance)
                AddReward(-0.001f);
            else if (currentDistance < player.LastXDistance)
                player.LastXDistance = currentDistance;
        }

    }

    private void OnCollisionEnter(Collision collision) {
        if (collision.collider.CompareTag("Finish")) {
            SetReward(1f);
            EndEpisode();
        }else if (collision.collider.CompareTag("Obstacle")) {
            SetReward(-1f);
            EndEpisode();
        }
    }

    private void OnTriggerEnter(Collider other) {
        if (other.CompareTag("Checkpoint")) {
            AddReward(0.01f);
        }
    }
}

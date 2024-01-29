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
        player.CurrentObstacle = 0;
        player.CurrentCoin = 0;
        player.RunnerManager.EnableCoins();
    }

    public override void CollectObservations(VectorSensor sensor) {
        Obstacle currentObstacle = player.RunnerManager.Obstacles[player.CurrentObstacle];
        GameObject currentCoin = player.RunnerManager.Coins[player.CurrentCoin];
        sensor.AddObservation((int)currentObstacle.ObstacleType);
        sensor.AddObservation((int)(currentObstacle.transform.localPosition.x));
        sensor.AddObservation((int)(currentObstacle.transform.localPosition.z - player.transform.localPosition.z));
        sensor.AddObservation((int)(currentCoin.transform.localPosition.x));
        sensor.AddObservation((int)(currentCoin.transform.localPosition.z - player.transform.localPosition.z));
    }

    public override void OnActionReceived(ActionBuffers actions) {
        Debug.Log($"[Action] {actions.DiscreteActions[0]}");
        if (!player.AcceptingSteps) return;
        int action = actions.DiscreteActions[0];
        float xInput = actions.ContinuousActions[0];
        player.SendInput(new StateMachine.Player.PlayerInput() {
            direction = new Vector2(xInput, 0)
        });
        switch (action) {
            case 0:
                //DoNothing
                break;
            /*case 1:
                player.SendInput(new StateMachine.Player.PlayerInput() {
                    direction = new Vector2(xInput, 0)
                });

                break;
            case 2:
                player.SendInput(new StateMachine.Player.PlayerInput() {
                    direction = new Vector2(xInput, 0)
                });
                break;*/
            case 1:
                player.SwitchState(new RollState(player));
                break;
            case 2:
                player.SwitchState(new JumpState(player));
                break;
            default:
                break;
        }



        Obstacle currentObstacle = player.RunnerManager.Obstacles[player.CurrentObstacle];

        /*if (currentObstacle.ObstacleType == ObstacleType.Wall && action >= 3) {
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
            else if (currentDistance < player.LastXDistance) {
                player.LastXDistance = currentDistance;
                AddReward(0.001f);
            }
        }*/
        if (currentObstacle.ObstacleType == ObstacleType.Wall && action != 0) {
            //agent.reward = -0.01f;
            AddReward(-0.001f);
        }
        else if (currentObstacle.ObstacleType == ObstacleType.Slide && action == 2) {
            //agent.reward = -0.01f;
            AddReward(-0.001f);
        }
        else if (currentObstacle.ObstacleType == ObstacleType.Jump && action == 1) {
            //agent.reward = -0.01f;
            AddReward(-0.001f);
        }
        else if (currentObstacle.ObstacleType == ObstacleType.Wall && action == 0) {
            int currentDistance = (int)Mathf.Abs(player.transform.position.x - currentObstacle.transform.position.x);

            if (currentDistance > player.LastXDistance)
                AddReward(-0.001f);
            else if (currentDistance < player.LastXDistance) {
                player.LastXDistance = currentDistance;
                AddReward(0.001f);
            }
        }

    }

    private void OnCollisionEnter(Collision collision) {
        if (collision.collider.CompareTag("Finish")) {
            SetReward(0.1f);
            EndEpisode();
            Debug.Log("[ML-Agents] Finished", gameObject);
        }else if (collision.collider.CompareTag("Obstacle")) {
            SetReward(-0.01f);
            EndEpisode();
        }
    }

    private void OnTriggerEnter(Collider other) {
        if (other.CompareTag("Checkpoint")) {
            AddReward(0.01f);
        }else if (other.CompareTag("Coin")) {
            AddReward(0.005f);
            other.gameObject.SetActive(false);
        }
    }

    public override void Heuristic(in ActionBuffers actionsOut) {
        ActionSegment<float> continuousActions = actionsOut.ContinuousActions;
        ActionSegment<int> discreteActions = actionsOut.DiscreteActions;

        continuousActions[0] = Input.GetAxis("Horizontal");

        if (Input.GetButton("Jump")) {
            //player.SwitchState(new JumpState(player));
            discreteActions[0] = 1;
        }
        else if (Input.GetButton("Roll")) {
            //player.SwitchState(new RollState(player));
            discreteActions[0] = 2;
        }
    }
}

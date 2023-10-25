using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlappyManager : MonoBehaviour
{
    [SerializeField] int numberOfObstacles;
    [SerializeField] GameObject obstaclePrefab;
    [SerializeField] Vector3 betweenObstaclesSpace;
    [SerializeField] int obstableOffset;
    [SerializeField] FlappyPlayer player;
    int currentScore;

    GameObject[] obstacles;
    int currentObstacle;

    // Start is called before the first frame update
    void Start()
    {
        obstacles = new GameObject[numberOfObstacles];
        for (int i = 0; i < numberOfObstacles; i++) {
            Vector3 obstaclePosition = new Vector3();

            obstaclePosition.x = (i + 1) * betweenObstaclesSpace.x;
            obstaclePosition.y = Random.Range(-obstableOffset, obstableOffset + 1);
            obstaclePosition.z = 0;

            GameObject obstacle = Instantiate(obstaclePrefab, obstaclePosition, Quaternion.identity);
            obstacles[i] = obstacle;
        }
    }


    private void FixedUpdate() {
        currentScore = (int)player.transform.position.x;
    }


}

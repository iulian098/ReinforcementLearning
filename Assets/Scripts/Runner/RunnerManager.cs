using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RunnerManager : MonoBehaviour
{
    public static RunnerManager instance;

    [SerializeField] int nrObstacles;
    [SerializeField] int obstacleXOffset;
    [SerializeField] int obstacleDistance;
    [SerializeField] GameObject obstaclePrefab;
    [SerializeField] Transform finishTransform;
    GameObject[] obstacles;

    float score;
    bool initialized;

    public GameObject[] Obstacles => obstacles;
    public float Score => score;
    public bool Initialized => initialized;
    private void Awake() {
        instance = this;
    }

    // Start is called before the first frame update
    public void Init()
    {
        if (initialized) return;
        initialized = true;

        obstacles = new GameObject[nrObstacles + 1];

        for (int i = 0; i < nrObstacles; i++) {
            Vector3 obstaclePos = new Vector3();
            obstaclePos.x = transform.position.x + Random.Range(-obstacleXOffset, obstacleXOffset + 1);
            obstaclePos.y = 0;
            obstaclePos.z = (i + 1) * obstacleDistance;
            GameObject obstacle = Instantiate(obstaclePrefab, obstaclePos, Quaternion.identity);
            obstacles[i] = obstacle;
        }

        finishTransform.position = new Vector3(0, 0, (nrObstacles + 1) * obstacleDistance);
        obstacles[nrObstacles] = finishTransform.gameObject;
    }

    // Update is called once per frame
    void Update()
    {
        score += Time.deltaTime;
    }

    public void ResetScore() {
        score = 0;
    }
}

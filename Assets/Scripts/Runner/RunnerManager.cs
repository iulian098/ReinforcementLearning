using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Runner {

    public class RunnerManager : MonoBehaviour {
        public static RunnerManager Instance;

        [SerializeField] int nrObstacles;
        [SerializeField] int obstacleXOffset;
        [SerializeField] int obstacleDistance;
        [SerializeField] Obstacle[] obstaclePrefabs;
        [SerializeField] Obstacle finish;
        [SerializeField] bool selfInit;
        [SerializeField] Transform obstaclesContainer;
        Obstacle[] obstacles;

        float score;
        bool initialized;

        public Obstacle[] Obstacles => obstacles;
        public float Score => score;
        public bool Initialized => initialized;
        private void Awake() {
            Instance = this;
        }

        private void Start() {
            if (selfInit) Init();
        }

        void Update() {
            score += Time.deltaTime;
        }

        public void ResetScore() {
            score = 0;
        }

        public void Init() {
            if (initialized) return;
            initialized = true;

            StartCoroutine(GenerateLevel());
        }

        IEnumerator GenerateLevel() {
            obstacles = new Obstacle[nrObstacles + 1];

            for (int i = 0; i < nrObstacles; i++) {
                Vector3 obstaclePos = new Vector3();
                obstaclePos.x = transform.position.x + Random.Range(-obstacleXOffset, obstacleXOffset + 1);
                obstaclePos.y = 0;
                obstaclePos.z = (i + 1) * obstacleDistance;
                Obstacle obstacle = Instantiate(obstaclePrefabs[Random.Range(0, obstaclePrefabs.Length)], obstaclesContainer);//obstaclePos, Quaternion.identity);
                obstacle.transform.localPosition = obstaclePos;
                obstacle.transform.rotation = Quaternion.identity;
                obstacles[i] = obstacle;
                yield return new WaitForSeconds(0.1f);
            }

            finish.transform.position = new Vector3(0, 0, (nrObstacles + 1) * obstacleDistance);
            obstacles[nrObstacles] = finish;
        }

        public void RegenerateLevel() {
            foreach (var obstacle in obstacles) {
                if (obstacle == finish) continue;
                Destroy(obstacle.gameObject);
            }
            StartCoroutine(GenerateLevel());
        }
    }

}

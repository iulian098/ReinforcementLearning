using UnityEngine;

public class RunnerPlayer : MonoBehaviour
{
    [SerializeField] float speed;
    [SerializeField] int maxX, minX;

    int currentObstacle = 0;

    public bool Stopped;
    public int CurrentObstacle => currentObstacle;

    // Update is called once per frame
    void Update()
    {
        if (Stopped) return;
        if (Input.GetKeyDown(KeyCode.A))
            ReceiveInput(-1);
        else if (Input.GetKeyDown(KeyCode.D))
            ReceiveInput(1);

        transform.Translate(Vector3.forward * speed * Time.deltaTime);

        if (currentObstacle >= RunnerManager.instance.Obstacles.Length - 1) return;

        if (transform.position.z >= RunnerManager.instance.Obstacles[currentObstacle].transform.position.z - 1)
            currentObstacle++;
    }

    public void ReceiveInput(int val) {
        if ((transform.position.x == maxX && val == 1) || (transform.position.x == minX && val == -1))
            return;

        transform.position += Vector3.right * val;
    }

    public void ResetData() {
        currentObstacle = 0;
        Stopped = false;
    }
}

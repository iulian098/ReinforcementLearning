using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public int id;
    public bool useAI;

    [SerializeField] float speed;
    [SerializeField] PlayerInput input;
    bool moved;

    private void Update() {

        if(!useAI)
            input.SendInput(Input.GetAxisRaw("Vertical"), Input.GetAxisRaw("Horizontal"));

        if (moved) {
            if (input.Horizontal == 0 && input.Vertical == 0)
                moved = false;
            return;
        }

        if (input.Horizontal != 0)
            transform.position += Vector3.right * input.Horizontal;
        else if (input.Vertical != 0)
            transform.position += Vector3.forward * input.Vertical;
    }

    public void SendInput(float h = 0, float v = 0) {
        input.SendInput(h, v);
    }
}

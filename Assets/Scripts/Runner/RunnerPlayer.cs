using StateMachine;
using UnityEngine;

namespace Runner {

    public class RunnerPlayer : MonoBehaviour {

        readonly int ANIM_COLL_HEIGHT = Animator.StringToHash("ColliderHeight");
        readonly int ANIM_COLL_Y = Animator.StringToHash("ColliderY");
        readonly int ANIM_ROLL = Animator.StringToHash("Roll");
        readonly int ANIM_RUN = Animator.StringToHash("Run");

        [SerializeField] float speed;
        [SerializeField] int maxX, minX;
        [SerializeField] Animator anim;
        [SerializeField] CapsuleCollider coll;
        [SerializeField] bool isAI;

        Vector3 collCenter;
        float collHeight;

        int currentObstacle = 0;
        int lastXDistance;

        bool acceptingSteps = true;
        public bool Stopped;
        public int CurrentObstacle { 
            get { 
                return currentObstacle; 
            } 
            set { 
                currentObstacle = value;
                lastXDistance = (int)Mathf.Abs(transform.position.x - RunnerManager.instance.Obstacles[currentObstacle].transform.position.x);
            } 
        }
        public CapsuleCollider Coll => coll;
        public bool AcceptingSteps => acceptingSteps;
        public int LastXDistance { get => lastXDistance; set => lastXDistance = value; }

        private void Start() {
            if(coll == null)
                coll = GetComponent<CapsuleCollider>();
            CurrentObstacle = 0;

        }

        void Update() {
            UpdateAnimator();
            if (Stopped) return;
            if (!isAI) ReceiveInput(Input.GetAxis("Horizontal") / 3, Input.GetButton("Jump"), Input.GetKeyDown(KeyCode.LeftControl));

            transform.Translate(Vector3.forward * speed * Time.deltaTime);

            if (currentObstacle >= RunnerManager.instance.Obstacles.Length - 1) return;

            if (transform.position.z >= RunnerManager.instance.Obstacles[currentObstacle].transform.position.z - 1)
                CurrentObstacle++;

        }

        void UpdateAnimator() {
            if (anim == null) return;

            if(anim.GetCurrentAnimatorStateInfo(0).IsName("Roll")){
                collHeight = anim.GetFloat(ANIM_COLL_HEIGHT);
                collCenter.y = anim.GetFloat(ANIM_COLL_Y);
                acceptingSteps = false;

            }
            else if (anim.IsInTransition(0)) {
                acceptingSteps = false;
            }
            else {
                collHeight = 2;
                collCenter.y = 1;
                acceptingSteps = true;
            }

            coll.height = collHeight;
            coll.center = collCenter;

            anim.SetFloat("Direction", Input.GetAxis("Horizontal"));

        }

        public void ReceiveInput(float val, bool jump, bool roll) {

            if (jump && anim != null) {

            }

            if (roll && anim != null) {
                anim.SetTrigger(ANIM_ROLL);
                acceptingSteps = false;
            }

            if ((transform.position.x >= maxX && val == 1) || (transform.position.x <= minX && val == -1))
                return;

            transform.position += Vector3.right * val;
        }

        public void ResetData() {
            CurrentObstacle = 0;
            Stopped = false;
            acceptingSteps = true;
            anim.ResetTrigger(ANIM_ROLL);
            anim.Play(ANIM_RUN);
        }
    }

}

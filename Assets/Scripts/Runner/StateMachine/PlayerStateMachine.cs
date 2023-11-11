// Ignore Spelling: Anim Collider

using Runner;
using System;
using UnityEngine;

namespace StateMachine.Player {
    public class PlayerStateMachine : StateMachine {
        readonly int ANIM_ROLL = Animator.StringToHash("Roll");
        readonly int ANIM_JUMP = Animator.StringToHash("Jump");
        readonly int ANIM_RUN = Animator.StringToHash("Run");
        readonly int ANIM_GROUNDED = Animator.StringToHash("Grounded");

        [SerializeField] Animator anim;
        [SerializeField] CapsuleCollider coll;
        [SerializeField] Rigidbody characterRigidbody;
        [SerializeField] float speed;
        [SerializeField] float jumpForce;
        [SerializeField] LayerMask layerMask;

        int currentObstacle;
        int lastXDistance;
        bool stopped;
        bool acceptingSteps;
        bool isGrounded;

        public Action<PlayerInput> OnInputReceived;

        public Animator Anim => anim;
        public CapsuleCollider Coll => coll;
        public Rigidbody CharacterRigidbody => characterRigidbody;
        public int CurrentObstacle { 
            get => currentObstacle; 
            set {
                currentObstacle = value;
                LastXDistance = (int)Mathf.Abs(transform.position.x - RunnerManager.Instance.Obstacles[currentObstacle].transform.position.x);
            }
        }
        public int LastXDistance { get => lastXDistance; set => lastXDistance = value; }
        public bool AcceptingSteps { get => acceptingSteps; set => acceptingSteps = value; }
        public bool Stopped { get => stopped; set => stopped = value; }
        public bool IsGrounded => isGrounded;
        public float Speed => speed;
        public float JumpForce => jumpForce;

        protected override void Update() {
            base.Update();
            if (transform.position.z >= RunnerManager.Instance.Obstacles[CurrentObstacle].transform.position.z - 1)
                CurrentObstacle++;
            GroundCheck();

            anim.SetBool(ANIM_GROUNDED, isGrounded);
            if (stopped) return;
            transform.Translate(Vector3.forward * Speed * Time.deltaTime);
        }

        void GroundCheck() {
            if (Physics.CheckSphere(transform.position, 0.2f, layerMask))
                isGrounded = true;
            else
                isGrounded = false;
        }

        public void SendInput(PlayerInput playerInput) {
            OnInputReceived?.Invoke(playerInput);
        }

        public void SetColliderHeight(float height) {
            coll.height = height;
            coll.center = new Vector3(0, height / 2, 0);
        }

        public void ResetData() {
            CurrentObstacle = 0;
            Stopped = false;
            acceptingSteps = true;
            characterRigidbody.velocity = Vector3.zero;
            anim.ResetTrigger(ANIM_ROLL);
            anim.ResetTrigger(ANIM_JUMP);
            anim.SetBool(ANIM_GROUNDED, true);
            anim.Play(ANIM_RUN);
            SetColliderHeight(2);
            SwitchState(new MovementState(this));
        }

        private void OnDrawGizmosSelected() {
            Gizmos.color = Color.green;
            Gizmos.DrawSphere(transform.position, 0.2f);
        }

    }
}

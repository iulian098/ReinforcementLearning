using UnityEngine;

namespace StateMachine.Player {

    public class MovementState : PlayerBaseState {
        readonly int ANIM_MOVEMENT = Animator.StringToHash("Movement");

        public MovementState(PlayerStateMachine stateMachine) : base(stateMachine) { }

        public override void OnStateEnter() {
            stateMachine.AcceptingSteps = true;
            stateMachine.OnInputReceived += Move;
            stateMachine.Anim.CrossFadeInFixedTime(ANIM_MOVEMENT, 0.1f);
        }

        public override void OnStateTick(float deltaTime) { }

        public override void OnStateExit() {
            stateMachine.AcceptingSteps = false;
            stateMachine.OnInputReceived -= Move;
        }

        void Move(PlayerInput playerInput) {
            stateMachine.transform.Translate(Vector3.right * playerInput.direction.x * stateMachine.XSpeed * Time.deltaTime);
        }

    }

}

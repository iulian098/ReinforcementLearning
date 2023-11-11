using UnityEngine;

namespace StateMachine.Player {

    public class RollState : PlayerBaseState {
        readonly int ROLL_HASH = Animator.StringToHash("Roll");

        public RollState(PlayerStateMachine stateMachine) : base(stateMachine) { }

        public override void OnStateEnter() {
            stateMachine.Anim.SetTrigger(ROLL_HASH);
            stateMachine.Anim.PlayInFixedTime(ROLL_HASH, 0, 0.1f);
            stateMachine.SetColliderHeight(1);
        }

        public override void OnStateTick(float deltaTime) {
            if (GetNormalizedTime(stateMachine.Anim, "Roll") < 1f) return;

            stateMachine.SwitchState(new MovementState(stateMachine));
        }

        public override void OnStateExit() {
            stateMachine.SetColliderHeight(2);
        }
    }

}

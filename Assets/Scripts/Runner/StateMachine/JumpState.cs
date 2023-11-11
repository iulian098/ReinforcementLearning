using JetBrains.Annotations;
using StateMachine.Player;
using System.Collections;
using UnityEngine;

namespace StateMachine.Player {

    public class JumpState : PlayerBaseState {
        readonly int ANIM_JUMP = Animator.StringToHash("Jump");

        public JumpState(PlayerStateMachine stateMachine) : base(stateMachine) { }

        public override void OnStateEnter() {
            stateMachine.CharacterRigidbody.AddForce(Vector3.up * stateMachine.JumpForce);
            stateMachine.Anim.PlayInFixedTime(ANIM_JUMP, 0, 0.1f);
            stateMachine.AcceptingSteps = false;
        }

        public override void OnStateTick(float deltaTime) {
            if (!stateMachine.IsGrounded && stateMachine.CharacterRigidbody.velocity.y < 0) {
                stateMachine.SwitchState(new FallingState(stateMachine));
                return;
            }
        }

        public override void OnStateExit() { }

    }
}

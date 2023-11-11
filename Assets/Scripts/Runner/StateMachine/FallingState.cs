using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace StateMachine.Player {
    public class FallingState : PlayerBaseState {
        public FallingState(PlayerStateMachine stateMachine) : base(stateMachine) { }

        public override void OnStateEnter() { }

        public override void OnStateTick(float deltaTime) {
            if (stateMachine.IsGrounded) {
                stateMachine.SwitchState(new MovementState(stateMachine));
            }
        }

        public override void OnStateExit() {
            stateMachine.AcceptingSteps = true;
        }
    }
}

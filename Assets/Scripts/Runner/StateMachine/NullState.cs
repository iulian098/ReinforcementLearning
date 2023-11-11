using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace StateMachine.Player {

    public class NullState : PlayerBaseState {
        public NullState(PlayerStateMachine stateMachine) : base(stateMachine) { }

        public override void OnStateEnter() {
            stateMachine.AcceptingSteps = true;
        }

        public override void OnStateExit() { }

        public override void OnStateTick(float deltaTime) { }
    }

}

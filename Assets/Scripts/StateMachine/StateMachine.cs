using UnityEngine;

namespace StateMachine {

    public class StateMachine : MonoBehaviour {
        private BaseState currentState;

        [SerializeField] float minTimeState;

        float stateTime;

        public void SwitchState(BaseState state) {
            if (stateTime < minTimeState) return;

            currentState?.OnStateExit();
            currentState = state;
            currentState.OnStateEnter();
        }

        protected virtual void Update() {
            stateTime += Time.deltaTime;
            if (currentState != null) currentState.OnStateTick(Time.deltaTime);
        }
    }

}

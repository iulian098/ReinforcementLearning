namespace StateMachine.Player {

    public abstract class PlayerBaseState : BaseState {
        protected PlayerStateMachine stateMachine;

        public PlayerBaseState(PlayerStateMachine stateMachine) {
            this.stateMachine = stateMachine;
        }
    }
}

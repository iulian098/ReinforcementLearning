using UnityEngine;

namespace StateMachine {

    public abstract class BaseState : ScriptableObject {
        public abstract void OnStateEnter();
        public abstract void OnStateTick(float deltaTime);
        public abstract void OnStateExit();

        protected float GetNormalizedTime(Animator animator, string tag) {
            AnimatorStateInfo currentInfo = animator.GetCurrentAnimatorStateInfo(0);
            AnimatorStateInfo nextInfo = animator.GetNextAnimatorStateInfo(0);

            if (animator.IsInTransition(0) && nextInfo.IsTag(tag))
                return nextInfo.normalizedTime;
            else if (!animator.IsInTransition(0) && currentInfo.IsTag(tag))
                return currentInfo.normalizedTime;
            else
                return 0f;

        }
    }

}

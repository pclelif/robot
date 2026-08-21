using UnityEngine;
using Robot.Player.Movement;

namespace Robot.Player
{
    [RequireComponent(typeof(RobotMovementController))]
    public sealed class RobotAnimator : MonoBehaviour
    {
        [SerializeField, Min(0f)] private float speedDampTime = 0.1f;
        [SerializeField] private string idleState = "StaticIdle";
        [SerializeField] private string walkState = "Walk";
        [SerializeField] private string runState = "Run";
        private RobotMovementController movement;
        private Animator animator;
        private static readonly int Speed = Animator.StringToHash("Speed");
        private static readonly int IsGrounded = Animator.StringToHash("IsGrounded");
        private int activeState;
        private bool hasSpeedParameter;
        private bool hasGroundedParameter;

        private void Awake()
        {
            movement = GetComponent<RobotMovementController>();
            if (animator == null) animator = GetComponentInChildren<Animator>();
            if (animator != null)
            {
                animator.applyRootMotion = false;
                foreach (AnimatorControllerParameter parameter in animator.parameters)
                {
                    hasSpeedParameter |= parameter.nameHash == Speed;
                    hasGroundedParameter |= parameter.nameHash == IsGrounded;
                }
            }
        }
        private void Update()
        {
            if (animator == null) return;
            if (hasSpeedParameter) animator.SetFloat(Speed, movement.CurrentSpeedNormalized, speedDampTime, Time.deltaTime);
            if (hasGroundedParameter) animator.SetBool(IsGrounded, movement.IsGrounded);

            // The supplied controller has named locomotion states but no parameters/transitions.
            // Drive those existing states without replacing the third-party controller.
            string state = movement.CurrentSpeedNormalized <= 0.01f ? idleState :
                movement.CurrentSpeedNormalized < 0.8f ? walkState : runState;
            int stateHash = Animator.StringToHash(state);
            if (stateHash != activeState)
            {
                animator.CrossFade(stateHash, speedDampTime);
                activeState = stateHash;
            }
        }
    }
}

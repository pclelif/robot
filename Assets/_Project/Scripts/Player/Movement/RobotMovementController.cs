using UnityEngine;

namespace Robot.Player.Movement
{
    [RequireComponent(typeof(CharacterController))]
    public class RobotMovementController : MonoBehaviour
    {
        [Header("Movement Settings")]
        [SerializeField] private float walkSpeed = 3.5f;
        [SerializeField] private float runSpeed = 7.0f;
        [SerializeField] private float rotationSpeed = 12.0f;
        [SerializeField] private float acceleration = 10.0f;

        [Header("Gravity & Grounding")]
        [SerializeField] private float gravity = -19.62f;
        [SerializeField] private float groundedGravity = -2.0f;
        [SerializeField] private Transform groundCheckPoint;
        [SerializeField] private float groundCheckRadius = 0.25f;
        [SerializeField] private LayerMask groundMask;

        [Header("Camera Reference")]
        [SerializeField] private Transform cameraTransform;

        // Public Properties for External / Mobile Controls
        public Vector2 MovementInput { get; set; }
        public bool IsRunningInput { get; set; }
        public bool IsGrounded { get; private set; }
        public float CurrentSpeedNormalized { get; private set; }

        // Component References
        private CharacterController characterController;
        private Animator animator;

        // Internal Movement State
        private Vector3 verticalVelocity;
        private float currentSpeed;
        private static readonly int SpeedHash = Animator.StringToHash("Speed");
        private static readonly int IsGroundedHash = Animator.StringToHash("IsGrounded");
        private static readonly int IsMovingHash = Animator.StringToHash("IsMoving");

        private void Awake()
        {
            characterController = GetComponent<CharacterController>();
            animator = GetComponentInChildren<Animator>();

            if (cameraTransform == null && Camera.main != null)
            {
                cameraTransform = Camera.main.transform;
            }
        }

        private void Update()
        {
            HandleGrounding();
            HandleMovementAndRotation();
            HandleGravity();
            UpdateAnimator();
        }

        private void HandleGrounding()
        {
            if (groundCheckPoint != null)
            {
                IsGrounded = Physics.CheckSphere(groundCheckPoint.position, groundCheckRadius, groundMask);
            }
            else
            {
                IsGrounded = characterController.isGrounded;
            }

            if (IsGrounded && verticalVelocity.y < 0)
            {
                verticalVelocity.y = groundedGravity;
            }
        }

        private void HandleMovementAndRotation()
        {
            // Read input vector (WASD or Mobile Joystick)
            Vector3 inputDir = new Vector3(MovementInput.x, 0f, MovementInput.y).normalized;

            // Handle keyboard input fallback if MovementInput is zero
            if (inputDir.sqrMagnitude < 0.01f)
            {
                float h = Input.GetAxisRaw("Horizontal");
                float v = Input.GetAxisRaw("Vertical");
                inputDir = new Vector3(h, 0f, v).normalized;
                
                if (Input.GetKey(KeyCode.LeftShift))
                {
                    IsRunningInput = true;
                }
            }

            bool isMoving = inputDir.sqrMagnitude > 0.01f;
            float targetSpeed = isMoving ? (IsRunningInput ? runSpeed : walkSpeed) : 0f;

            currentSpeed = Mathf.MoveTowards(currentSpeed, targetSpeed, acceleration * Time.deltaTime);

            if (isMoving)
            {
                // Calculate camera-relative movement direction
                Vector3 camForward = cameraTransform != null ? cameraTransform.forward : Vector3.forward;
                Vector3 camRight = cameraTransform != null ? cameraTransform.right : Vector3.right;
                camForward.y = 0f;
                camRight.y = 0f;
                camForward.Normalize();
                camRight.Normalize();

                Vector3 moveDirection = (camForward * inputDir.z + camRight * inputDir.x).normalized;

                // Move CharacterController
                characterController.Move(moveDirection * (currentSpeed * Time.deltaTime));

                // Smooth Rotation towards moveDirection
                if (moveDirection.sqrMagnitude > 0.001f)
                {
                    Quaternion targetRotation = Quaternion.LookRotation(moveDirection);
                    transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
                }
            }

            CurrentSpeedNormalized = targetSpeed > 0f ? (currentSpeed / runSpeed) : 0f;
        }

        private void HandleGravity()
        {
            verticalVelocity.y += gravity * Time.deltaTime;
            characterController.Move(verticalVelocity * Time.deltaTime);
        }

        private void UpdateAnimator()
        {
            if (animator == null) return;

            animator.SetFloat(SpeedHash, CurrentSpeedNormalized, 0.1f, Time.deltaTime);
            animator.SetBool(IsGroundedHash, IsGrounded);
            animator.SetBool(IsMovingHash, CurrentSpeedNormalized > 0.05f);
        }

        private void OnDrawGizmosSelected()
        {
            if (groundCheckPoint != null)
            {
                Gizmos.color = IsGrounded ? Color.green : Color.red;
                Gizmos.DrawWireSphere(groundCheckPoint.position, groundCheckRadius);
            }
        }
    }
}

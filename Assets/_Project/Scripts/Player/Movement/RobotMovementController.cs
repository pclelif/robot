using UnityEngine;

namespace Robot.Player.Movement
{
    [RequireComponent(typeof(CharacterController))]
    public class RobotMovementController : MonoBehaviour
    {
        [Header("Movement Settings")]
        [SerializeField] private float walkSpeed = 6.0f;
        [SerializeField] private float runSpeed = 10.0f;
        [SerializeField] private float rotationSpeed = 18.0f;
        [SerializeField] private float acceleration = 35.0f;

        [Header("Gravity & Grounding")]
        [SerializeField] private float gravity = -15.0f;
        [SerializeField] private float groundedGravity = -2.0f;
        [SerializeField] private Transform groundCheckPoint;
        [SerializeField] private float groundCheckRadius = 0.4f;
        [SerializeField] private LayerMask groundMask = ~0; // Default to all layers

        // Public Properties
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
        private Vector3 moveDirection;
        private static readonly int SpeedHash = Animator.StringToHash("Speed");
        private static readonly int IsGroundedHash = Animator.StringToHash("IsGrounded");
        private static readonly int IsMovingHash = Animator.StringToHash("IsMoving");

        private void Awake()
        {
            characterController = GetComponent<CharacterController>();
            animator = GetComponentInChildren<Animator>();

            if (animator != null)
            {
                animator.applyRootMotion = false; // Prevent animator root motion from locking position
            }

            EnsureGroundFloorExists();
        }

        private void Start()
        {
            // Position robot safely above ground level at Start
            if (transform.position.y < 0.1f)
            {
                transform.position = new Vector3(transform.position.x, 0.15f, transform.position.z);
            }
        }

        private void EnsureGroundFloorExists()
        {
            // If no floor collider exists in scene, create a default floor plane
            Collider[] colliders = FindObjectsByType<Collider>(FindObjectsSortMode.None);
            bool hasFloor = false;
            foreach (var col in colliders)
            {
                if (col.gameObject != gameObject && !col.transform.IsChildOf(transform))
                {
                    hasFloor = true;
                    break;
                }
            }

            if (!hasFloor)
            {
                GameObject floor = GameObject.CreatePrimitive(PrimitiveType.Plane);
                floor.name = "Auto_Environment_Floor";
                floor.transform.position = new Vector3(0f, 0f, 0f);
                floor.transform.localScale = new Vector3(20f, 1f, 20f); // 200x200 floor plane

                Renderer ren = floor.GetComponent<Renderer>();
                if (ren != null)
                {
                    Shader urpLit = Shader.Find("Universal Render Pipeline/Lit");
                    if (urpLit != null)
                    {
                        Material mat = new Material(urpLit);
                        mat.color = new Color(0.25f, 0.28f, 0.32f); // Slate gray floor
                        ren.sharedMaterial = mat;
                    }
                }
            }
        }

        private void Update()
        {
            HandleGrounding();
            HandleInputAndMovement();
            UpdateAnimator();
        }

        private void HandleGrounding()
        {
            Vector3 checkPos = groundCheckPoint != null ? groundCheckPoint.position : transform.position + Vector3.up * 0.15f;
            
            bool rayGrounded = Physics.Raycast(transform.position + Vector3.up * 0.2f, Vector3.down, 0.4f, groundMask, QueryTriggerInteraction.Ignore);
            IsGrounded = rayGrounded || characterController.isGrounded;

            if (IsGrounded && verticalVelocity.y < 0)
            {
                verticalVelocity.y = groundedGravity;
            }
            else
            {
                verticalVelocity.y += gravity * Time.deltaTime;
                verticalVelocity.y = Mathf.Max(verticalVelocity.y, -15.0f); // Clamp downward velocity
            }
        }

        private void HandleInputAndMovement()
        {
            float h = MovementInput.x;
            float v = MovementInput.y;
            bool run = false;

            // Direct KeyCode checks (W = Forward +Z, S = Backward -Z, D = Right +X, A = Left -X)
            try
            {
                if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow)) v += 1f;
                if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow)) v -= 1f;
                if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow)) h += 1f;
                if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow)) h -= 1f;
                if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift)) run = true;
            }
            catch { }

#if ENABLE_INPUT_SYSTEM
            if (Mathf.Approximately(h, 0f) && Mathf.Approximately(v, 0f))
            {
                try
                {
                    var keyboard = UnityEngine.InputSystem.Keyboard.current;
                    if (keyboard != null)
                    {
                        if (keyboard.wKey.isPressed || keyboard.upArrowKey.isPressed) v += 1f;
                        if (keyboard.sKey.isPressed || keyboard.downArrowKey.isPressed) v -= 1f;
                        if (keyboard.dKey.isPressed || keyboard.rightArrowKey.isPressed) h += 1f;
                        if (keyboard.aKey.isPressed || keyboard.leftArrowKey.isPressed) h -= 1f;
                        if (keyboard.leftShiftKey.isPressed || keyboard.rightShiftKey.isPressed) run = true;
                    }
                }
                catch { }
            }
#endif

            if (Mathf.Approximately(h, 0f) && Mathf.Approximately(v, 0f))
            {
                try
                {
                    h = Input.GetAxisRaw("Horizontal");
                    v = Input.GetAxisRaw("Vertical");
                }
                catch { }
            }

            IsRunningInput = run;

            Vector3 inputDir = new Vector3(h, 0f, v);
            if (inputDir.sqrMagnitude > 1.0f)
            {
                inputDir.Normalize();
            }

            bool isMoving = inputDir.sqrMagnitude > 0.01f;
            float targetSpeed = isMoving ? (IsRunningInput ? runSpeed : walkSpeed) : 0f;

            currentSpeed = Mathf.MoveTowards(currentSpeed, targetSpeed, acceleration * Time.deltaTime);

            if (isMoving)
            {
                // Pure Direct World WASD Movement: W = +Z (North), S = -Z (South), D = +X (East), A = -X (West)
                // Completely ignores static/tilted scene camera angles so W ALWAYS walks forward in space!
                moveDirection = new Vector3(inputDir.x, 0f, inputDir.z).normalized;

                if (moveDirection.sqrMagnitude > 0.001f)
                {
                    Quaternion targetRotation = Quaternion.LookRotation(moveDirection);
                    transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
                }
            }
            else
            {
                moveDirection = Vector3.zero;
            }

            // Single combined CharacterController.Move call per frame
            Vector3 finalVelocity = (moveDirection * currentSpeed) + verticalVelocity;
            characterController.Move(finalVelocity * Time.deltaTime);

            CurrentSpeedNormalized = targetSpeed > 0f ? (currentSpeed / runSpeed) : 0f;
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
            Vector3 checkPos = groundCheckPoint != null ? groundCheckPoint.position : transform.position + Vector3.up * 0.15f;
            Gizmos.color = IsGrounded ? Color.green : Color.red;
            Gizmos.DrawWireSphere(checkPos, groundCheckRadius);
        }
    }
}

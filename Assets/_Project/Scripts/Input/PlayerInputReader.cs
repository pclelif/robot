using UnityEngine;
using UnityEngine.InputSystem;

namespace Robot.Input
{
    /// <summary>Owns Input System access and exposes device-independent player intent with Keyboard fallback.</summary>
    public sealed class PlayerInputReader : MonoBehaviour
    {
        [SerializeField] private InputActionAsset actions;
        private const string PlayerMapName = "Player";
        private const string UiMapName = "UI";

        private InputActionMap playerMap;
        private InputActionMap uiMap;
        private InputAction moveAction;
        private InputAction lookAction;
        private InputAction runAction;
        private InputAction zoomAction;
        private InputAction jumpAction;
        private InputAction interactAction;
        private InputAction pauseAction;
        private Vector2 moveInput;
        private Vector2 lookInput;
        private Vector2 mobileMoveInput;
        private Vector2 mobileLookInput;
        private float zoomInput;
        private bool jumpPressed;
        private bool interactPressed;
        private bool pausePressed;
        private bool isListening;

        public Vector2 MoveInput => Vector2.ClampMagnitude(moveInput + mobileMoveInput, 1f);
        public Vector2 LookInput => lookInput + mobileLookInput;
        public bool RunHeld => (runAction != null && runAction.IsPressed()) || UnityEngine.Input.GetKey(KeyCode.LeftShift);
        public float ZoomInput => zoomInput;

        public void Configure(InputActionAsset inputActions)
        {
            actions = inputActions;
            Initialize();
            if (isActiveAndEnabled) StartListening();
        }

        private void Awake()
        {
            Initialize();
        }

        private void Initialize()
        {
            if (actions == null || playerMap != null) return;
            playerMap = actions.FindActionMap(PlayerMapName, true);
            uiMap = actions.FindActionMap(UiMapName, false);
            moveAction = playerMap.FindAction("Move", true);
            lookAction = playerMap.FindAction("Look", true);
            runAction = playerMap.FindAction("Run", false) ?? playerMap.FindAction("Sprint", false);
            zoomAction = playerMap.FindAction("Zoom", false);
            jumpAction = playerMap.FindAction("Jump", false);
            interactAction = playerMap.FindAction("Interact", false);
            pauseAction = playerMap.FindAction("Pause", false);
        }

        private void OnEnable()
        {
            Initialize();
            StartListening();
        }

        private void StartListening()
        {
            if (playerMap == null || isListening) return;
            playerMap.Enable();
            if (moveAction != null) { moveAction.performed += ReadMove; moveAction.canceled += ReadMove; }
            if (lookAction != null) { lookAction.performed += ReadLook; lookAction.canceled += ReadLook; }
            if (zoomAction != null) { zoomAction.performed += ReadZoom; zoomAction.canceled += ReadZoom; }
            SubscribeButtons();
            isListening = true;
        }

        private void OnDisable()
        {
            if (playerMap == null) return;
            if (moveAction != null) { moveAction.performed -= ReadMove; moveAction.canceled -= ReadMove; }
            if (lookAction != null) { lookAction.performed -= ReadLook; lookAction.canceled -= ReadLook; }
            if (zoomAction != null) { zoomAction.performed -= ReadZoom; zoomAction.canceled -= ReadZoom; }
            UnsubscribeButtons();
            playerMap.Disable();
            uiMap?.Disable();
            isListening = false;
        }

        private void Update()
        {
            // 1. Primary: Input System live value polling
            if (moveAction != null && moveAction.enabled)
            {
                moveInput = moveAction.ReadValue<Vector2>();
            }

            // 2. Safety Fallback: Legacy Keyboard WASD if Input System returns zero
            if (moveInput.sqrMagnitude < 0.001f)
            {
                float x = 0f;
                float y = 0f;
                if (UnityEngine.Input.GetKey(KeyCode.W) || UnityEngine.Input.GetKey(KeyCode.UpArrow)) y += 1f;
                if (UnityEngine.Input.GetKey(KeyCode.S) || UnityEngine.Input.GetKey(KeyCode.DownArrow)) y -= 1f;
                if (UnityEngine.Input.GetKey(KeyCode.D) || UnityEngine.Input.GetKey(KeyCode.RightArrow)) x += 1f;
                if (UnityEngine.Input.GetKey(KeyCode.A) || UnityEngine.Input.GetKey(KeyCode.LeftArrow)) x -= 1f;

                Vector2 kbDir = new Vector2(x, y);
                if (kbDir.sqrMagnitude > 0.001f)
                {
                    moveInput = kbDir.normalized;
                }
            }
        }

        public void SetMobileMove(Vector2 value) => mobileMoveInput = Vector2.ClampMagnitude(value, 1f);
        public void SetMobileLook(Vector2 value) => mobileLookInput = value;
        public bool ConsumeJumpPressed() => Consume(ref jumpPressed) || UnityEngine.Input.GetKeyDown(KeyCode.Space);
        public bool ConsumeInteractPressed() => Consume(ref interactPressed) || UnityEngine.Input.GetKeyDown(KeyCode.E);
        public bool ConsumePausePressed() => Consume(ref pausePressed) || UnityEngine.Input.GetKeyDown(KeyCode.Escape);

        public void SetPaused(bool paused)
        {
            if (paused) { playerMap.Disable(); uiMap?.Enable(); }
            else { uiMap?.Disable(); playerMap.Enable(); }
        }

        private void ReadMove(InputAction.CallbackContext context) => moveInput = context.ReadValue<Vector2>();
        private void ReadLook(InputAction.CallbackContext context) => lookInput = context.ReadValue<Vector2>();
        private void ReadZoom(InputAction.CallbackContext context) => zoomInput = context.ReadValue<float>();

        private void SubscribeButtons()
        {
            if (jumpAction != null) jumpAction.performed += OnJump;
            if (interactAction != null) interactAction.performed += OnInteract;
            if (pauseAction != null) pauseAction.performed += OnPause;
        }

        private void UnsubscribeButtons()
        {
            if (jumpAction != null) jumpAction.performed -= OnJump;
            if (interactAction != null) interactAction.performed -= OnInteract;
            if (pauseAction != null) pauseAction.performed -= OnPause;
        }

        private void OnJump(InputAction.CallbackContext _) => jumpPressed = true;
        private void OnInteract(InputAction.CallbackContext _) => interactPressed = true;
        private void OnPause(InputAction.CallbackContext _) => pausePressed = true;
        private static bool Consume(ref bool value) { bool result = value; value = false; return result; }
    }
}

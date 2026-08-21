using Unity.Cinemachine;
using UnityEngine;
using Robot.Input;

namespace Robot.Player.CameraControl
{
    /// <summary>
    /// Smooth 3rd-person orbital camera controlled by mouse for world exploration.
    /// Provides smooth, controlled mouse orbit without wild camera jumps.
    /// </summary>
    [RequireComponent(typeof(CinemachineCamera), typeof(CinemachineOrbitalFollow))]
    public sealed class ThirdPersonCameraController : MonoBehaviour
    {
        [Header("Target & Offset")]
        [SerializeField] private Transform target;
        [SerializeField] private Vector3 targetOffset = new Vector3(0f, 1.2f, 0f);

        [Header("Distance & Zoom")]
        [SerializeField, Min(0.5f)] private float defaultDistance = 5.0f;
        [SerializeField, Min(0.5f)] private float minDistance = 2.5f;
        [SerializeField, Min(0.5f)] private float maxDistance = 10.0f;
        [SerializeField, Min(0.1f)] private float zoomSpeed = 2.0f;

        [Header("Pitch & Yaw Limits")]
        [SerializeField] private float defaultPitch = 15.0f;
        [SerializeField] private float minVerticalAngle = -10.0f;
        [SerializeField] private float maxVerticalAngle = 60.0f;

        [Header("Mouse Sensitivity")]
        [SerializeField] private bool enableMouseLook = true;
        [SerializeField, Min(0.01f)] private float mouseLookSensitivity = 0.2f;

        private CinemachineCamera virtualCamera;
        private CinemachineOrbitalFollow orbitalFollow;
        private CinemachineDeoccluder deoccluder;
        private PlayerInputReader input;

        private Transform cameraPivotTarget;
        private float yaw;
        private float pitch;
        private float distance;

        private void Awake()
        {
            virtualCamera = GetComponent<CinemachineCamera>();
            orbitalFollow = GetComponent<CinemachineOrbitalFollow>();
            deoccluder = GetComponent<CinemachineDeoccluder>();

            distance = defaultDistance;
            pitch = defaultPitch;
            ResolveInput();
            ConfigureDeoccluderAndOrbital();
        }

        private void Start()
        {
            ResolveInput();
            AssignTarget();
            ConfigureDeoccluderAndOrbital();

            if (target != null)
            {
                yaw = target.eulerAngles.y;
            }
            ApplyOrbit();
        }

        private void LateUpdate()
        {
            ResolveInput();
            AssignTarget();
            UpdatePivotTargetPosition();

            HandleCursorLocking();

            if (enableMouseLook)
            {
                Vector2 lookDelta = GetLookDelta();
                if (lookDelta.sqrMagnitude > 0.0001f)
                {
                    yaw += lookDelta.x * mouseLookSensitivity;
                    pitch = Mathf.Clamp(pitch - lookDelta.y * mouseLookSensitivity, minVerticalAngle, maxVerticalAngle);
                }
            }

            float scroll = GetZoomInput();
            if (Mathf.Abs(scroll) > 0.001f)
            {
                distance = Mathf.Clamp(distance - scroll * zoomSpeed, minDistance, maxDistance);
            }

            ApplyOrbit();
        }

        public void SetTarget(Transform value)
        {
            target = value;
            ResolveInput();
            AssignTarget();
            ConfigureDeoccluderAndOrbital();
            if (target != null)
            {
                yaw = target.eulerAngles.y;
            }
            ApplyOrbit();
        }

        private void ResolveInput()
        {
            if (input == null && target != null)
            {
                input = target.GetComponent<PlayerInputReader>();
            }
        }

        private void HandleCursorLocking()
        {
            // Lock cursor on Right Click or Left Click in Game view
            if (UnityEngine.Input.GetMouseButtonDown(1) || UnityEngine.Input.GetMouseButtonDown(0))
            {
                LockCursor();
            }
            // Unlock cursor on ESC key
            else if (UnityEngine.Input.GetKeyDown(KeyCode.Escape))
            {
                UnlockCursor();
            }
        }

        private static void LockCursor()
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        private static void UnlockCursor()
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        private Vector2 GetLookDelta()
        {
            // Read mouse delta smoothly without artificial multiplier
            float mouseX = UnityEngine.Input.GetAxis("Mouse X");
            float mouseY = UnityEngine.Input.GetAxis("Mouse Y");

            if (Mathf.Abs(mouseX) > 0.0001f || Mathf.Abs(mouseY) > 0.0001f)
            {
                // Clamp delta per frame to prevent wild camera snaps
                mouseX = Mathf.Clamp(mouseX, -20f, 20f);
                mouseY = Mathf.Clamp(mouseY, -20f, 20f);
                return new Vector2(mouseX, mouseY);
            }

            if (input != null && input.LookInput.sqrMagnitude > 0.0001f)
            {
                return input.LookInput;
            }

            return Vector2.zero;
        }

        private float GetZoomInput()
        {
            float scroll = UnityEngine.Input.GetAxis("Mouse ScrollWheel");
            if (Mathf.Abs(scroll) > 0.001f)
            {
                return scroll;
            }

            if (input != null && Mathf.Abs(input.ZoomInput) > 0.001f)
            {
                return input.ZoomInput;
            }

            return 0f;
        }

        private void UpdatePivotTargetPosition()
        {
            if (cameraPivotTarget != null && target != null)
            {
                cameraPivotTarget.position = target.position + targetOffset;
                cameraPivotTarget.rotation = Quaternion.identity;
            }
        }

        private void AssignTarget()
        {
            if (virtualCamera == null || target == null) return;

            if (cameraPivotTarget == null)
            {
                GameObject pivotGo = GameObject.Find("RobotCameraPivotTarget");
                if (pivotGo == null)
                {
                    pivotGo = new GameObject("RobotCameraPivotTarget");
                }
                cameraPivotTarget = pivotGo.transform;
            }

            UpdatePivotTargetPosition();

            if (virtualCamera.Follow != cameraPivotTarget) virtualCamera.Follow = cameraPivotTarget;
            if (virtualCamera.LookAt != cameraPivotTarget) virtualCamera.LookAt = cameraPivotTarget;
        }

        private void ConfigureDeoccluderAndOrbital()
        {
            if (deoccluder == null) deoccluder = GetComponent<CinemachineDeoccluder>();
            if (deoccluder != null)
            {
                deoccluder.IgnoreTag = "Player";
                deoccluder.MinimumDistanceFromTarget = 0.8f;
            }

            if (orbitalFollow == null) orbitalFollow = GetComponent<CinemachineOrbitalFollow>();
            if (orbitalFollow != null)
            {
                orbitalFollow.OrbitStyle = CinemachineOrbitalFollow.OrbitStyles.Sphere;
                orbitalFollow.TargetOffset = Vector3.zero;
            }
        }

        private void ApplyOrbit()
        {
            if (orbitalFollow == null) return;

            orbitalFollow.OrbitStyle = CinemachineOrbitalFollow.OrbitStyles.Sphere;
            orbitalFollow.TargetOffset = Vector3.zero;
            orbitalFollow.Radius = distance;

            orbitalFollow.HorizontalAxis.Value = yaw;
            orbitalFollow.HorizontalAxis.Wrap = true;
            orbitalFollow.HorizontalAxis.Range = new Vector2(-180f, 180f);

            orbitalFollow.VerticalAxis.Value = pitch;
            orbitalFollow.VerticalAxis.Wrap = false;
            orbitalFollow.VerticalAxis.Range = new Vector2(minVerticalAngle, maxVerticalAngle);
        }
    }
}

using Unity.Cinemachine;
using UnityEngine;
using Robot.Input;

namespace Robot.Player.CameraControl
{
    /// <summary>
    /// Professional Third-Person Camera Controller:
    /// - Zeroes out Cinemachine double-damping & double-offset conflicts for exact chest-level framing (1.2m)
    /// - Smooth position follow (positionDamping)
    /// - Responsive, smooth mouse orbit (yaw & pitch)
    /// - Camera-relative movement support
    /// - Smooth auto-aligning behind player movement heading when walking
    /// </summary>
    [RequireComponent(typeof(CinemachineCamera), typeof(CinemachineOrbitalFollow))]
    public sealed class ThirdPersonCameraController : MonoBehaviour
    {
        [Header("Target & Framing Offset")]
        [SerializeField] private Transform target;
        [SerializeField] private Vector3 targetOffset = new Vector3(0f, 1.2f, 0f);

        [Header("Distance & Zoom")]
        [SerializeField, Min(0.5f)] private float defaultDistance = 5.0f;
        [SerializeField, Min(0.5f)] private float minDistance = 2.0f;
        [SerializeField, Min(0.5f)] private float maxDistance = 10.0f;
        [SerializeField, Min(0.1f)] private float zoomSpeed = 2.0f;

        [Header("Pitch Limits")]
        [SerializeField] private float defaultPitch = 15.0f;
        [SerializeField] private float minPitch = -10.0f;
        [SerializeField] private float maxPitch = 60.0f;

        [Header("Mouse Orbit Sensitivity")]
        [SerializeField] private bool enableMouseLook = true;
        [SerializeField, Min(0.01f)] private float mouseSensitivity = 0.15f;

        [Header("Smooth Damping Times")]
        [SerializeField, Min(0.01f)] private float positionDamping = 0.05f;
        [SerializeField, Min(0.01f)] private float rotationDamping = 0.05f;

        [Header("Auto-Align Behind Player")]
        [SerializeField] private bool enableAutoAlign = true;
        [SerializeField, Min(0.1f)] private float autoAlignDelay = 0.4f;
        [SerializeField, Min(0.01f)] private float autoAlignDamping = 0.3f;

        private CinemachineCamera virtualCamera;
        private CinemachineOrbitalFollow orbitalFollow;
        private CinemachineDeoccluder deoccluder;
        private PlayerInputReader input;

        private Transform cameraPivotTarget;
        private Vector3 currentPivotPosition;
        private Vector3 positionVelocity;

        private float targetYaw;
        private float currentYaw;
        private float yawVelocity;

        private float targetPitch;
        private float currentPitch;
        private float pitchVelocity;

        private float distance;
        private float autoAlignVelocity;
        private float lastMouseInputTime;

        private void Awake()
        {
            virtualCamera = GetComponent<CinemachineCamera>();
            orbitalFollow = GetComponent<CinemachineOrbitalFollow>();
            deoccluder = GetComponent<CinemachineDeoccluder>();

            distance = defaultDistance;
            targetPitch = defaultPitch;
            currentPitch = defaultPitch;

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
                targetYaw = target.eulerAngles.y;
                currentYaw = targetYaw;
                currentPivotPosition = target.position + targetOffset;
            }

            ApplyOrbit();
        }

        private void LateUpdate()
        {
            ResolveInput();
            AssignTarget();
            if (target == null) return;

            UpdateSmoothPivotPosition();
            HandleMouseLookAndAutoAlign();
            HandleZoom();
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
                targetYaw = target.eulerAngles.y;
                currentYaw = targetYaw;
                currentPivotPosition = target.position + targetOffset;
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

        private void UpdateSmoothPivotPosition()
        {
            if (cameraPivotTarget == null || target == null) return;

            Vector3 desiredPivotPosition = target.position + targetOffset;
            currentPivotPosition = Vector3.SmoothDamp(currentPivotPosition, desiredPivotPosition, ref positionVelocity, positionDamping);

            cameraPivotTarget.position = currentPivotPosition;
            cameraPivotTarget.rotation = Quaternion.identity;
        }

        private void HandleMouseLookAndAutoAlign()
        {
            float mouseX = UnityEngine.Input.GetAxis("Mouse X");
            float mouseY = UnityEngine.Input.GetAxis("Mouse Y");

            bool hasMouseInput = enableMouseLook && (Mathf.Abs(mouseX) > 0.0001f || Mathf.Abs(mouseY) > 0.0001f);
            bool isMoving = input != null && input.MoveInput.sqrMagnitude > 0.01f;

            if (hasMouseInput)
            {
                lastMouseInputTime = Time.time;
                targetYaw += mouseX * mouseSensitivity * 10f;
                targetPitch = Mathf.Clamp(targetPitch - mouseY * mouseSensitivity * 10f, minPitch, maxPitch);
            }
            else if (enableAutoAlign && isMoving && (Time.time - lastMouseInputTime > autoAlignDelay))
            {
                float playerHeading = target.eulerAngles.y;
                targetYaw = Mathf.SmoothDampAngle(targetYaw, playerHeading, ref autoAlignVelocity, autoAlignDamping);
            }

            currentYaw = Mathf.SmoothDampAngle(currentYaw, targetYaw, ref yawVelocity, rotationDamping);
            currentPitch = Mathf.SmoothDampAngle(currentPitch, targetPitch, ref pitchVelocity, rotationDamping);
        }

        private void HandleZoom()
        {
            float scroll = UnityEngine.Input.GetAxis("Mouse ScrollWheel");
            if (input != null && Mathf.Abs(input.ZoomInput) > 0.001f)
            {
                scroll = input.ZoomInput;
            }

            if (Mathf.Abs(scroll) > 0.001f)
            {
                distance = Mathf.Clamp(distance - scroll * zoomSpeed, minDistance, maxDistance);
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
                
                // Zero out Cinemachine's internal target offset and damping to avoid double-offset and double-damping!
                orbitalFollow.TargetOffset = Vector3.zero;
                orbitalFollow.TrackerSettings.PositionDamping = Vector3.zero;
                orbitalFollow.TrackerSettings.RotationDamping = Vector3.zero;
                orbitalFollow.TrackerSettings.QuaternionDamping = 0f;
            }
        }

        private void ApplyOrbit()
        {
            if (orbitalFollow == null) return;

            orbitalFollow.OrbitStyle = CinemachineOrbitalFollow.OrbitStyles.Sphere;
            orbitalFollow.TargetOffset = Vector3.zero;
            orbitalFollow.Radius = distance;

            orbitalFollow.HorizontalAxis.Value = currentYaw;
            orbitalFollow.HorizontalAxis.Wrap = true;
            orbitalFollow.HorizontalAxis.Range = new Vector2(-180f, 180f);

            orbitalFollow.VerticalAxis.Value = currentPitch;
            orbitalFollow.VerticalAxis.Wrap = false;
            orbitalFollow.VerticalAxis.Range = new Vector2(minPitch, maxPitch);
        }
    }
}

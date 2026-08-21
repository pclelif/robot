using Unity.Cinemachine;
using UnityEngine;

namespace Robot.Player.CameraControl
{
    /// <summary>
    /// Smooth Chase Third-Person Camera:
    /// - Mouse camera interaction is COMPLETELY AND PERMANENTLY REMOVED.
    /// - Character turns its body visibly first on key press, then camera smoothly glides behind character's back.
    /// </summary>
    [RequireComponent(typeof(CinemachineCamera), typeof(CinemachineOrbitalFollow))]
    public sealed class ThirdPersonCameraController : MonoBehaviour
    {
        [Header("Target & Offset")]
        [SerializeField] private Transform target;
        [SerializeField] private Vector3 targetOffset = new Vector3(0f, 1.2f, 0f);

        [Header("Distance & Pitch")]
        [SerializeField, Min(0.5f)] private float defaultDistance = 5.0f;
        [SerializeField] private float defaultPitch = 15.0f;

        [Header("Camera Damping")]
        [SerializeField, Min(0.01f)] private float positionDamping = 0.05f;
        [SerializeField, Min(0.01f)] private float rotationDamping = 0.45f;

        private CinemachineCamera virtualCamera;
        private CinemachineOrbitalFollow orbitalFollow;
        private CinemachineDeoccluder deoccluder;

        private Transform cameraPivotTarget;
        private Vector3 currentPivotPosition;
        private Vector3 positionVelocity;

        private float currentYaw;
        private float yawVelocity;

        private void Awake()
        {
            virtualCamera = GetComponent<CinemachineCamera>();
            orbitalFollow = GetComponent<CinemachineOrbitalFollow>();
            deoccluder = GetComponent<CinemachineDeoccluder>();

            // Mouse look is completely disabled. Cursor is free and visible.
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;

            ConfigureDeoccluderAndOrbital();
        }

        private void Start()
        {
            AssignTarget();
            ConfigureDeoccluderAndOrbital();

            if (target != null)
            {
                currentYaw = target.eulerAngles.y;
                currentPivotPosition = target.position + targetOffset;
            }

            ApplyOrbit();
        }

        private void LateUpdate()
        {
            AssignTarget();
            if (target == null) return;

            UpdatePivotAndRotation();
            ApplyOrbit();
        }

        public void SetTarget(Transform value)
        {
            target = value;
            AssignTarget();
            ConfigureDeoccluderAndOrbital();

            if (target != null)
            {
                currentYaw = target.eulerAngles.y;
                currentPivotPosition = target.position + targetOffset;
            }

            ApplyOrbit();
        }

        private void UpdatePivotAndRotation()
        {
            if (cameraPivotTarget == null || target == null) return;

            // 1. Position follow with smooth damping
            Vector3 desiredPivotPosition = target.position + targetOffset;
            currentPivotPosition = Vector3.SmoothDamp(currentPivotPosition, desiredPivotPosition, ref positionVelocity, positionDamping);
            cameraPivotTarget.position = currentPivotPosition;

            // 2. Yaw rotation smoothly glides behind character's rotation after character turns
            float targetYaw = target.eulerAngles.y;
            currentYaw = Mathf.SmoothDampAngle(currentYaw, targetYaw, ref yawVelocity, rotationDamping);
            cameraPivotTarget.rotation = Quaternion.Euler(0f, currentYaw, 0f);
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
            orbitalFollow.Radius = defaultDistance;

            orbitalFollow.HorizontalAxis.Value = currentYaw;
            orbitalFollow.HorizontalAxis.Wrap = true;
            orbitalFollow.HorizontalAxis.Range = new Vector2(-180f, 180f);

            orbitalFollow.VerticalAxis.Value = defaultPitch;
            orbitalFollow.VerticalAxis.Wrap = false;
            orbitalFollow.VerticalAxis.Range = new Vector2(-89f, 89f);
        }
    }
}

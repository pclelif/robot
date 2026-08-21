using Unity.Cinemachine;
using UnityEngine;

namespace Robot.Player.CameraControl
{
    /// <summary>
    /// Fixed-orientation 3rd-person camera that stays behind and above the robot at a natural exploration angle (15° pitch, 5m distance, 1.2m chest height).
    /// Mouse look is completely disabled to preserve rock-solid camera stability.
    /// </summary>
    [RequireComponent(typeof(CinemachineCamera), typeof(CinemachineOrbitalFollow))]
    public sealed class ThirdPersonCameraController : MonoBehaviour
    {
        [Header("Target & Offset")]
        [SerializeField] private Transform target;
        [SerializeField] private Vector3 targetOffset = new Vector3(0f, 1.2f, 0f);

        [Header("Distance & Pitch Angle")]
        [SerializeField, Min(0.5f)] private float defaultDistance = 5.0f;
        [SerializeField] private float defaultPitch = 15.0f;
        [SerializeField] private float defaultYaw = 0.0f;

        private CinemachineCamera virtualCamera;
        private CinemachineOrbitalFollow orbitalFollow;
        private CinemachineDeoccluder deoccluder;

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
            yaw = defaultYaw;

            // Ensure cursor is unlocked and visible
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;

            ConfigureDeoccluderAndOrbital();
        }

        private void Start()
        {
            AssignTarget();
            ConfigureDeoccluderAndOrbital();
            ApplyOrbit();
        }

        private void LateUpdate()
        {
            AssignTarget();
            UpdatePivotTargetPosition();
            ApplyOrbit();
        }

        public void SetTarget(Transform value)
        {
            target = value;
            AssignTarget();
            ConfigureDeoccluderAndOrbital();
            ApplyOrbit();
        }

        private void UpdatePivotTargetPosition()
        {
            if (cameraPivotTarget != null && target != null)
            {
                // Position tracks robot chest height, rotation stays world-aligned (identity)
                // so the camera does NOT spin when the robot turns!
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
            orbitalFollow.VerticalAxis.Range = new Vector2(-89f, 89f);
        }
    }
}

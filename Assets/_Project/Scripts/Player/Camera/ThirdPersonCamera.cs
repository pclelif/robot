using UnityEngine;

namespace Robot.Player.CameraControl
{
    public class ThirdPersonCamera : MonoBehaviour
    {
        [Header("Target Configuration")]
        [SerializeField] private Transform target;
        [SerializeField] private Vector3 targetOffset = new Vector3(0f, 1.5f, 0f);

        [Header("Orbit & Distance Settings")]
        [SerializeField] private float defaultDistance = 4.5f;
        [SerializeField] private float minDistance = 1.0f;
        [SerializeField] private float maxDistance = 8.0f;
        [SerializeField] private float pitchMin = -20.0f;
        [SerializeField] private float pitchMax = 70.0f;

        [Header("Sensitivity")]
        [SerializeField] private float mouseSensitivityX = 3.0f;
        [SerializeField] private float mouseSensitivityY = 2.5f;
        [SerializeField] private float touchSensitivity = 0.2f;

        [Header("Camera Collision Prevention")]
        [SerializeField] private LayerMask obstacleLayers;
        [SerializeField] private float sphereRadius = 0.25f;
        [SerializeField] private float distanceSmoothSpeed = 12.0f;
        [SerializeField] private float rotationSmoothSpeed = 15.0f;

        // External Touch Input Vector (Mobile)
        public Vector2 MobileLookInput { get; set; }

        private float currentYaw;
        private float currentPitch;
        private float currentDistance;
        private float targetDistance;

        private void Start()
        {
            currentDistance = defaultDistance;
            targetDistance = defaultDistance;

            Vector3 angles = transform.eulerAngles;
            currentYaw = angles.y;
            currentPitch = angles.x;

            if (target == null)
            {
                GameObject player = GameObject.FindGameObjectWithTag("Player");
                if (player != null)
                {
                    target = player.transform;
                }
            }
        }

        private void LateUpdate()
        {
            if (target == null) return;

            HandleInput();
            CalculateCollisionDistance();
            UpdateCameraTransform();
        }

        private void HandleInput()
        {
            // Read Mouse Input
            float inputYaw = Input.GetAxis("Mouse X") * mouseSensitivityX;
            float inputPitch = -Input.GetAxis("Mouse Y") * mouseSensitivityY;

            // Add Mobile Touch Look Input
            inputYaw += MobileLookInput.x * touchSensitivity;
            inputPitch += -MobileLookInput.y * touchSensitivity;

            currentYaw += inputYaw;
            currentPitch += inputPitch;
            currentPitch = Mathf.Clamp(currentPitch, pitchMin, pitchMax);

            // Reset Mobile Look Input after consuming
            MobileLookInput = Vector2.zero;
        }

        private void CalculateCollisionDistance()
        {
            Vector3 focusPoint = target.position + targetOffset;
            Quaternion rotation = Quaternion.Euler(currentPitch, currentYaw, 0f);
            Vector3 desiredCameraPos = focusPoint - (rotation * Vector3.forward * defaultDistance);
            Vector3 rayDirection = (desiredCameraPos - focusPoint).normalized;

            if (Physics.SphereCast(focusPoint, sphereRadius, rayDirection, out RaycastHit hit, defaultDistance, obstacleLayers))
            {
                targetDistance = Mathf.Clamp(hit.distance - sphereRadius, minDistance, maxDistance);
            }
            else
            {
                targetDistance = defaultDistance;
            }

            currentDistance = Mathf.Lerp(currentDistance, targetDistance, Time.deltaTime * distanceSmoothSpeed);
        }

        private void UpdateCameraTransform()
        {
            Vector3 focusPoint = target.position + targetOffset;
            Quaternion rotation = Quaternion.Euler(currentPitch, currentYaw, 0f);
            Vector3 finalPosition = focusPoint - (rotation * Vector3.forward * currentDistance);

            transform.rotation = Quaternion.Slerp(transform.rotation, rotation, Time.deltaTime * rotationSmoothSpeed);
            transform.position = finalPosition;
        }

        public void SetTarget(Transform newTarget)
        {
            target = newTarget;
        }
    }
}

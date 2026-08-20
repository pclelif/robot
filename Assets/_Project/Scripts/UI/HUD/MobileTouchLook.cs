using UnityEngine;
using UnityEngine.EventSystems;
using Robot.Player.CameraControl;

namespace Robot.UI.HUD
{
    public class MobileTouchLook : MonoBehaviour, IDragHandler, IPointerDownHandler, IPointerUpHandler
    {
        [Header("Target Camera")]
        [SerializeField] private ThirdPersonCamera targetCamera;
        [SerializeField] private float touchSensitivity = 1.0f;

        private Vector2 previousPointerPosition;
        private bool isDragging;

        private void Start()
        {
            if (targetCamera == null)
            {
                targetCamera = FindFirstObjectByType<ThirdPersonCamera>();
            }
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            isDragging = true;
            previousPointerPosition = eventData.position;
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (!isDragging) return;

            Vector2 delta = eventData.position - previousPointerPosition;
            previousPointerPosition = eventData.position;

            if (targetCamera != null)
            {
                targetCamera.MobileLookInput = delta * touchSensitivity;
            }
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            isDragging = false;
            if (targetCamera != null)
            {
                targetCamera.MobileLookInput = Vector2.zero;
            }
        }
    }
}

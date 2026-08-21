using UnityEngine;
using UnityEngine.EventSystems;
using Robot.Input;

namespace Robot.UI.HUD
{
    public class MobileTouchLook : MonoBehaviour, IDragHandler, IPointerDownHandler, IPointerUpHandler
    {
        [SerializeField] private PlayerInputReader inputReader;
        [SerializeField] private float touchSensitivity = 1.0f;

        private Vector2 previousPointerPosition;
        private bool isDragging;

        private void Start()
        {
        }

        public void Configure(PlayerInputReader reader) => inputReader = reader;

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

            if (inputReader != null)
            {
                inputReader.SetMobileLook(delta * touchSensitivity);
            }
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            isDragging = false;
            if (inputReader != null)
            {
                inputReader.SetMobileLook(Vector2.zero);
            }
        }
    }
}

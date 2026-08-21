using UnityEngine;
using UnityEngine.EventSystems;
using Robot.Player.Movement;

namespace Robot.UI.HUD
{
    public class VirtualJoystick : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler
    {
        [Header("Joystick UI Elements")]
        [SerializeField] private RectTransform joystickBackground;
        [SerializeField] private RectTransform joystickHandle;
        [SerializeField] private float handleRange = 100.0f;

        [Header("Player Movement Target")]
        [SerializeField] private RobotMovementController movementController;

        public Vector2 InputVector { get; private set; }

        private Vector2 joystickCenter;

        private void Start()
        {
            if (joystickBackground == null)
            {
                joystickBackground = GetComponent<RectTransform>();
            }

            if (movementController == null)
            {
                movementController = FindFirstObjectByType<RobotMovementController>();
            }
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            OnDrag(eventData);
        }

        public void OnDrag(PointerEventData eventData)
        {
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                joystickBackground,
                eventData.position,
                eventData.pressEventCamera,
                out Vector2 position
            );

            float bgWidth = joystickBackground != null && joystickBackground.rect.width > 0 ? joystickBackground.rect.width : (joystickBackground != null && joystickBackground.sizeDelta.x > 0 ? joystickBackground.sizeDelta.x : 100f);
            float bgHeight = joystickBackground != null && joystickBackground.rect.height > 0 ? joystickBackground.rect.height : (joystickBackground != null && joystickBackground.sizeDelta.y > 0 ? joystickBackground.sizeDelta.y : 100f);

            position.x = position.x / bgWidth;
            position.y = position.y / bgHeight;

            Vector2 calculatedInput = new Vector2(position.x * 2f, position.y * 2f);
            if (float.IsNaN(calculatedInput.x) || float.IsNaN(calculatedInput.y))
            {
                calculatedInput = Vector2.zero;
            }

            InputVector = (calculatedInput.magnitude > 1.0f) ? calculatedInput.normalized : calculatedInput;

            if (joystickHandle != null)
            {
                joystickHandle.anchoredPosition = new Vector2(
                    InputVector.x * (bgWidth / 2f) * (handleRange / 100f),
                    InputVector.y * (bgHeight / 2f) * (handleRange / 100f)
                );
            }

            if (movementController != null)
            {
                movementController.MovementInput = InputVector;
            }
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            InputVector = Vector2.zero;
            if (joystickHandle != null)
            {
                joystickHandle.anchoredPosition = Vector2.zero;
            }

            if (movementController != null)
            {
                movementController.MovementInput = Vector2.zero;
            }
        }
    }
}

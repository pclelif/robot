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

            position.x = (position.x / joystickBackground.sizeDelta.x);
            position.y = (position.y / joystickBackground.sizeDelta.y);

            InputVector = new Vector2(position.x * 2f, position.y * 2f);
            InputVector = (InputVector.magnitude > 1.0f) ? InputVector.normalized : InputVector;

            if (joystickHandle != null)
            {
                joystickHandle.anchoredPosition = new Vector2(
                    InputVector.x * (joystickBackground.sizeDelta.x / 2f) * (handleRange / 100f),
                    InputVector.y * (joystickBackground.sizeDelta.y / 2f) * (handleRange / 100f)
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

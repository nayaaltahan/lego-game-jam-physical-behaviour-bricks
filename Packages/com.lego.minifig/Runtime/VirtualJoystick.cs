using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace LEGOMinifig
{
    public class VirtualJoystick : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler
    {
        public float Horizontal { get { return input.x; } }
        public float Vertical { get { return input.y; } }
        public bool Jumped { get { return hasJumpInput; } }

        [SerializeField] float deadZone = 0;
        [SerializeField] Button jumpButton;

        int draggingPointerId;
        bool draggingStick;
        Vector2 input = Vector2.zero;
        bool hasJumpInput;

        RectTransform background = null;
        RectTransform handle = null;
        RectTransform rectComponent = null;
        Canvas canvas;


        void Awake()
        {
            rectComponent = GetComponent<RectTransform>();
            canvas = GetComponentInParent<Canvas>();

            background = transform.GetChild(0).GetComponent<RectTransform>();
            handle = transform.GetChild(0).GetChild(0).GetComponent<RectTransform>();            
        }

        void OnEnable()
        {
            if (jumpButton != null)
            {
                jumpButton.onClick.AddListener(RegisterJump);
            }
        }

        void OnDisable()
        {
            if (jumpButton != null)
            {
                jumpButton.onClick.RemoveAllListeners();
            }
        }

        void LateUpdate()
        {
            hasJumpInput = false;
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            if (eventData.pointerPressRaycast.gameObject != jumpButton)
            {
                draggingStick = true;
                draggingPointerId = eventData.pointerId;
                OnDrag(eventData);
            }
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (draggingStick && eventData.pointerId == draggingPointerId)
            {
                Vector2 position = RectTransformUtility.WorldToScreenPoint(null, background.position);
                Vector2 radius = background.sizeDelta / 2;
                input = (eventData.position - position) / (radius * canvas.scaleFactor);

                if (input.magnitude > deadZone)
                {
                    if (input.magnitude > 1)
                    {
                        input = input.normalized;
                    }
                }
                else
                {
                    input = Vector2.zero;
                }

                handle.anchoredPosition = input * radius;
            }
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            if (eventData.pointerId == draggingPointerId)
            {
                draggingStick = false;

                input = Vector2.zero;
                handle.anchoredPosition = Vector2.zero;
            }
        }

        void RegisterJump()
        {
            hasJumpInput = true;
        }
    }
}
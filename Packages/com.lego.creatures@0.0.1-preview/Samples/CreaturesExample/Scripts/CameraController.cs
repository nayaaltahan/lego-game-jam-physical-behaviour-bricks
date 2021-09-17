using UnityEngine;
using UnityEngine.EventSystems;

namespace LEGO.Creatures.Sample
{
    public class CameraController : MonoBehaviour
    {
        [SerializeField]
        float distance = 4.0f;

        float zoomScalar;

        bool isEnabled;

        bool mouseRotationActive;
        Vector3 lastMousePosition;

        const float maxZoom = 1.0f;
        const float minZoom = 30.0f;
        const float buttonZoomSpeed = 8.0f;
        const float scrollZoomSpeed = 25f;

        const float minVerticalAngle = 0.0f;
        const float maxVerticalAngle = 75.0f;
        const float rotationSpeed = 90.0f;
        const float mouseRotationSpeed = 0.1f;

        enum CameraMovement
        {
            NotMoving,
            ZoomingIn,
            ZoomingOut,
            RotatingLeft,
            RotatingRight,
            RotatingUp,
            RotatingDown
        }

        CameraMovement currentMovement = CameraMovement.NotMoving;

        Vector3 objectCenter;
        Vector3 relativeToObject;

        Vector3 initialPosition;
        Quaternion initialRotation;

        void Start()
        {
            initialPosition = transform.position;
            initialRotation = transform.rotation;
        }

        void Update()
        {
            if (isEnabled)
            {
                InputHandler();
                MouseRotation();
                ScrollZoom();

                if (currentMovement != CameraMovement.NotMoving)
                {
                    KeyboardRotation();
                }
            }
        }

        void InputHandler()
        {
            if (Input.GetKeyDown(KeyCode.DownArrow)) { RotateDown(true); }
            else if (Input.GetKeyUp(KeyCode.DownArrow)) { RotateDown(false); }

            if (Input.GetKeyDown(KeyCode.UpArrow)) { RotateUp(true); }
            else if (Input.GetKeyUp(KeyCode.UpArrow)) { RotateUp(false); }

            if (Input.GetKeyDown(KeyCode.RightArrow)) { RotateRight(true); }
            else if (Input.GetKeyUp(KeyCode.RightArrow)) { RotateRight(false); }

            if (Input.GetKeyDown(KeyCode.LeftArrow)) { RotateLeft(true); }
            else if (Input.GetKeyUp(KeyCode.LeftArrow)) { RotateLeft(false); }

            if (Input.GetKeyDown(KeyCode.Plus)) { ZoomIn(true); }
            else if (Input.GetKeyUp(KeyCode.Plus)) { ZoomIn(false); }

            if (Input.GetKeyDown(KeyCode.Minus)) { ZoomOut(true); }
            else if (Input.GetKeyUp(KeyCode.Minus)) { ZoomOut(false); }
        }

        void ScrollZoom()
        {
            var scrollDelta = Input.mouseScrollDelta.y;

            zoomScalar -= scrollZoomSpeed * scrollDelta * Time.deltaTime;

            if (zoomScalar < maxZoom)
            {
                zoomScalar = maxZoom;
            }

            if (scrollDelta != 0)
            {
                transform.position = objectCenter + -transform.forward * zoomScalar;
            }
        }

        void MouseRotation()
        {
            var orbitDeltaX = 0.0f;
            var orbitDeltaY = 0.0f;

            if (Input.GetMouseButtonDown(0))
            {
                if (!EventSystem.current || !EventSystem.current.IsPointerOverGameObject(-1))
                {
                    mouseRotationActive = true;
                }
            }
            if (Input.GetMouseButton(0))
            {
                if (mouseRotationActive)
                {
                    var direction = Input.mousePosition - lastMousePosition;
                    orbitDeltaX = direction.x;
                    orbitDeltaY = -direction.y;
                }
            }
            if (Input.GetMouseButtonUp(0))
            {
                mouseRotationActive = false;
            }
            lastMousePosition = Input.mousePosition;

            // Rotate the camera.
            transform.RotateAround(objectCenter, Vector3.up, orbitDeltaX * mouseRotationSpeed);

            // Clamp the vertical rotation.
            var angles = transform.rotation.eulerAngles;
            var minVerticalAngleLimit = 90f + minVerticalAngle;
            var maxVerticalAngleLimit = 90f - maxVerticalAngle;

            if (angles.x + orbitDeltaY * mouseRotationSpeed < 270f + minVerticalAngleLimit && angles.x + orbitDeltaY * mouseRotationSpeed > 180f)
            {
                orbitDeltaY = (270f + minVerticalAngleLimit - angles.x) / mouseRotationSpeed;
            }
            else if (angles.x + orbitDeltaY * mouseRotationSpeed > 90f - maxVerticalAngleLimit && angles.x + orbitDeltaY * mouseRotationSpeed < 180f)
            {
                orbitDeltaY = (90f - maxVerticalAngleLimit - angles.x) / mouseRotationSpeed;
            }

            transform.RotateAround(objectCenter, transform.right, orbitDeltaY * mouseRotationSpeed);
        }

        void KeyboardRotation()
        {
            switch (currentMovement)
            {
                case CameraMovement.ZoomingIn:
                    if (zoomScalar >= maxZoom) { zoomScalar -= buttonZoomSpeed * Time.deltaTime; }
                    transform.position = objectCenter + -transform.forward * zoomScalar;
                    break;
                case CameraMovement.ZoomingOut:
                    if (zoomScalar <= minZoom) { zoomScalar += buttonZoomSpeed * Time.deltaTime; }
                    transform.position = objectCenter + -transform.forward * zoomScalar;
                    break;
                case CameraMovement.RotatingLeft:
                    relativeToObject = Quaternion.AngleAxis(rotationSpeed * Time.deltaTime, Vector3.up) * relativeToObject;
                    transform.position = objectCenter + relativeToObject;
                    break;
                case CameraMovement.RotatingRight:
                    relativeToObject = Quaternion.AngleAxis(-rotationSpeed * Time.deltaTime, Vector3.up) * relativeToObject;
                    transform.position = objectCenter + relativeToObject;
                    break;
                case CameraMovement.RotatingUp:
                    if (transform.rotation.eulerAngles.x < maxVerticalAngle || transform.rotation.eulerAngles.x > minVerticalAngle - 1.0f)
                    {
                        relativeToObject = Quaternion.AngleAxis(rotationSpeed * Time.deltaTime, transform.right) * relativeToObject;
                        transform.position = objectCenter + relativeToObject;
                    }
                    break;
                case CameraMovement.RotatingDown:
                    if (transform.rotation.eulerAngles.x > minVerticalAngle && transform.rotation.eulerAngles.x < maxVerticalAngle + 1.0f)
                    {
                        relativeToObject = Quaternion.AngleAxis(-rotationSpeed * Time.deltaTime, transform.right) * relativeToObject;
                        transform.position = objectCenter + relativeToObject;
                    }
                    break;
            }

            transform.LookAt(objectCenter);
        }

        public void FocusCamera(GameObject onObject)
        {
            var creatureController = onObject.GetComponent<CreatureController>();

            if (creatureController)
            {
                var objectBounds = creatureController.GetObjectBounds();
                objectCenter = objectBounds.center;

                transform.LookAt(objectCenter);

                var newPosition = objectCenter;
                var direction = (transform.position - newPosition).normalized;

                var newDistance = distance + Mathf.Max(objectBounds.size.x, objectBounds.size.y, objectBounds.size.z);
                newPosition += direction * newDistance;
                transform.position = newPosition;

                zoomScalar = newDistance;

                isEnabled = true;
            }
        }

        public void ResetPosition()
        {
            transform.position = initialPosition;
            transform.rotation = initialRotation;

            isEnabled = false;
        }

        public void RotateRight(bool enable)
        {
            relativeToObject = transform.position - objectCenter;
            currentMovement = enable ? CameraMovement.RotatingRight : CameraMovement.NotMoving;
        }

        public void RotateLeft(bool enable)
        {
            relativeToObject = transform.position - objectCenter;
            currentMovement = enable ? CameraMovement.RotatingLeft : CameraMovement.NotMoving;
        }

        public void RotateUp(bool enable)
        {
            relativeToObject = transform.position - objectCenter;
            currentMovement = enable ? CameraMovement.RotatingUp : CameraMovement.NotMoving;
        }

        public void RotateDown(bool enable)
        {
            relativeToObject = transform.position - objectCenter;
            currentMovement = enable ? CameraMovement.RotatingDown : CameraMovement.NotMoving;
        }

        public void ZoomIn(bool enable)
        {
            currentMovement = enable ? CameraMovement.ZoomingIn : CameraMovement.NotMoving;
        }

        public void ZoomOut(bool enable)
        {
            currentMovement = enable ? CameraMovement.ZoomingOut : CameraMovement.NotMoving;
        }
    }
}

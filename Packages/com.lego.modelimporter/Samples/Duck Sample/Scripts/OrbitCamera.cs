// Copyright (C) LEGO System A/S - All Rights Reserved
// Unauthorized copying of this file, via any medium is strictly prohibited

using UnityEngine;
using UnityEngine.EventSystems;

namespace LEGOModelImporter.DuckSample
{
    public class OrbitCamera : MonoBehaviour
    {
        public Transform Target;
        public float ZoomSpeed = 0.3f;
        public float OrbitSpeed = 0.1f;
        [Range(10f, 1000f)]
        public float MaxZoomDistance = 500f;
        [Range(0f, 500f)]
        public float MinZoomDistance = 100f;
        [Range(-85f, 85f)]
        public float MaxVerticalAngle = 70f;
        [Range(-85f, 85f)]
        public float MinVerticalAngle = -30f;

        [Range(1.0f, 20.0f)]
        public float ZoomStretchDistance = 1.0f;
        [Range(1.0f, 20.0f)]
        public float VerticalStretchAngleY = 5.0f;

        public bool UseTouch = false;
        public bool Interactive = true;
        public float GyroEnableTime = 2.0f;

        [HideInInspector] public float RotationRate = 0.0f;

        private AnimationCurve _stretchCurve;
        private Camera _camera;

        private bool _active;
        private Vector3 _lastMousePosition;

        private AnimationCurve _gyroEffectCurve;
        private float _gyroTime;
        private float _gyroEffect;
        private float _orbitGyroOffsetX;
        private float _orbitGyroOffsetY;

        float _currentZoomDelta = 0.0f;
        float _currentOrbitDeltaY = 0.0f;

        float _elasticityCooldown = 0.1f;
        float _currentElasticityElapsed = 0.0f;

        void Start()
        {
            _camera = GetComponent<Camera>();

            _gyroEffectCurve = AnimationCurve.EaseInOut(0.0f, 0.0f, 1.0f, 1.0f);
            _stretchCurve = AnimationCurve.EaseInOut(0.0f, 0.0f, 1.0f, 1.0f);

            if (SystemInfo.supportsGyroscope)
            {
                Input.gyro.enabled = true;
            }
        }

        void Update()
        {
            if (!Target)
            {
                return;
            }

            float orbitDeltaX = 0.0f;
            
            if(Interactive)
            {
                if (Input.touchSupported && UseTouch)
                {
                    if (Input.touchCount == 2)
                    {
                        // Store both touches.
                        Touch touchZero = Input.GetTouch(0);
                        Touch touchOne = Input.GetTouch(1);

                        // Find the position in the previous frame of each touch.
                        Vector2 touchZeroPrevPos = touchZero.position - touchZero.deltaPosition;
                        Vector2 touchOnePrevPos = touchOne.position - touchOne.deltaPosition;

                        // Find the magnitude of the vector (the distance) between the touches in each frame.
                        float prevTouchDeltaMag = (touchZeroPrevPos - touchOnePrevPos).magnitude;
                        float touchDeltaMag = (touchZero.position - touchOne.position).magnitude;

                        // Find the difference in the distances between each frame.
                        _currentZoomDelta = prevTouchDeltaMag - touchDeltaMag;
                    }
                    else if (Input.touchCount == 1)
                    {
                        Touch touch = Input.GetTouch(0);
                        switch (touch.phase)
                        {
                            case TouchPhase.Began:
                            {
                                if (!EventSystem.current || !EventSystem.current.IsPointerOverGameObject(0))
                                {
                                    _active = true;
                                }
                                break;
                            }
                            case TouchPhase.Moved:
                            {
                                if (_active)
                                {
                                    orbitDeltaX = touch.deltaPosition.x;
                                    _currentOrbitDeltaY = -touch.deltaPosition.y;
                                }
                                break;
                            }
                            case TouchPhase.Ended:
                            {
                                _active = false;
                                break;
                            }
                        }
                    }
                }
                else if (Input.mousePresent)
                {
                    _currentZoomDelta = Input.mouseScrollDelta.y;
                    if (Input.GetMouseButtonDown(1))
                    {
                        if (!EventSystem.current || !EventSystem.current.IsPointerOverGameObject(-1))
                        {
                            _active = true;
                        }
                    }
                    if (Input.GetMouseButton(1))
                    {
                        if (_active)
                        {
                            Vector3 direction = Input.mousePosition - _lastMousePosition;
                            orbitDeltaX = direction.x;
                            _currentOrbitDeltaY = -direction.y;
                        }
                    }
                    if (Input.GetMouseButtonUp(1))
                    {
                        _active = false;
                    }
                    _lastMousePosition = Input.mousePosition;
                }
            }

            // Offset camera based on gyro input.
            if (Input.gyro.enabled)
            {
                _gyroTime += Time.deltaTime;
                var ratio = Mathf.Min(1.0f, _gyroTime / GyroEnableTime);
                _gyroEffect = _gyroEffectCurve.Evaluate(ratio);

                // First undo offset from previous frame.
                orbitDeltaX -= _orbitGyroOffsetX;
                _currentOrbitDeltaY -= _orbitGyroOffsetY;

                Gyroscope gyro = Input.gyro;

                var cameraScale = 100.0f;

                _orbitGyroOffsetX = -gyro.gravity.x * cameraScale * _gyroEffect;
                _orbitGyroOffsetY = -gyro.gravity.z * cameraScale * _gyroEffect;

                orbitDeltaX += _orbitGyroOffsetX;
                _currentOrbitDeltaY += _orbitGyroOffsetY;

                // Save the rotation rate for shake detection
                RotationRate = gyro.rotationRate.magnitude;
            }

            // Rotate the camera.
            _camera.transform.RotateAround(Target.position, Vector3.up, orbitDeltaX * OrbitSpeed);

            ApplyOrbit();
            _camera.transform.LookAt(Target);

            ApplyZoom();
        }

        float ClampDelta(float delta, float value, float min, float max, float stretch)
        {
            float outDelta = delta;
            if (value < min)
            {
                if (outDelta < 0.0f)
                {
                    _currentElasticityElapsed = 0.0f;
                    if (value < min - stretch)
                    {
                        outDelta = 0.0f;
                    }
                    else
                    {
                        var ratio = Mathf.Abs(value - min) / stretch;
                        outDelta *= _stretchCurve.Evaluate(1.0f - ratio);
                    }
                }
                else if (outDelta == 0.0f)
                {
                    _currentElasticityElapsed += Time.deltaTime;
                    if (_currentElasticityElapsed >= _elasticityCooldown)
                    {
                        var ratio = value / (min - stretch);
                        outDelta = _stretchCurve.Evaluate(ratio) * 1.5f;
                    }
                }
            }
            else if (value > max)
            {
                if (outDelta > 0.0f)
                {
                    _currentElasticityElapsed = 0.0f;
                    if (value > max + stretch)
                    {
                        outDelta = 0.0f;
                    }
                    else
                    {
                        var ratio = Mathf.Abs(value - max) / stretch;
                        outDelta *= _stretchCurve.Evaluate(1.0f - ratio);
                    }
                }
                else if (outDelta == 0.0f)
                {
                    _currentElasticityElapsed += Time.deltaTime;
                    if(_currentElasticityElapsed >= _elasticityCooldown)
                    {
                        var ratio = value / (max + stretch);
                        outDelta = -_stretchCurve.Evaluate(ratio) * 1.5f;
                    }
                }
            }

            return outDelta;
        }

        void ApplyOrbit()
        {
            // Clamp the zoomDelta to make sure the camera stays between min and max distances.
            var diff = _camera.transform.position - Target.position;
            var dist = diff.magnitude;

            var angle = Mathf.Acos(new Vector3(diff.x, 0.0f, diff.z).magnitude / dist) * Mathf.Rad2Deg * Mathf.Sign(diff.y);
            var value = angle + _currentOrbitDeltaY * OrbitSpeed;

            _currentOrbitDeltaY = ClampDelta(_currentOrbitDeltaY, value, MinVerticalAngle, MaxVerticalAngle, VerticalStretchAngleY);

            _camera.transform.RotateAround(Target.position, _camera.transform.right, _currentOrbitDeltaY * OrbitSpeed);

            _currentOrbitDeltaY = 0.0f;
        }

        void ApplyZoom()
        {
            // Clamp the zoomDelta to make sure the camera stays between min and max distances.
            var diff = _camera.transform.position - Target.position;
            var dist = diff.magnitude;

            var value = dist +_currentZoomDelta * ZoomSpeed;

            _currentZoomDelta = ClampDelta(_currentZoomDelta, value, MinZoomDistance, MaxZoomDistance, ZoomStretchDistance);

            // Change the distance based on the change in distance between the touches.
            _camera.transform.Translate(Vector3.back * _currentZoomDelta * ZoomSpeed);

            _currentZoomDelta = 0.0f;
        }
    }

}
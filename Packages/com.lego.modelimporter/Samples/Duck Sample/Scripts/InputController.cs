// Copyright (C) LEGO System A/S - All Rights Reserved
// Unauthorized copying of this file, via any medium is strictly prohibited

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LEGOModelImporter.DuckSample
{
    /// <summary>
    /// Wrapper for input handling
    /// </summary>
    public class InputController : MonoBehaviour
    {
        public static InputController Instance { get; private set; }

        public float CurrentInputMoveDeltaMagnitude { get; private set; } = 0.0f;
        public Vector2 CurrentInputMoveDelta { get; private set; }
        
        Vector2 _pointerPosition = Vector2.zero;
        public Vector2 MousePosition { get { return _pointerPosition; } }

        /// <summary>
        /// All possible actions in the sample
        /// </summary>
        public enum InputAction
        {
            RotateLeft,
            RotateRight,
            HoldDown,
            Restart
        }

        Dictionary<InputAction, bool> _isActionDown = new Dictionary<InputAction, bool>();
        Dictionary<InputAction, bool> _isActionUp = new Dictionary<InputAction, bool>();
        Dictionary<InputAction, bool> _isAction = new Dictionary<InputAction, bool>();
        
        bool _canRotate = true;

        #region Settings
        [Header("Settings")]
        [Tooltip("Amount touch pointer needs to move to rotate.")]
        [Min(0.0f)] [SerializeField] float RotateDelta = 80.0f;
        [Tooltip("Cooldown period for rotation.")]
        [Min(0.0f)] [SerializeField] float RotateWaitTime = .6f;
        [Tooltip("Rate device needs to be shaken to trigger restart.")]
        [Min(0.0f)] [SerializeField] float ShakeRate = 6.5f;
        #endregion

        private void Awake()
        {
            if (Instance)
            {
                Destroy(Instance);
            }
            else
            {
                Instance = this;
            }
        }

        void Update()
        {
            RuntimeBrickBuilder.Instance.Orbit.UseTouch = !Input.mousePresent;
            UpdateInput();
        }


        void UpdateInputMove()
        {
            if(Input.touchCount > 0)
            {
                var touch = Input.touches[0];
                CurrentInputMoveDeltaMagnitude += touch.deltaPosition.magnitude;
                CurrentInputMoveDelta += touch.deltaPosition;

                _pointerPosition = touch.position;
            }
            else
            {
                var newMousePosition = Input.mousePosition;
                var mouseDelta = Vector2.Distance(newMousePosition, _pointerPosition);
                CurrentInputMoveDeltaMagnitude += mouseDelta;
                CurrentInputMoveDelta = new Vector2(newMousePosition.x, newMousePosition.y) - _pointerPosition;

                _pointerPosition = newMousePosition;
            }
        }

        public void ResetDelta()
        {
            CurrentInputMoveDeltaMagnitude = 0.0f;
            CurrentInputMoveDelta = Vector2.zero;
        }

        bool TouchUp(Touch touch)
        {
            return touch.phase == TouchPhase.Ended || touch.phase == TouchPhase.Canceled;
        }

        bool TouchDown(Touch touch)
        {
            return touch.phase == TouchPhase.Began;
        }

        bool Touch(Touch touch)
        {
            return touch.phase == TouchPhase.Moved || touch.phase == TouchPhase.Stationary || touch.phase == TouchPhase.Began;
        }

        IEnumerator DelayRotation()
        {
            _canRotate = false;
            yield return new WaitForSeconds(RotateWaitTime);
            _canRotate = true;
        }

        void UpdateInput()
        {
            UpdateInputMove();

            if(Input.gyro.enabled)
            {
                var currentShake = RuntimeBrickBuilder.Instance.Orbit.RotationRate;

                _isActionDown[InputAction.Restart] = currentShake > ShakeRate;
                _isAction[InputAction.Restart] = currentShake > ShakeRate;
                _isActionUp[InputAction.Restart] = currentShake < ShakeRate;
            }

            if (Input.touchCount > 0)
            {
                Touch touch = Input.touches[0];

                _isActionDown[InputAction.HoldDown] = TouchDown(touch);
                _isActionUp[InputAction.HoldDown] = TouchUp(touch);
                _isAction[InputAction.HoldDown] = Touch(touch);

                if (_canRotate)
                {
                    // Rotate if our delta on the x-axis is high enough
                    var delta = touch.deltaPosition.x;

                    // Or rotate by tapping on either the left or right side of the screen with a second finger
                    if(Input.touchCount == 2)
                    {
                        var xCoordinate = Input.touches[1].deltaPosition.x;
                        var halfWidth = Screen.width * .5f;
                        if(xCoordinate < halfWidth)
                        {
                            delta = -RotateDelta;
                        }
                        else
                        {
                            delta = RotateDelta;
                        }
                    }

                    if (delta <= -RotateDelta)
                    {
                        StartCoroutine(DelayRotation());
                        _isAction[InputAction.RotateLeft] = TouchDown(touch);
                        _isActionUp[InputAction.RotateLeft] = TouchUp(touch);
                        _isActionDown[InputAction.RotateLeft] = Touch(touch);
                    }
                    else if (delta >= RotateDelta)
                    {
                        StartCoroutine(DelayRotation());
                        _isAction[InputAction.RotateRight] = TouchDown(touch);
                        _isActionUp[InputAction.RotateRight] = TouchUp(touch);
                        _isActionDown[InputAction.RotateRight] = Touch(touch);
                    }
                    else
                    {
                        _isAction[InputAction.RotateLeft] = false;
                        _isActionUp[InputAction.RotateLeft] = false;
                        _isActionDown[InputAction.RotateLeft] = false;
                        _isAction[InputAction.RotateRight] = false;
                        _isActionUp[InputAction.RotateRight] = false;
                        _isActionDown[InputAction.RotateRight] = false;
                    }
                }
                else
                {
                    _isAction[InputAction.RotateLeft] = false;
                    _isActionUp[InputAction.RotateLeft] = false;
                    _isActionDown[InputAction.RotateLeft] = false;
                    _isAction[InputAction.RotateRight] = false;
                    _isActionUp[InputAction.RotateRight] = false;
                    _isActionDown[InputAction.RotateRight] = false;
                }
            }
            else if(Input.mousePresent)
            {
                _isActionDown[InputAction.Restart] = Input.GetKeyDown(KeyCode.R);
                _isAction[InputAction.Restart] = Input.GetKey(KeyCode.R);
                _isActionUp[InputAction.Restart] = Input.GetKeyUp(KeyCode.R);

                _isActionDown[InputAction.RotateLeft] = Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.LeftArrow);
                _isActionDown[InputAction.RotateRight] = Input.GetKeyDown(KeyCode.D) || Input.GetKeyDown(KeyCode.RightArrow) || Input.GetMouseButtonDown(1);
                _isActionDown[InputAction.HoldDown] = Input.GetMouseButtonDown(0);

                _isActionUp[InputAction.RotateLeft] = Input.GetKeyUp(KeyCode.A) || Input.GetKeyUp(KeyCode.LeftArrow);
                _isActionUp[InputAction.RotateRight] = Input.GetKeyUp(KeyCode.D) || Input.GetKeyUp(KeyCode.RightArrow) || Input.GetMouseButtonUp(1);
                _isActionUp[InputAction.HoldDown] = Input.GetMouseButtonUp(0);

                _isAction[InputAction.RotateLeft] = Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow);
                _isAction[InputAction.RotateRight] = Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow) || Input.GetMouseButton(1);
                _isAction[InputAction.HoldDown] = Input.GetMouseButton(0);
            }

            if (Input.touchCount == 0)
            {
                _canRotate = true;
            }
        }

        #region Query functions
        public bool ActionDown(InputAction action)
        {
            if (_isActionDown.TryGetValue(action, out bool down))
            {
                return down;
            }
            return false;
        }

        public bool ActionUp(InputAction action)
        {
            if (_isActionUp.TryGetValue(action, out bool down))
            {
                return down;
            }
            return false;
        }

        public bool Action(InputAction action)
        {
            if (_isAction.TryGetValue(action, out bool down))
            {
                return down;
            }
            return false;
        }
        #endregion
    }
}

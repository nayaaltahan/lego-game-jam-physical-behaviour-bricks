// Copyright (C) LEGO System A/S - All Rights Reserved
// Unauthorized copying of this file, via any medium is strictly prohibited

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace LEGOModelImporter.DuckSample
{
    public class RuntimeBrickBuilder : MonoBehaviour
    {
        public static RuntimeBrickBuilder Instance { get; private set; }

        enum SelectionState
        {
            NoSelection,
            DraggingBrick,
            MovingBrick,
            FinishedDuck
        }

        class Rotation
        {
            public float angle;
            public Vector3 pivot;
            public Vector3 axis;
        }

        [Header("Rendering")]
        [Tooltip("Render pipeline asset set at runtime.")]
        [SerializeField] RenderPipelineAsset _pipelineAsset;

        #region Animation Settings
        [Header("Animation Settings")]

        [Tooltip("Height that a brick selection is hovering to.")]
        [Min(0.0f)] [SerializeField] public float MaxHoverHeight = 0.07f;

        [Tooltip("Velocity of brick hover.")]
        [Min(0.0f)] [SerializeField] public float HoverVelocity = .5f;

        [Tooltip("Velocity of brick wobble.")]
        [Min(0.0f)] [SerializeField] public float WobbleVelocity = .5f;

        [Tooltip("Amount of brick wobble.")]
        [Min(0.0f)] [SerializeField] public float WobbleAmount = 8.0f;

        [Tooltip("Velocity brick positions are animated with towards their goal.")]
        [Min(0.0f)] [SerializeField] public float MoveVelocity = 10.0f;

        [Tooltip("Velocity brick rotations are animated with towards their goal.")]
        [Min(0.0f)] [SerializeField] public float RotationVelocity = 10.0f;

        [Tooltip("Duration of the final animation.")]
        [Min(0.0f)] [SerializeField] public float CelebrationJumpDuration = 0.5f;

        [Tooltip("Jump height of the final animation.")]
        [Min(0.0f)] [SerializeField] public float CelebrationJumpHeight = 2.1f;

        [Tooltip("Animation curve for hover.")]
        [SerializeField] public AnimationCurve HoverAnimationCurve;

        [Tooltip("Animation curve for wobble.")]
        [SerializeField] public AnimationCurve WobbleAnimationCurve;

        [Tooltip("Animation curve for final animation.")]
        [SerializeField] public AnimationCurve DuckAnimationCurve;

        private bool _playDuckAnimation = false;
        #endregion

        #region Scene
        Camera _sceneCamera;

        [Tooltip("Orbit camera object in the scene")]
        [SerializeField] OrbitCamera _orbitCamera;
        public OrbitCamera Orbit { get { return _orbitCamera; } }

        int _activeCounter = 0;
        Brick[] _bricksInScene = null;
        AnimatedGroup _animatedGroup = null;
        RigidbodyGroupPool _groupPool = null;
        #endregion

        #region Physics
        [Header("Rigidbody Settings")]
        [Tooltip("Mass of the rigidbodies.")]
        [Min(0.0f)] [SerializeField] public float Mass = 10.0f;

        [Tooltip("Drag of the rigidbodies.")]
        [Min(0.0f)] [SerializeField] public float Drag = 0.0f;

        [Tooltip("Angular drag of the rigidbodies.")]
        [Min(0.0f)] [SerializeField] public float AngularDrag = 0.5f;
        [Tooltip("Gravity of the physics scene.")]
        [SerializeField] public float Gravity = Physics.gravity.y;

        bool _forceFieldActive = false;
        #endregion

        #region Selection
        Brick _focusBrick = null;
        Brick _draggingBrick = null;
        SelectionState _currentSelectionState;
        HashSet<Brick> _selectedBricks = new HashSet<Brick>();
        #endregion

        #region Building
        bool _disableBuilding = true;
        float _holdTimeElapsed = 0.0f;
        Plane _worldPlane = new Plane(Vector3.up, 0.0f); // The building plane
        Vector3 _localPickup;
        Vector3 _placeOffset = Vector3.zero; // The "click" offset
        Vector3 _pickupOffset = Vector3.zero; // The offset from the brick position to the place we clicked on the brick with our mouse.
        RaycastHit _currentCollidingHit;
        Rotation _queuedRotation = null;
        BrickBuildingUtility.ConnectionResult _currentConnection = BrickBuildingUtility.ConnectionResult.Empty();

        public Vector3 PickupOffset => _pickupOffset;

        [Header("Building Settings")]

        [Tooltip("Time you have to hold over a brick to build it.")]
        [Min(0.0f)] [SerializeField] public float HoldTime = 1.0f;

        [Tooltip("Distance the brick has to be from the end goal to click.")]
        [Min(0.0f)] [SerializeField] public float ClickDistance = BrickBuildingUtility.LU_1;

        [Tooltip("The radius you can build in.")]
        [Min(0.0f)] [SerializeField] public float BuildingRadius = 8.0f;

        [Tooltip("Offset in screen space that the brick is above your pointer.")]
        [Min(0.0f)] [SerializeField] float YOffset = 5000.0f;

        #endregion

        private void OnValidate()
        {
            if (Instance == null)
            {
                Instance = this;
            }
        }

        void Awake()
        {
            if(Instance == null)
            {
                Instance = this;
            }

#if UNITY_EDITOR
            GraphicsSettings.renderPipelineAsset = _pipelineAsset;
            QualitySettings.renderPipeline = _pipelineAsset;
#endif

            _animatedGroup = AnimatedGroup.CreateAnimatedGroup();
            _groupPool = new RigidbodyGroupPool(6);

            _bricksInScene = FindObjectsOfType<Brick>();

            _sceneCamera = Camera.main;
            var topScreen = _sceneCamera.ScreenToWorldPoint(new Vector3(Screen.width * 0.5f, 0.0f, 0.0f));
            foreach (var brick in _bricksInScene)
            {
                var rigidbody = _groupPool.UseGroup(brick);

                brick.DisconnectAll();

                var point = UnityEngine.Random.onUnitSphere * 4.0f;
                rigidbody.transform.position = point + new Vector3(0.0f, topScreen.y, 0.0f);
                Vector3 noise = GenerateNoise(80.0f);
                var randomRot = Quaternion.Euler(noise);
                rigidbody.transform.rotation *= randomRot;

                rigidbody.IsKinematic = false;
            }

            _groupPool.Update();
            StartCoroutine(DelayBuilding(_bricksInScene));

            // In case any models were generated automatically or present, destroy them.
            var models = FindObjectsOfType<Model>();
            foreach(var model in models)
            {
                Destroy(model.gameObject);
            }
        }

        void Restart()
        {
            if(_currentSelectionState == SelectionState.FinishedDuck)
            {
                return;
            }

            // Return all rigidbodies to split up bricks
            foreach (var rb in _groupPool)
            {
                _groupPool.ReturnGroup(rb);
            }

            var bounds = BrickBuildingUtility.ComputeBounds(_bricksInScene);

            // Now disconnect all bricks from each other and add some force
            foreach (var brick in _bricksInScene)
            {
                brick.DisconnectAll();

                var rigidbody = _groupPool.UseGroup(brick);

                rigidbody.IsKinematic = false;
                rigidbody.Rigidbody.AddExplosionForce(UnityEngine.Random.value * 120.0f, bounds.center, bounds.extents.magnitude);
            }

            _groupPool.Update();
            StartCoroutine(DelayBuilding(_bricksInScene));
        }

        bool IsReadyToBuild(RigidbodyGroup group)
        {
            var distance = Vector3.Distance(_orbitCamera.Target.position, group.transform.position);
            var bounds = BrickBuildingUtility.ComputeBounds(group.GetComponentsInChildren<Brick>());
            var euler = group.transform.rotation.eulerAngles;

            var boundsCloseToZero = bounds.min.y >= -0.1f && bounds.min.y <= 0.1f;
            var eulerCloseToZero = euler.x >= -0.1f && euler.x <= 0.1f && euler.z >= -0.1f && euler.z <= 0.1f;

            return eulerCloseToZero && boundsCloseToZero && Vector3.Dot(group.transform.up, Vector3.up) > 0.98f && distance < BuildingRadius && group.Rigidbody.IsSleeping();
        }

        IEnumerator DelayBuilding(Brick[] realBricksInScene)
        {
            yield return new WaitForSeconds(.7f);
            _disableBuilding = false;
            foreach(var brick in realBricksInScene)
            {
                StartCoroutine(DelayBuilding(brick));
            }
        }

        IEnumerator DelayBuilding(Brick brick, float delay = 0.0f)
        {
            yield return new WaitForSeconds(delay);

            // Get a rigidbody for each brick as a starting point and create an impostor brick for animating
            var group = _groupPool.GetGroupFromBrick(brick);
            _animatedGroup.CreateAnimatedBrick(brick);

            while (true)
            {
                // We want to enable building as soon as a brick is lying still.
                if (group.Rigidbody.velocity.magnitude < 0.5f)
                {
                    _activeCounter++;
                    if (_activeCounter == 6)
                    {
                        _forceFieldActive = true;
                    }

                    group.IsKinematic = IsReadyToBuild(group);
                    if(group.IsKinematic)
                    {
                        break;
                    }
                }
                yield return null;
            }
        }

        void Update()
        {
            CheckRotation();
            var ray = GetRay();
            UpdateSelection(ray);

            if (_playDuckAnimation && _animatedGroup.CurrentAnimationState != AnimatedGroup.AnimationState.Snap)
            {
                _playDuckAnimation = false;
                AudioController.Instance.PlayQuack();
                _currentSelectionState = SelectionState.FinishedDuck;
                StartCoroutine(DuckAnimation());
            }
            _groupPool.Update();

            if (InputController.Instance.ActionDown(InputController.InputAction.Restart))
            {
                Restart();
            }
        }

        void FixedUpdate()
        {
            if(!_disableBuilding)
            {
                if(_currentSelectionState == SelectionState.MovingBrick)
                {
                    var ray = GetRay();
                    MoveBricks(ray);
                }

                foreach (var group in _groupPool)
                {
                    if (group.IsKinematic)
                    {
                        continue;
                    }

                    if (_focusBrick)
                    {
                        var focusGroup = _groupPool.GetGroupFromBrick(_focusBrick);
                        focusGroup.IsKinematic = true;
                        if (group == focusGroup)
                        {
                            continue;
                        }
                    }

                    group.IsKinematic = IsReadyToBuild(group);
                }
            }
        }

        void FindPreconnect()
        {
            if(!_focusBrick)
            {
                return;
            }

            HashSet<(Connection, Connection)> currentConnections = new HashSet<(Connection, Connection)>();

            // Get preconnect offset (Re-evaluate for cooler animations)
            foreach (var brick in _selectedBricks)
            {
                foreach (var part in brick.parts)
                {
                    if (!part.connectivity)
                    {
                        continue;
                    }

                    var connections = part.connectivity.QueryConnections(out bool reject);

                    foreach (var connection in connections)
                    {
                        currentConnections.Add(connection);
                    }
                }
            }

            var potentialPlaceOffset = Vector3.zero;
            var firstPlaceOffset = true;
            foreach ((Connection, Connection) connection in currentConnections)
            {
                var preconnectOffset = Vector3.zero;
                if (!_selectedBricks.Contains(connection.Item2.field.connectivity.part.brick))
                {
                    if (connection.Item1 is PlanarFeature p1 && connection.Item2 is PlanarFeature p2)
                    {
                        preconnectOffset = p2.GetPreconnectOffset() * 4.0f;
                    }

                    if (firstPlaceOffset)
                    {
                        potentialPlaceOffset = preconnectOffset;
                        firstPlaceOffset = false;
                    }
                    else
                    {
                        if ((preconnectOffset - potentialPlaceOffset).sqrMagnitude > 0.01f)
                        {
                            potentialPlaceOffset = Vector3.zero;
                            break;
                        }
                    }
                }
            }

            _placeOffset = potentialPlaceOffset;

            var rigidbody = _groupPool.GetGroupFromBrick(_focusBrick);
            rigidbody.transform.position += _placeOffset;
        }

        public Ray GetRay()
        {
            return _sceneCamera.ScreenPointToRay(InputController.Instance.MousePosition);
        }

        bool CanConnect(BrickBuildingUtility.ConnectionResult connection, RaycastHit collidingHit, out Vector3 pivot)
        {
            pivot = Vector3.zero;

            if (!connection.IsEmpty())
            {
                var src = connection.srcConnection;
                
                // Compute the pivot for the rotation
                pivot = src.field.connectivity.part.brick.transform.position + _pickupOffset;
                
                // Check if the bounds will connect under ground in hit local space
                var hitNormal = collidingHit.normal;
                var hitPoint = collidingHit.point;
                var transformation = Matrix4x4.TRS(hitPoint, Quaternion.FromToRotation(Vector3.up, hitNormal), Vector3.one).inverse;

                var bounds = BrickBuildingUtility.ComputeBounds(_selectedBricks);

                var rot = Quaternion.AngleAxis(connection.angleToConnect, connection.rotationAxisToConnect);

                var boundsMinPos = (rot * (bounds.min - pivot)) + pivot;
                boundsMinPos = boundsMinPos += connection.connectionOffset;
                boundsMinPos = transformation.MultiplyPoint(boundsMinPos);
                return boundsMinPos.y >= 0.0f || !collidingHit.transform;
            }
            return false;
        }

        void ComputeNewConnection(Ray ray)
        {
            if(!_focusBrick)
            {
                return;
            }

            var bounds = BrickBuildingUtility.ComputeBounds(_selectedBricks);

            var rigidbody = _groupPool.GetGroupFromBrick(_focusBrick);

            var oldPosition = rigidbody.transform.position;
            var oldRotation = rigidbody.transform.rotation;
            
            var rayScreenSpace = _sceneCamera.WorldToScreenPoint(ray.origin);
            var rayOffset = rayScreenSpace + new Vector3(0.0f, YOffset, 0.0f);
            ray.origin = _sceneCamera.ScreenToWorldPoint(rayOffset);

            var pivot = _pickupOffset + _focusBrick.transform.position;
            BrickBuildingUtility.AlignBricks(_focusBrick, _selectedBricks, bounds, pivot, _pickupOffset, ray, _worldPlane, 80.0f, out _, out Vector3 offset, out Vector3 prerotateOffset, out Quaternion rotation, out _currentCollidingHit);

            if (!_currentConnection.IsEmpty())
            {
                rigidbody.transform.position += prerotateOffset;
            }
            else
            {
                var localOffset = _focusBrick.transform.InverseTransformDirection(_pickupOffset);
                rotation.ToAngleAxis(out float alignedAngle, out Vector3 alignedAxis);

                rigidbody.transform.RotateAround(pivot, alignedAxis, alignedAngle);
                rigidbody.transform.position += offset;

                // Transform pickup offset back to world space for later use
                _pickupOffset = _focusBrick.transform.TransformDirection(localOffset);
            }

            Physics.SyncTransforms();

            bounds = BrickBuildingUtility.ComputeBounds(_selectedBricks);

            var canConnect = false;

            BrickBuildingUtility.ConnectionResult chosenConnection = BrickBuildingUtility.ConnectionResult.Empty();
            if (BrickBuildingUtility.FindBestConnection(_pickupOffset, _selectedBricks, ray, _sceneCamera.transform.localToWorldMatrix, _bricksInScene, bounds, out BrickBuildingUtility.ConnectionResult[] result))
            {
                foreach (var con in result)
                {
                    if(!CanConnect(con, _currentCollidingHit, out Vector3 newPivot))
                    {
                        continue;
                    }
                    if(chosenConnection.IsEmpty() || (!con.colliding && chosenConnection.colliding && con.maxSqrDistance < chosenConnection.maxSqrDistance))
                    {
                        // Don't allow building under the ground plane.
                        if(con.dstConnection.field.transform.position.y <= 0.05f)
                        {
                            continue;
                        }

                        var dotUp = Vector3.Dot(con.dstConnection.field.transform.up, Vector3.up);
                        if (dotUp >= 0.97f)
                        {
                            pivot = newPivot;
                            chosenConnection = con;
                            canConnect = !con.colliding;
                        }
                    }
                }
            }

            if (canConnect)
            {
                var connectionResult = chosenConnection;

                var localOffset = _focusBrick.transform.InverseTransformDirection(_pickupOffset);

                rigidbody.transform.RotateAround(pivot, connectionResult.rotationAxisToConnect, connectionResult.angleToConnect);
                rigidbody.transform.position += connectionResult.connectionOffset;

                Physics.SyncTransforms();

                _currentConnection = connectionResult;

                _pickupOffset = _focusBrick.transform.TransformDirection(localOffset);
            }
            else
            {
                _placeOffset = Vector3.zero;
                _currentConnection.Reset();

                if(BrickBuildingUtility.Colliding(_selectedBricks, bounds, _focusBrick.gameObject.scene.GetPhysicsScene(), out int hits))
                {
                    var moveBack = false;
                    var buffer = BrickBuildingUtility.ColliderBuffer;
                    for(var i = 0; i < hits; i++)
                    {
                        var hit = buffer[i];

                        var brick = hit.GetComponentInParent<Brick>();
                        if(brick)
                        {
                            if (_selectedBricks.Contains(brick))
                            {
                                continue;
                            }
                        }

                        var brickRigidbody = hit.GetComponentInParent<RigidbodyGroup>();
                        if(!brickRigidbody || brickRigidbody.IsKinematic)
                        {
                            moveBack = true;
                            break;
                        }
                    }

                    if(moveBack)
                    {
                        rigidbody.transform.position = oldPosition;
                        rigidbody.transform.rotation = oldRotation;
                    }
                }
            }
        }

        void FindBricksConnectedAbove(Brick brick, ICollection<Brick> connectedBricks)
        {
            var connected = brick.GetConnectedBricks(false);
            foreach(var connectedBrick in connected)
            {
                if(connectedBricks.Contains(connectedBrick))
                {
                    continue;
                }

                var localOther = brick.transform.InverseTransformPoint(connectedBrick.transform.position);

                if(localOther.y >= 0.0f)
                {
                    connectedBricks.Add(connectedBrick);
                    FindBricksConnectedAbove(connectedBrick, connectedBricks);
                }
            }
        }

        public static Vector3 GenerateNoise(float scale)
        {
            return UnityEngine.Random.insideUnitSphere * scale;
        }

        void MoveBricks(Ray ray)
        {
            Rotate();

            if (InputController.Instance.CurrentInputMoveDeltaMagnitude > 25.0f)
            {
                InputController.Instance.ResetDelta();
                _holdTimeElapsed = 0.0f;

                ComputeNewConnection(ray);

                Physics.SyncTransforms();

                FindPreconnect();
                Physics.SyncTransforms();
            }

            if(!_currentConnection.IsEmpty())
            {
                _holdTimeElapsed += Time.deltaTime;

                if(_holdTimeElapsed > HoldTime)
                {
                    _holdTimeElapsed = 0.0f;

                    // Get rid of place offset. Need to sync transforms afterwards.
                    var group = _groupPool.GetGroupFromBrick(_focusBrick);
                    group.transform.position -= _placeOffset;
                    _placeOffset = Vector3.zero;

                    Physics.SyncTransforms();

                    var bricks = group.GetComponentsInChildren<Brick>();
                    var bounds = BrickBuildingUtility.ComputeBounds(bricks);

                    Vector3 noise = GenerateNoise(1.5f / Mathf.Max(1.0f, bounds.size.sqrMagnitude));
                    var randomRot = Quaternion.Euler(noise);
                    group.transform.rotation *= randomRot;
                    _animatedGroup.ConnectBrick();

                    BrickBuildingUtility.Connect(_currentConnection.srcConnection, _currentConnection.dstConnection);

                    foreach (var brick in _selectedBricks)
                    {
                        foreach (var part in brick.parts)
                        {
                            foreach (var field in part.connectivity)
                            {
                                if (field == _currentConnection.srcConnection.field)
                                {
                                    continue;
                                }

                                var connections = field.QueryConnections(out bool reject);
                                if (reject)
                                {
                                    continue;
                                }

                                foreach (var connection in connections)
                                {
                                    BrickBuildingUtility.Connect(connection.Item1, connection.Item2, _selectedBricks);
                                    break;
                                }
                            }
                        }
                    }
                    _currentConnection.Reset();

                    var connectedBricks = _focusBrick.GetConnectedBricks();

                    var groupsToReturn = new HashSet<RigidbodyGroup>();
                    
                    foreach (var brick in connectedBricks)
                    {
                        var rb = _groupPool.GetGroupFromBrick(brick);
                        if(rb && rb != group)
                        {
                            groupsToReturn.Add(rb);
                        }
                        group.AddBrick(brick);
                    }

                    foreach(var rb in groupsToReturn)
                    {
                        _groupPool.ReturnGroup(rb);
                    }

                    _currentSelectionState = SelectionState.NoSelection;
                    InputController.Instance.ResetDelta();

                    if (_focusBrick)
                    {
                        if (_focusBrick.GetConnectedBricks().Count == 6)
                        {
                            _playDuckAnimation = true;
                        }
                    }

                    _focusBrick = null;
                    _selectedBricks.Clear();
                }
            }
        }

        void UpdateSelection(Ray ray)
        {
            switch (_currentSelectionState)
            {
                case SelectionState.NoSelection:
                {
                    if(_forceFieldActive)
                    {
                        foreach (var group in _groupPool)
                        {
                            if(group.transform.childCount == 0)
                            {
                                continue;
                            }

                            if(!group.Rigidbody.IsSleeping())
                            {
                                continue;
                            }

                            var direction = _orbitCamera.Target.transform.position - group.transform.position;
                            if (direction.magnitude > BuildingRadius)
                            {
                                // https://www.youtube.com/watch?v=IvT8hjy6q4o
                                var source = group.transform.position;
                                var target = _orbitCamera.Target.transform.position;
                                
                                var g = Physics.gravity.y;

                                float displacementY = target.y - source.y;
                                Vector3 displacementXZ = new Vector3(target.x - source.x, 0.0f, target.z - source.z);

                                float h = displacementXZ.magnitude * .3f;

                                Vector3 velocityY = Vector3.up * Mathf.Sqrt(-2 * g * h);
                                Vector3 velocityXZ = displacementXZ / (Mathf.Sqrt(-2 * h / g) + Mathf.Sqrt(2 * (displacementY - h)/g));

                                var velocity = velocityXZ + velocityY;

                                group.Rigidbody.velocity = velocity;
                                group.Rigidbody.AddTorque(velocity * 75.0f);
                            }
                        }
                    }

                    if (InputController.Instance.ActionDown(InputController.InputAction.HoldDown))
                    {
                        if (Physics.Raycast(ray, out RaycastHit hit, 100.0f, BrickBuildingUtility.IgnoreMask))
                        {
                            var brick = hit.collider.gameObject.GetComponentInParent<Brick>();

                            if(!brick)
                            {
                                var animatedBrick = hit.collider.gameObject.GetComponentInParent<AnimatedGroup.AnimatedBrick>();
                                if(animatedBrick)
                                {
                                    brick = animatedBrick.Brick;
                                }
                            }

                            if (brick)
                            {
                                _pickupOffset = hit.point - brick.transform.position;
                                _draggingBrick = brick;
                                _currentSelectionState = SelectionState.DraggingBrick;
                                InputController.Instance.ResetDelta();

                                _orbitCamera.Interactive = false;
                            }
                        }
                    }
                    else
                    {
                        _orbitCamera.Interactive = true;
                    }
                    break;
                }
                case SelectionState.DraggingBrick:
                {
                    if(InputController.Instance.ActionUp(InputController.InputAction.HoldDown))
                    {
                        _draggingBrick = null;
                        InputController.Instance.ResetDelta();
                        _focusBrick = null;
                        _currentSelectionState = SelectionState.NoSelection;
                        _orbitCamera.Interactive = true;
                    }

                    if (InputController.Instance.CurrentInputMoveDeltaMagnitude > 5.0f)
                    {
                        _focusBrick = _draggingBrick;
                        _draggingBrick = null;

                        var connectedCount = _focusBrick.GetConnectedBricks().Count;

                        _selectedBricks.Clear();
                        var connectedAbove = new HashSet<Brick>();
                        FindBricksConnectedAbove(_focusBrick, connectedAbove);
                        connectedAbove.Add(_focusBrick);
                        foreach(var brick in connectedAbove)
                        {
                            brick.DisconnectInverse(connectedAbove);
                            _selectedBricks.Add(brick);
                        }

                        var newCount = _focusBrick.GetConnectedBricks().Count;

                        if (newCount < connectedCount)
                        {
                            AudioController.Instance.PlayClick();
                        }

                        var group = _groupPool.GetGroupFromBrick(_focusBrick);
                        var brickChildren = group.GetComponentsInChildren<Brick>();
                        var unrelatedChildren = new List<Brick>();
                        foreach(var child in brickChildren)
                        {
                            if(_selectedBricks.Contains(child))
                            {
                                continue;
                            }
                            unrelatedChildren.Add(child);
                        }

                        if(unrelatedChildren.Count > 0)
                        {
                            _groupPool.UseGroup(unrelatedChildren[0], unrelatedChildren);
                        }

                        var localPickupOffset = _focusBrick.transform.InverseTransformDirection(_pickupOffset);

                        var localOffsets = new List<Vector3>();

                        foreach(var brick in _selectedBricks)
                        {
                            brick.transform.SetParent(null, true);

                            if(brick == _focusBrick)
                            {
                                continue;
                            }
                            var offset = _focusBrick.transform.position - brick.transform.position;
                            var local = _focusBrick.transform.InverseTransformVector(offset);
                            localOffsets.Add(local);
                        }

                        var focusEuler = _focusBrick.transform.rotation.eulerAngles;
                        _focusBrick.transform.rotation = Quaternion.Euler(0.0f, focusEuler.y, 0.0f);

                        group.transform.position = _focusBrick.transform.position;
                        group.transform.rotation = _focusBrick.transform.rotation;

                        group.AddBrick(_focusBrick);

                        var i = 0;
                        foreach (var brick in _selectedBricks)
                        {
                            if (brick == _focusBrick)
                            {
                                continue;
                            }

                            var euler = brick.transform.rotation.eulerAngles;
                            brick.transform.rotation = Quaternion.Euler(0.0f, euler.y, 0.0f);
                            brick.transform.position = _focusBrick.transform.position - _focusBrick.transform.TransformVector(localOffsets[i++]);

                            group.AddBrick(brick);
                        }
                        group.IsKinematic = true;

                        _pickupOffset = _focusBrick.transform.TransformDirection(localPickupOffset);

                        _currentSelectionState = SelectionState.MovingBrick;
                        InputController.Instance.ResetDelta();

                        _animatedGroup.StartAnimation(_focusBrick, _selectedBricks, AnimatedGroup.AnimationState.Initial);
                    }
                    break;
                }
                case SelectionState.MovingBrick:
                {
                    if (!InputController.Instance.Action(InputController.InputAction.HoldDown))
                    {
                        if(!_currentConnection.IsEmpty() && InputController.Instance.CurrentInputMoveDeltaMagnitude < 40.0f)
                        {
                            _holdTimeElapsed = HoldTime;
                            MoveBricks(ray);
                        }
                        else
                        {
                            PlaceBricks();
                        }
                    }
                    break;
                }
                case SelectionState.FinishedDuck:
                break;
            }
        }

        IEnumerator DuckAnimation()
        {
            _animatedGroup.Duck();
            var rb = _groupPool.GetGroupFromBrick(_bricksInScene[0]);
            var originalPos = rb.transform.position;
            var originalRotation = rb.transform.rotation;
            var originalScale = rb.transform.localScale;

            var targets = new Vector3[3];
            var rotations = new Quaternion[3];
            var scales = new Vector3[3];

            var multiplier = Vector2.one * 2.0f;

            for (var i = 0; i < 3; i++)
            {
                var target = (UnityEngine.Random.insideUnitCircle * 0.5f) * multiplier - Vector2.one;
                targets[i] = originalPos + new Vector3(target.x, 0.0f, target.y);

                Vector3 noise = GenerateNoise(30.0f);
                var randomRot = Quaternion.Euler(noise);
                rotations[i] = originalRotation * randomRot;

                var scaleValue = UnityEngine.Random.value * 0.5f;
                var scale = new Vector3(scaleValue, scaleValue, scaleValue);
                scales[i] = originalScale + scale;
            }

            var elapsed = 0.0f;

            var currentSourcePos = originalPos;
            var currentSourceRot = originalRotation;
            var currentSourceScale = originalScale;

            for (var i = 0; i < 3; i++)
            {
                while (elapsed <= CelebrationJumpDuration)
                {
                    var t = elapsed / CelebrationJumpDuration;
                    var eval = DuckAnimationCurve.Evaluate(t);

                    var newPos = Vector3.Lerp(currentSourcePos, targets[i], eval);
                    if (t >= 0.5f)
                    {
                        t = 1.0f - t;
                    }

                    eval = DuckAnimationCurve.Evaluate(t);

                    var currentHeight = CelebrationJumpHeight * eval;
                    newPos.y = currentSourcePos.y + currentHeight;
                    rb.transform.position = newPos;

                    rb.transform.rotation = Quaternion.Slerp(currentSourceRot, rotations[i], eval);
                    rb.transform.localScale = Vector3.Lerp(currentSourceScale, scales[i], eval);

                    elapsed += Time.deltaTime;
                    yield return null;
                }

                var pos = rb.transform.position;
                pos.y = originalPos.y;
                rb.transform.position = pos;
                currentSourcePos = pos;
                currentSourceRot = rb.transform.rotation;
                currentSourceScale = rb.transform.localScale;
                elapsed = 0.0f;
            }

            rb.transform.rotation = originalRotation;
            rb.transform.localScale = originalScale;
            _currentSelectionState = SelectionState.NoSelection;
            _orbitCamera.Interactive = true;

            _animatedGroup.EndAnimation();
        }

        void PlaceBricks()
        {
            if (_focusBrick)
            {
                _placeOffset = Vector3.zero;
                _holdTimeElapsed = 0.0f;

                var rb = _groupPool.GetGroupFromBrick(_focusBrick);
                rb.IsKinematic = false;
                
                if(InputController.Instance.CurrentInputMoveDeltaMagnitude > 10.0f)
                {
                    var delta = InputController.Instance.CurrentInputMoveDelta;

                    var mousePos = InputController.Instance.MousePosition;
                    var startPos = mousePos - delta;

                    var startRay = _sceneCamera.ScreenPointToRay(startPos);
                    var mouseRay = _sceneCamera.ScreenPointToRay(mousePos);

                    Physics.Raycast(startRay, out RaycastHit startHitInfo, 200.0f);
                    Physics.Raycast(mouseRay, out RaycastHit mouseHitInfo, 200.0f);

                    var direction = mouseHitInfo.point - startHitInfo.point;
                    direction.y = Mathf.Min(direction.magnitude * 5.0f, 20.0f);

                    rb.Rigidbody.AddTorque(direction * 105.0f, ForceMode.Impulse);
                    rb.Rigidbody.AddForce(direction * 3.0f, ForceMode.Impulse);
                }

                _animatedGroup.EndAnimation();
                _animatedGroup.CurrentAnimationState = AnimatedGroup.AnimationState.Inactive;

                _currentConnection.Reset();

                _currentSelectionState = SelectionState.NoSelection;
                _orbitCamera.Interactive = true;

                InputController.Instance.ResetDelta();

                _focusBrick = null;
                _selectedBricks.Clear();
            }
        }
       
        void CheckRotation()
        {
            if(!_focusBrick)
            {
                return;
            }

            var rigidbody = _groupPool.GetGroupFromBrick(_focusBrick);
            if(!rigidbody)
            {
                return;
            }

            var rotated = false;
            var axis = Vector3.zero;
            var angle = 0.0f;
            _localPickup = _focusBrick.transform.InverseTransformDirection(_pickupOffset);

            if (InputController.Instance.ActionDown(InputController.InputAction.RotateLeft))
            {
                rotated = true;
                axis = Vector3.up;
                angle = -90.0f;
            }
            else if(InputController.Instance.ActionDown(InputController.InputAction.RotateRight))
            {
                rotated = true;
                axis = Vector3.up;
                angle = 90.0f;
            }

            if(rotated)
            {
                var pivot = _focusBrick.transform.position + _pickupOffset;
                _queuedRotation = new Rotation { angle = angle, axis = axis, pivot = pivot };
            }
        }

        void Rotate()
        {
            if(_queuedRotation == null)
            {
                return;
            }

            var rigidbody = _groupPool.GetGroupFromBrick(_focusBrick);
            if (!rigidbody)
            {
                _queuedRotation = null;
                return;
            }

            rigidbody.transform.RotateAround(_queuedRotation.pivot, _queuedRotation.axis, _queuedRotation.angle);

            _pickupOffset = _focusBrick.transform.TransformDirection(_localPickup);

            _holdTimeElapsed = 0.0f;
            _queuedRotation = null;
        }
    }
}


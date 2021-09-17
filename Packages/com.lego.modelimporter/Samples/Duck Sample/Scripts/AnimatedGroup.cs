// Copyright (C) LEGO System A/S - All Rights Reserved
// Unauthorized copying of this file, via any medium is strictly prohibited

using System.Collections.Generic;
using UnityEngine;

namespace LEGOModelImporter.DuckSample
{
    public class AnimatedGroup : MonoBehaviour
    {
        public class AnimatedBrick : MonoBehaviour
        {
            public Brick Brick { get; set; }
        }

        public enum AnimationState
        {
            Inactive,
            Initial,
            Hover,
            Snap,
            Placing,
            Duck
        }

        Vector3 _currentTargetPosition;
        AnimatedBrick _currentFocusParent = null;
        List<AnimatedBrick> _animatingBricks = new List<AnimatedBrick>();
        public static readonly string KnobLayerName = "Knob";
        static readonly int KnobLayerMask = LayerMask.GetMask(KnobLayerName);

        public Dictionary<Brick, AnimatedBrick> BrickToAnimatedBrick { get; } = new Dictionary<Brick, AnimatedBrick>();

        #region Hover
        enum HoverState
        {
            HoverUp,
            HoverDown
        }

        float _currentLocalHoverHeight = 0.0f;
        
        Quaternion _currentLocalWobbleTarget = Quaternion.identity;
        Quaternion _currentLocalWobbleSource = Quaternion.identity;
        float _elapsedWobbleTime = 0.0f;
        float _totalWobbleTime = 1.0f / RuntimeBrickBuilder.Instance.WobbleVelocity;

        float _elapsedHoverTime = 0.0f;
        float _totalHoverTime = 0.0f;

        HoverState _currentHoverState = HoverState.HoverUp;

        #endregion

        AnimationState _currentAnimationState = AnimationState.Inactive;

        public AnimationState CurrentAnimationState
        {
            get { return _currentAnimationState; }
            set
            {
                _currentAnimationState = value;
                switch (value)
                {
                    case AnimationState.Hover:
                        SetupNewWobble();
                        _currentHoverState = HoverState.HoverUp;
                        break;
                    default:
                        break;
                }
                _currentLocalHoverHeight = 0.0f;
                _elapsedHoverTime = 0.0f;
            }
        }
       
        private void Update()
        {
            Animate();
        }

        private void SetupNewWobble()
        {
            _currentLocalWobbleSource = _currentLocalWobbleTarget;

            Vector3 noise = RuntimeBrickBuilder.GenerateNoise(RuntimeBrickBuilder.Instance.WobbleAmount);
            _currentLocalWobbleTarget = Quaternion.Euler(noise);
            _elapsedWobbleTime = 0.0f;
            _totalWobbleTime = 1.0f / RuntimeBrickBuilder.Instance.WobbleVelocity;
        }


        public void StartAnimation(Brick focusParent, ICollection<Brick> bricks, AnimationState state = AnimationState.Hover)
        {
            EndAnimation();
            _currentFocusParent = BrickToAnimatedBrick[focusParent];
            CurrentAnimationState = state;

            foreach (var brick in bricks)
            {
                var animatingBrick = BrickToAnimatedBrick[brick];
                _animatingBricks.Add(animatingBrick);
                if (focusParent == brick)
                {
                    continue;
                }

                animatingBrick.transform.SetParent(_currentFocusParent.transform, true);
            }
        }

        public void EndAnimation()
        {
            _currentFocusParent = null;
            foreach (var animatingBrick in _animatingBricks)
            {
                animatingBrick.transform.SetParent(null, true);
            }
            _animatingBricks.Clear();

            foreach(var brick in BrickToAnimatedBrick.Values)
            {
                brick.transform.SetParent(null, true);
            }

            CurrentAnimationState = AnimationState.Inactive;
        }

        public void ConnectBrick()
        {
            CurrentAnimationState = AnimationState.Snap;
        }

        public void PlaceBrick()
        {
            CurrentAnimationState = AnimationState.Placing;
        }

        public void Pickup()
        {
            CurrentAnimationState = AnimationState.Initial;
        }

        public void Duck()
        {
            foreach(var brick in BrickToAnimatedBrick.Values)
            {
                if(brick == _currentFocusParent)
                {
                    continue;
                }

                brick.transform.SetParent(_currentFocusParent.transform, true);
            }
            CurrentAnimationState = AnimationState.Duck;
        }

        public void CreateAnimatedBrick(Brick brick)
        {
            if(BrickToAnimatedBrick.ContainsKey(brick))
            {
                return;
            }

            // We create impostor bricks that are used for rendering and animation of the actual bricks.
            // We re-create all mesh renderers (shell, inside detail, knobs, tubes) and destroy the original ones.
            var impostor = new GameObject("ImpostorBrick " + brick.designID);
            var impostorComponent = impostor.AddComponent<AnimatedBrick>();
            impostorComponent.Brick = brick;
            BrickToAnimatedBrick.Add(brick, impostorComponent);

            var meshRenderers = brick.GetComponentsInChildren<MeshRenderer>();

            foreach (var part in brick.parts)
            {
                part.knobs.Clear();
                part.tubes.Clear();
                part.insideDetails.Clear();

                foreach(var field in part.connectivity)
                {
                    if(field is PlanarField pf)
                    {
                        foreach(var connection in pf.connections)
                        {
                            connection.tubes.Clear();
                            connection.knob = null;
                        }
                        pf.insideDetail = null;
                    }
                }
            }

            foreach (var renderer in meshRenderers)
            {
                var impostorChild = new GameObject("Impostor Renderer " + renderer.gameObject.name);
                var impostorRenderer = impostorChild.AddComponent<MeshRenderer>();
                var impostorFilter = impostorChild.AddComponent<MeshFilter>();

                impostorRenderer.material = renderer.material;
                impostorFilter.mesh = renderer.gameObject.GetComponent<MeshFilter>().mesh;

                var insideDetail = renderer.GetComponent<InsideDetail>();
                if(insideDetail)
                {
                    Destroy(insideDetail.gameObject);
                }

                var knob = renderer.GetComponent<Knob>();
                if (knob)
                {
                    var knobCollider = new GameObject("Knob Collider");
                    knobCollider.transform.position = knob.transform.position;
                    knobCollider.transform.rotation = knob.transform.rotation;
                    knobCollider.transform.localScale = knob.transform.localScale;
                    var boxCollider = knobCollider.AddComponent<BoxCollider>();
                    boxCollider.size = new Vector3(BrickBuildingUtility.LU_5, BrickBuildingUtility.LU_1 * 2, BrickBuildingUtility.LU_5);
                    boxCollider.center = new Vector3(0.0f, BrickBuildingUtility.LU_1, 0.0f);
                    knobCollider.layer = LayerMask.NameToLayer(KnobLayerName);

                    var part = renderer.GetComponentInParent<Part>();
                    knobCollider.transform.parent = part.transform;

                    Destroy(knob.gameObject);
                }

                var tube = renderer.GetComponent<Tube>();
                if (tube)
                {
                    Destroy(tube.gameObject);
                }

                impostorChild.transform.parent = impostor.transform;
                impostorChild.transform.localPosition = renderer.transform.localPosition;
                impostorChild.transform.localRotation = renderer.transform.localRotation;
            }

            impostor.transform.position = brick.transform.position;
            impostor.transform.rotation = brick.transform.rotation;
            impostor.transform.localScale = brick.transform.parent.localScale;

            foreach (var renderer in meshRenderers)
            {
                Destroy(renderer.gameObject);
            }
        }

        public static AnimatedGroup CreateAnimatedGroup()
        {
            Part.SetAdditionalIgnoreMask(KnobLayerMask);

            GameObject group = new GameObject("ImpostorGroup");
            var animatedImpostor = group.AddComponent<AnimatedGroup>();
            return animatedImpostor;
        }

        void MoveBricks(float multiplier = 1.0f)
        {
            if(_currentFocusParent)
            {
                if(_currentAnimationState != AnimationState.Duck)
                {
                    var brick = _currentFocusParent.Brick;
                    _currentTargetPosition = brick.transform.position;

                    var angleBetween = Quaternion.Angle(_currentFocusParent.transform.rotation, brick.transform.rotation);
                    var ratio = RuntimeBrickBuilder.Instance.RotationVelocity / angleBetween;
                    var pivot = RuntimeBrickBuilder.Instance.PickupOffset + _currentFocusParent.transform.position;

                    _currentFocusParent.transform.position -= pivot;

                    _currentFocusParent.transform.rotation = Quaternion.Slerp(_currentFocusParent.transform.rotation, brick.transform.rotation, ratio);

                    pivot = RuntimeBrickBuilder.Instance.PickupOffset + _currentFocusParent.transform.position + pivot;

                    _currentFocusParent.transform.position += pivot;

                    var direction = _currentTargetPosition - _currentFocusParent.transform.position;
                    var increment = RuntimeBrickBuilder.Instance.MoveVelocity * multiplier * Time.deltaTime;

                    var distanceSquared = MathUtils.DistanceSquared(_currentFocusParent.transform.position, _currentTargetPosition);
                    increment = Mathf.Min(increment, Mathf.Sqrt(distanceSquared));

                    _currentFocusParent.transform.position += increment * direction;

                    var newDistance = MathUtils.DistanceSquared(_currentFocusParent.transform.position, _currentTargetPosition);

                    if (newDistance >= distanceSquared)
                    {
                        _currentFocusParent.transform.position = _currentTargetPosition;
                    }

                    _currentFocusParent.transform.localScale = brick.transform.parent.localScale;

                    foreach (var impostor in BrickToAnimatedBrick.Values)
                    {
                        if (!_animatingBricks.Contains(impostor))
                        {
                            impostor.transform.position = impostor.Brick.transform.position;
                            impostor.transform.rotation = impostor.Brick.transform.rotation;
                            impostor.transform.localScale = impostor.Brick.transform.parent.localScale;
                        }
                    }
                }
                else
                {
                    var brick = _currentFocusParent.Brick;
                    _currentFocusParent.transform.rotation = brick.transform.parent.rotation;
                    _currentFocusParent.transform.position = brick.transform.parent.position;
                    _currentFocusParent.transform.localScale = brick.transform.parent.localScale;
                }
            }
            else
            {
                foreach(var impostor in BrickToAnimatedBrick.Values)
                {
                    impostor.transform.position = impostor.Brick.transform.position;
                    impostor.transform.rotation = impostor.Brick.transform.rotation;
                    impostor.transform.localScale = impostor.Brick.transform.parent.localScale;
                }
            }
        }

        void Animate()
        {
            _totalHoverTime = (RuntimeBrickBuilder.Instance.MaxHoverHeight * 2.0f) / RuntimeBrickBuilder.Instance.HoverVelocity;

            switch (CurrentAnimationState)
            {
                case AnimationState.Inactive:
                    MoveBricks(2.0f);
                    return;
                case AnimationState.Initial:
                    CurrentAnimationState = AnimationState.Hover;
                    return;
                case AnimationState.Hover:
                    MoveBricks();
                    _elapsedHoverTime += Time.deltaTime;
                    _elapsedWobbleTime += Time.deltaTime;

                    switch (_currentHoverState)
                    {
                        case HoverState.HoverUp:
                        {
                            var t = _elapsedHoverTime / _totalHoverTime;
                            var evaluated = RuntimeBrickBuilder.Instance.HoverAnimationCurve.Evaluate(t);

                            _currentLocalHoverHeight = RuntimeBrickBuilder.Instance.MaxHoverHeight * evaluated;
                            if (_currentLocalHoverHeight >= RuntimeBrickBuilder.Instance.MaxHoverHeight)
                            {
                                _currentHoverState = HoverState.HoverDown;
                                _elapsedHoverTime = 0.0f;
                            }
                            break;
                        }
                        case HoverState.HoverDown:
                        {
                            var t = 1 - (_elapsedHoverTime / _totalHoverTime);
                            var evaluated = RuntimeBrickBuilder.Instance.HoverAnimationCurve.Evaluate(t);

                            _currentLocalHoverHeight = RuntimeBrickBuilder.Instance.MaxHoverHeight * evaluated;
                            if (_currentLocalHoverHeight <= 0.0f)
                            {
                                _currentHoverState = HoverState.HoverUp;
                                _elapsedHoverTime = 0.0f;
                            }
                            break;
                        }
                    }
                    ApplyHover();
                    break;
                case AnimationState.Snap:
                    MoveBricks(0.8f);

                    if (Vector3.Distance(_currentFocusParent.transform.position, _currentFocusParent.Brick.transform.position) < RuntimeBrickBuilder.Instance.ClickDistance)
                    {
                        var brick = _currentFocusParent.Brick;

                        _currentFocusParent.transform.position = brick.transform.position;
                        _currentFocusParent.transform.rotation = brick.transform.rotation;
                        _currentFocusParent.transform.localScale = brick.transform.parent.localScale;
                        CurrentAnimationState = AnimationState.Inactive;
                        AudioController.Instance.PlayClick();
                    }
                    break;
                case AnimationState.Placing:
                    MoveBricks();

                    if (Vector3.Distance(_currentFocusParent.transform.position, _currentFocusParent.Brick.transform.position) < 0.01f)
                    {
                        var brick = _currentFocusParent.Brick;
                        _currentFocusParent.transform.position = brick.transform.position;
                        _currentFocusParent.transform.rotation = brick.transform.rotation;
                        _currentFocusParent.transform.localScale = brick.transform.parent.localScale;
                        EndAnimation();
                    }
                    break;
                case AnimationState.Duck:
                    MoveBricks(2.0f);
                    break;
            }
        }

        void ApplyHover()
        {
            if(_currentFocusParent)
            {
                if (_elapsedWobbleTime > _totalWobbleTime)
                {
                    SetupNewWobble();
                }

                var originalPosition = _currentFocusParent.transform.position;
                var originalRotation = _currentFocusParent.transform.rotation;

                var t = _elapsedWobbleTime / _totalWobbleTime;
                var eval = RuntimeBrickBuilder.Instance.WobbleAnimationCurve.Evaluate(t);
                var newRot = Quaternion.Slerp(_currentLocalWobbleSource, _currentLocalWobbleTarget, eval);
                _currentFocusParent.transform.localRotation *= newRot;

                _currentFocusParent.transform.localPosition += new Vector3(0.0f, _currentLocalHoverHeight, 0.0f);

                foreach(var animated in _animatingBricks)
                {
                    var brick = animated.Brick;
                    foreach(var part in brick.parts)
                    {
                        if(Part.IsColliding(part, animated.transform.localToWorldMatrix, BrickBuildingUtility.ColliderBuffer, out _))
                        {
                            _currentFocusParent.transform.position = originalPosition;
                            _currentFocusParent.transform.rotation = originalRotation;
                            break;
                        }
                    }
                }
            }
        }
    }
}
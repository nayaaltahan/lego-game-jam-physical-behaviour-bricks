// Copyright (C) LEGO System A/S - All Rights Reserved
// Unauthorized copying of this file, via any medium is strictly prohibited

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LEGOModelImporter.DuckSample
{
    public class RigidbodyGroupPool : IEnumerable<RigidbodyGroup>
    {
        RigidbodyGroup[] _rigidbodyGroups;
        int[] _unusedIndices;

        public RigidbodyGroupPool(int capacity = 1)
        {
            _rigidbodyGroups = new RigidbodyGroup[capacity];
            _unusedIndices = new int[capacity];

            for (var i = 0; i < capacity; i++)
            {
                var go = new GameObject("Rigidbody");
                _rigidbodyGroups[i] = go.AddComponent<RigidbodyGroup>();
                _rigidbodyGroups[i].IsKinematic = true;
                _rigidbodyGroups[i].Rigidbody.collisionDetectionMode = CollisionDetectionMode.Continuous;
                _rigidbodyGroups[i].Rigidbody.ResetCenterOfMass();
                _unusedIndices[i] = i;

                go.SetActive(false);
            }
        }

        public RigidbodyGroup GetGroupFromBrick(Brick brick)
        {
            var parent = brick.transform.parent;
            if (!parent)
            {
                return null;
            }

            var rb = brick.GetComponentInParent<RigidbodyGroup>();
            return rb;
        }

        public RigidbodyGroup UseGroup(Brick target, ICollection<Brick> relativeTargets = null)
        {
            for (var i = 0; i < _rigidbodyGroups.Length; i++)
            {
                if (_unusedIndices[i] == i)
                {
                    var group = _rigidbodyGroups[i];
                    _unusedIndices[i] = -1;
                    group.gameObject.SetActive(true);
                    group.IsKinematic = true;

                    group.Clear();

                    var position = target.transform.position;
                    var rotation = target.transform.rotation;

                    group.transform.position = position;
                    group.transform.rotation = rotation;
                    group.AddBrick(target);

                    if (relativeTargets != null)
                    {
                        foreach (var relative in relativeTargets)
                        {
                            if (relative == target)
                            {
                                continue;
                            }

                            group.AddBrick(relative);
                        }
                    }

                    return group;
                }
            }
            return null;
        }

        public void ReturnGroup(RigidbodyGroup rigidbodyGroup)
        {
            if(!rigidbodyGroup)
            {
                return;
            }

            for (var i = 0; i < _rigidbodyGroups.Length; i++)
            {
                if (_rigidbodyGroups[i] == rigidbodyGroup && _unusedIndices[i] == -1)
                {
                    ReturnGroup(i);
                    break;
                }
            }
        }

        void ReturnGroup(int index)
        {
            var group = _rigidbodyGroups[index];

            group.IsKinematic = true;
            group.Clear();
            group.gameObject.SetActive(false);

            _unusedIndices[index] = index;
        }

        public void Update()
        {
            Physics.gravity = new Vector3(0.0f, RuntimeBrickBuilder.Instance.Gravity, 0.0f);
            for (var i = 0; i < _rigidbodyGroups.Length; i++)
            {
                RigidbodyGroup group = _rigidbodyGroups[i];

                group.Rigidbody.mass = RuntimeBrickBuilder.Instance.Mass;
                group.Rigidbody.angularDrag = RuntimeBrickBuilder.Instance.AngularDrag;
                group.Rigidbody.drag = RuntimeBrickBuilder.Instance.Drag;

                if (_unusedIndices[i] == -1 && group.transform.childCount == 0)
                {
                    ReturnGroup(i);
                }
            }
        }

        public IEnumerator<RigidbodyGroup> GetEnumerator()
        {
            foreach (var rigidbody in _rigidbodyGroups)
            {
                yield return rigidbody;
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
// Copyright (C) LEGO System A/S - All Rights Reserved
// Unauthorized copying of this file, via any medium is strictly prohibited

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LEGOModelImporter.DuckSample
{
    [RequireComponent(typeof(Rigidbody))]
    public class RigidbodyGroup : MonoBehaviour
    {
        public Rigidbody Rigidbody;
        public bool IsKinematic
        {
            get { return Rigidbody.isKinematic; }
            set
            {
                Rigidbody.isKinematic = value;
            }
        }
        bool _canPlaySound = true;

        void Awake()
        {
            Rigidbody = GetComponent<Rigidbody>();
        }

        public void AddBrick(Brick brick)
        {
            brick.transform.SetParent(transform, true);
        }

        public void RemoveBrick(Brick brick)
        {
            brick.transform.SetParent(null, true);
        }

        public void Clear()
        {
            if (transform.childCount > 0)
            {
                var children = new List<Transform>();
                foreach(Transform child in transform)
                {
                    children.Add(child);
                }
                foreach (Transform child in children)
                {
                    child.SetParent(null, true);
                }
            }
        }

        public IEnumerator SoundDelay(float delay)
        {
            _canPlaySound = false;
            yield return new WaitForSeconds(delay);
            _canPlaySound = true;
        }

        private void OnCollisionEnter(Collision collision)
        {
            if(_canPlaySound)
            {
                AudioController.Instance.PlayFall();
                var br = collision.gameObject.GetComponentInParent<RigidbodyGroup>();
                if(br)
                {
                    StartCoroutine(br.SoundDelay(.5f));
                }
            }
        }
    }
}


// Copyright (C) LEGO System A/S - All Rights Reserved
// Unauthorized copying of this file, via any medium is strictly prohibited

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LEGOMinifig.MinifigExample
{

    public class NPCController : MonoBehaviour
    {
        public MinifigController nPCMoveMinifigController;
        public MinifigController nPCFollowMinifigController;
        public List<Transform> waypoints;
        public Transform hotdogTarget;

        int currentWaypoint;

        // Start is called before the first frame update
        void Start()
        {
            SetNextWaypoint();

            FollowHotdog();
        }

        void SetNextWaypoint()
        {
            nPCMoveMinifigController.MoveTo(waypoints[currentWaypoint].position, 0.0f, SetNextWaypoint, 1.0f, Random.Range(0.0f, 1.0f), false, Random.Range(1.0f, 3.0f), 1.0f, Vector3.zero);
            currentWaypoint = (currentWaypoint + 1) % waypoints.Count;
        }

        void FollowHotdog()
        {
            nPCFollowMinifigController.ReachHandsTo(nPCFollowMinifigController.transform.TransformPoint(Vector3.forward * 3.0f + Vector3.up * 2.5f), null);
            nPCFollowMinifigController.Follow(hotdogTarget, 0.5f, BeHappy, 0.0f, 0.0f, false, Random.Range(0.5f, 1.0f));
        }

        void BeHappy()
        {
            nPCFollowMinifigController.StopFollowing();
            StartCoroutine(DoBeHappy());
        }

        IEnumerator DoBeHappy()
        {
            nPCFollowMinifigController.StopReachingHands();
            nPCFollowMinifigController.TurnTo(Vector3.zero);
            nPCFollowMinifigController.GetComponent<MinifigFaceAnimationController>().PlayAnimation(MinifigFaceAnimationController.FaceAnimation.Laugh);

            yield return new WaitForSeconds(Random.Range(2.0f, 4.0f));

            FollowHotdog();
        }
    }

}

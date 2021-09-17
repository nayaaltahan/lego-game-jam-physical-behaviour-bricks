// Copyright (C) LEGO System A/S - All Rights Reserved
// Unauthorized copying of this file, via any medium is strictly prohibited

using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using System;

namespace LEGOMinifig.MinifigExample
{

    public class AnimationUI : MonoBehaviour
    {
        public MinifigController minifigPrefab;
        public MinifigController minifigController;
        public Minifig minifig;
        public MinifigFaceAnimationController minifigFaceAnimationController;
        public VirtualJoystick virtualJoystick;
        public GameObject leftHandAccessoryPrefab;
        public Vector3 leftHandAccessoryGrabPosition;
        public Quaternion leftHandAccessoryGrabOrientation;
        public GameObject rightHandAccessoryPrefab;
        public Vector3 rightHandAccessoryGrabPosition;
        public Quaternion rightHandAccessoryGrabOrientation;
        public GameObject templateButton;

        FollowPlayer followPlayer;

        Button specialAnimationButton;
        GameObject specialAnimationScrollView;
        Transform specialAnimationContent;
        Button faceAnimationButton;
        GameObject faceAnimationScrollView;
        Transform faceAnimationContent;
        Button reachButton;
        Button liftFootButton;
        Button lookButton;
        Button explodeButton;
        Button equipLeftHandButton;
        Button equipRightHandButton;

        void Start()
        {
            specialAnimationButton = GameObject.Find("Animation UI/Special Animation Button").GetComponent<Button>();
            specialAnimationScrollView = GameObject.Find("Animation UI/Special Animation Scroll View");
            specialAnimationContent = GameObject.Find("Animation UI/Special Animation Scroll View/Viewport/Content").transform;

            faceAnimationButton = GameObject.Find("Animation UI/Face Animation Button").GetComponent<Button>();
            faceAnimationScrollView = GameObject.Find("Animation UI/Face Animation Scroll View");
            faceAnimationContent = GameObject.Find("Animation UI/Face Animation Scroll View/Viewport/Content").transform;

            reachButton = GameObject.Find("Animation UI/Reach Button").GetComponent<Button>();
            liftFootButton = GameObject.Find("Animation UI/Lift Foot Button").GetComponent<Button>();
            lookButton = GameObject.Find("Animation UI/Look Button").GetComponent<Button>();
            explodeButton = GameObject.Find("Animation UI/Explode Button").GetComponent<Button>();

            equipLeftHandButton = GameObject.Find("Animation UI/Equip Left Hand Button").GetComponent<Button>();
            equipRightHandButton = GameObject.Find("Animation UI/Equip Right Hand Button").GetComponent<Button>();

            specialAnimationScrollView.SetActive(false);
            faceAnimationScrollView.SetActive(false);

            // Set up special animation button
            specialAnimationButton.onClick.AddListener(delegate
            {
                specialAnimationButton.interactable = false;
                faceAnimationButton.interactable = false;

                reachButton.gameObject.SetActive(false);
                liftFootButton.gameObject.SetActive(false);
                lookButton.gameObject.SetActive(false);
                explodeButton.gameObject.SetActive(false);
                equipLeftHandButton.gameObject.SetActive(false);
                equipRightHandButton.gameObject.SetActive(false);

                specialAnimationScrollView.SetActive(true);
            });

            // Set up face animation button
            faceAnimationButton.onClick.AddListener(delegate
            {
                specialAnimationButton.interactable = false;
                faceAnimationButton.interactable = false;

                reachButton.gameObject.SetActive(false);
                liftFootButton.gameObject.SetActive(false);
                lookButton.gameObject.SetActive(false);
                explodeButton.gameObject.SetActive(false);
                equipLeftHandButton.gameObject.SetActive(false);
                equipRightHandButton.gameObject.SetActive(false);

                faceAnimationScrollView.SetActive(true);
            });

            // Set up rig buttons
            reachButton.onClick.AddListener(delegate
            {
                StopCoroutine("DoReachForward");
                StartCoroutine("DoReachForward");
            });
            liftFootButton.onClick.AddListener(delegate
            {
                StopCoroutine("DoLiftFoot");
                StartCoroutine("DoLiftFoot");
            });
            lookButton.onClick.AddListener(delegate
            {
                StartCoroutine("DoLook");

                lookButton.interactable = false;
            });
            explodeButton.onClick.AddListener(delegate
            {
                StartCoroutine("DoExplode");

                explodeButton.interactable = false;
            });

            // Set up equip buttons
            equipLeftHandButton.onClick.AddListener(delegate
            {
                var existingAccessory = minifig.GetLeftHandAccessory();
                if (existingAccessory)
                {
                    Destroy(existingAccessory.gameObject);
                }
                else
                {
                    var leftHandAccessory = Instantiate(leftHandAccessoryPrefab);
                    minifig.SetLeftHandAccessory(leftHandAccessory, leftHandAccessoryGrabPosition, leftHandAccessoryGrabOrientation);
                }
            });
            equipRightHandButton.onClick.AddListener(delegate
            {
                var existingAccessory = minifig.GetRightHandAccessory();
                if (existingAccessory)
                {
                    Destroy(existingAccessory.gameObject);
                }
                else
                {
                    var rightHandAccessory = Instantiate(rightHandAccessoryPrefab);
                    minifig.SetRightHandAccessory(rightHandAccessory, rightHandAccessoryGrabPosition, rightHandAccessoryGrabOrientation);
                }
	    	});

            // Create buttons for special animations
            foreach (var name in Enum.GetNames(typeof(MinifigController.SpecialAnimation)))
            {
                var animation = (MinifigController.SpecialAnimation)Enum.Parse(typeof(MinifigController.SpecialAnimation), name);

                var button = Instantiate(templateButton, specialAnimationContent);

                button.GetComponentInChildren<Text>().text = name;
                button.SetActive(true);

                button.GetComponent<Button>().onClick.AddListener(delegate
                {
                    specialAnimationButton.interactable = true;
                    faceAnimationButton.interactable = true;

                    reachButton.gameObject.SetActive(true);
                    liftFootButton.gameObject.SetActive(true);
                    lookButton.gameObject.SetActive(true);
                    explodeButton.gameObject.SetActive(true);
                    equipLeftHandButton.gameObject.SetActive(true);
                    equipRightHandButton.gameObject.SetActive(true);

                    specialAnimationScrollView.SetActive(false);
                    minifigController.PlaySpecialAnimation(animation);
                });
            }

            // Create buttons for face animations
            foreach (var name in Enum.GetNames(typeof(MinifigFaceAnimationController.FaceAnimation)))
            {
                var animation = (MinifigFaceAnimationController.FaceAnimation)Enum.Parse(typeof(MinifigFaceAnimationController.FaceAnimation), name);

                var button = Instantiate(templateButton, faceAnimationContent);

                button.GetComponentInChildren<Text>().text = name;
                button.SetActive(true);
                button.GetComponent<Button>().interactable = minifigFaceAnimationController.HasAnimation(animation);

                button.GetComponent<Button>().onClick.AddListener(delegate
                {
                    specialAnimationButton.interactable = true;
                    faceAnimationButton.interactable = true;

                    reachButton.gameObject.SetActive(true);
                    liftFootButton.gameObject.SetActive(true);
                    lookButton.gameObject.SetActive(true);
                    explodeButton.gameObject.SetActive(true);
                    equipLeftHandButton.gameObject.SetActive(true);
                    equipRightHandButton.gameObject.SetActive(true);

                    faceAnimationScrollView.SetActive(false);
                    minifigFaceAnimationController.PlayAnimation(animation);
                });
            }

            // Add reference to follow player script.
            followPlayer = Camera.main.GetComponent<FollowPlayer>();
        }

        IEnumerator DoReachForward()
        {
            minifigController.ReachHandsTo(minifigController.transform.TransformPoint(Vector3.forward * 3.0f + Vector3.up * 2.5f), null, 0.25f);

            yield return new WaitForSeconds(2.0f);

            minifigController.StopReachingHands(0.25f);
        }

        IEnumerator DoLiftFoot()
        {
            minifigController.ReachLeftFootTo(minifigController.transform.TransformPoint(Vector3.forward * 2.0f + Vector3.up * 1.0f + Vector3.left * 0.5f), null, 0.25f);

            yield return new WaitForSeconds(2.0f);

            minifigController.StopReachingLeftFoot(0.25f);
        }

        IEnumerator DoLook()
        {
            minifigController.LookAt(minifigController.transform.TransformPoint(Vector3.forward * 3.0f + Vector3.left * UnityEngine.Random.Range(-10.0f, 10.0f)), 0.25f);

            yield return new WaitForSeconds(2.0f);

            minifigController.StopLooking(0.25f);

            yield return new WaitForSeconds(0.25f);

            lookButton.interactable = true;
        }

        IEnumerator DoExplode()
        {
            minifigController.Explode();

            yield return new WaitForSeconds(5.0f);

            Destroy(minifigController.gameObject);

            minifigController = Instantiate(minifigPrefab, Vector3.zero, Quaternion.Euler(0.0f, 180.0f, 0.0f));
            minifig = minifigController.GetComponent<Minifig>();
            minifigFaceAnimationController = minifigController.GetComponent<MinifigFaceAnimationController>();

            minifigController.SetVirtualJoystick(virtualJoystick);

            followPlayer?.TargetCameraOnPlayer(minifigController.gameObject);

            explodeButton.interactable = true;
        }
    }

}
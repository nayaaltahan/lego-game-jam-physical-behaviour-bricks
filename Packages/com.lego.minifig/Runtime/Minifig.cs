using System;
using System.Collections.Generic;
using UnityEngine;

namespace LEGOMinifig
{
    public class Minifig : MonoBehaviour
    {
#pragma warning disable 649
        [HideInInspector, SerializeField]
        Transform headAccessoryLocator;

        [HideInInspector, SerializeField]
        Transform neckAccessoryLocator;

        [HideInInspector, SerializeField]
        Transform leftHandAccessoryLocator;

        [HideInInspector, SerializeField]
        Transform rightHandAccessoryLocator;

        [HideInInspector, SerializeField]
        Transform waistAccessoryLocator;

        [HideInInspector, SerializeField]
        Transform capeAccessoryLocator;

        [HideInInspector, SerializeField]
        Transform torsoLocator;

        [HideInInspector, SerializeField] 
        Transform hipLocator;

        [HideInInspector, SerializeField]
        Transform torsoBackDecorationGeometry;

        [HideInInspector, SerializeField]
        Transform torsoFrontDecorationGeometry;

        [HideInInspector, SerializeField]
        Transform torsoGeometry;

        [HideInInspector, SerializeField]
        List<Transform> armsGeometry;

        [HideInInspector, SerializeField]
        List<Transform> handsGeometry;

        [HideInInspector, SerializeField]
        List<Transform> legsGeometry;

        [HideInInspector, SerializeField]
        Transform headGeometry;

        [HideInInspector, SerializeField]
        Transform faceGeometry;
#pragma warning restore 649

        public enum AccessoryTransformSpace
        {
            Acccessory,
            BodyPart,
            OriginalPose
        }

        Vector3 spine1OriginalPosition, spine5OriginalPosition, neckOriginalPosition, hipOriginalPosition, waistOriginalPosition;
        Quaternion spine1OriginalRotation, spine5OriginalRotation, neckOriginalRotation, hipOriginalRotation, waistOriginalRotation;

        void Awake()
        {
            spine1OriginalPosition = transform.InverseTransformPoint(torsoLocator.parent.position);
            spine5OriginalPosition = transform.InverseTransformPoint(neckAccessoryLocator.parent.parent.parent.position);
            neckOriginalPosition = transform.InverseTransformPoint(neckAccessoryLocator.position);
            hipOriginalPosition = transform.InverseTransformPoint(hipLocator.parent.position);
            waistOriginalPosition = transform.InverseTransformPoint(waistAccessoryLocator.position);

            spine1OriginalRotation = Quaternion.Inverse(transform.rotation) * torsoLocator.parent.rotation;
            spine5OriginalRotation = Quaternion.Inverse(transform.rotation) * neckAccessoryLocator.parent.parent.parent.rotation;
            neckOriginalRotation = Quaternion.Inverse(transform.rotation) * neckAccessoryLocator.rotation;
            hipOriginalRotation = Quaternion.Inverse(transform.rotation) * hipLocator.parent.rotation;
            waistOriginalRotation = Quaternion.Inverse(transform.rotation) * waistAccessoryLocator.rotation;
        }

        public void SetHeadAccessory(GameObject accessory)
        {
            accessory.transform.SetParent(headAccessoryLocator, false);
        }

        public void SetCapeAccessory(GameObject accessory, Vector3 accessoryLocalPosition, Quaternion accessoryLocalOrientation = new Quaternion(), AccessoryTransformSpace accessoryTransformSpace = AccessoryTransformSpace.Acccessory)
        {
            SetNeckAccessory(capeAccessoryLocator, accessory, accessoryLocalPosition, accessoryLocalOrientation, accessoryTransformSpace);
        }

        public void SetNeckAccessory(GameObject accessory, Vector3 accessoryLocalPosition, Quaternion accessoryLocalOrientation = new Quaternion(), AccessoryTransformSpace accessoryTransformSpace = AccessoryTransformSpace.Acccessory)
        {
            SetNeckAccessory(neckAccessoryLocator, accessory, accessoryLocalPosition, accessoryLocalOrientation, accessoryTransformSpace);
        }

        void SetNeckAccessory(Transform parent, GameObject accessory, Vector3 accessoryLocalPosition, Quaternion accessoryLocalOrientation, AccessoryTransformSpace accessoryTransformSpace)
        {
            switch (accessoryTransformSpace)
            {
                case AccessoryTransformSpace.Acccessory:
                    {
                        accessory.transform.localRotation = accessoryLocalOrientation;
                        accessory.transform.SetParent(parent, false);
                        // FIXME Handle this proper.
                        accessory.transform.localRotation = accessory.transform.localRotation;
                        accessory.transform.localPosition = accessory.transform.localPosition;
                        break;
                    }
                case AccessoryTransformSpace.BodyPart:
                    {
                        accessory.transform.SetParent(parent);
                        accessory.transform.localRotation = accessoryLocalOrientation;
                        accessory.transform.localPosition = accessoryLocalPosition;
                        break;
                    }
                case AccessoryTransformSpace.OriginalPose:
                    {
                        accessory.transform.localPosition = accessoryLocalPosition;
                        accessory.transform.localRotation = accessoryLocalOrientation;

                        var spine1JointRestorePosition = torsoLocator.parent.position;
                        var spine1JointRestoreRotation = torsoLocator.parent.rotation;
                        var spine5JointRestorePosition = neckAccessoryLocator.parent.parent.parent.position;
                        var spine5JointRestoreRotation = neckAccessoryLocator.parent.parent.parent.rotation;
                        var neckJointRestorePosition = neckAccessoryLocator.position;
                        var neckJointRestoreRotation = neckAccessoryLocator.rotation;

                        torsoLocator.parent.position = transform.TransformPoint(spine1OriginalPosition);
                        torsoLocator.parent.rotation = transform.rotation * spine1OriginalRotation;
                        neckAccessoryLocator.parent.parent.parent.position = transform.TransformPoint(spine5OriginalPosition);
                        neckAccessoryLocator.parent.parent.parent.rotation = transform.rotation * spine5OriginalRotation;
                        neckAccessoryLocator.position = transform.TransformPoint(neckOriginalPosition);
                        neckAccessoryLocator.rotation = transform.rotation * neckOriginalRotation;

                        accessory.transform.SetParent(parent, true);

                        torsoLocator.parent.position = spine1JointRestorePosition;
                        torsoLocator.parent.rotation = spine1JointRestoreRotation;
                        neckAccessoryLocator.parent.parent.parent.position = spine5JointRestorePosition;
                        neckAccessoryLocator.parent.parent.parent.rotation = spine5JointRestoreRotation;
                        neckAccessoryLocator.position = neckJointRestorePosition;
                        neckAccessoryLocator.rotation = neckJointRestoreRotation;

                        break;
                    }
            }
        }

        public void SetWaistAccessory(GameObject accessory, Vector3 accessoryLocalPosition, Quaternion accessoryLocalOrientation = new Quaternion(), AccessoryTransformSpace accessoryTransformSpace = AccessoryTransformSpace.Acccessory)
        {
            switch (accessoryTransformSpace)
            {
                case AccessoryTransformSpace.Acccessory:
                {
                    accessory.transform.localRotation = accessoryLocalOrientation;
                    accessory.transform.SetParent(hipLocator, false);
                    accessory.transform.localRotation = accessory.transform.localRotation;
                    accessory.transform.localPosition = accessory.transform.localPosition;
                    break;
                }
                case AccessoryTransformSpace.BodyPart:
                {
                    accessory.transform.SetParent(hipLocator);
                    accessory.transform.localRotation = accessoryLocalOrientation;
                    accessory.transform.localPosition = accessoryLocalPosition;
                    break;
                }
                case AccessoryTransformSpace.OriginalPose:
                {
                    accessory.transform.localRotation = accessoryLocalOrientation;
                    accessory.transform.localPosition = accessoryLocalPosition;

                    var hipJointRestorePosition = hipLocator.parent.position;
                    var hipJointRestoreRotation = hipLocator.parent.rotation;
                    var waistJointRestoreRotation = waistAccessoryLocator.rotation;

                    hipLocator.parent.position = hipOriginalPosition;
                    hipLocator.parent.rotation = hipOriginalRotation;
                    waistAccessoryLocator.position = waistOriginalPosition;
                    waistAccessoryLocator.rotation = waistOriginalRotation;

                    accessory.transform.SetParent(waistAccessoryLocator, true);

                    hipLocator.parent.position = hipJointRestorePosition;
                    hipLocator.parent.rotation = hipJointRestoreRotation;
                    waistAccessoryLocator.position = torsoLocator.parent.position; // Always have the same position as spine1
                    waistAccessoryLocator.rotation = waistJointRestoreRotation;

                    break;
                }
            }
        }

        public void SetLeftHandAccessory(GameObject accessory, Vector3 accessoryLocalGrabPosition, Quaternion accessoryLocalGrabOrientation = new Quaternion(), AccessoryTransformSpace accessoryTransformSpace = AccessoryTransformSpace.Acccessory)
        {
            SetHandAccessory(accessory, leftHandAccessoryLocator, accessoryLocalGrabPosition, accessoryLocalGrabOrientation, accessoryTransformSpace);
        }

        public void SetRightHandAccessory(GameObject accessory, Vector3 accessoryLocalGrabPosition, Quaternion accessoryLocalGrabOrientation = new Quaternion(), AccessoryTransformSpace accessoryTransformSpace = AccessoryTransformSpace.Acccessory)
        {
            SetHandAccessory(accessory, rightHandAccessoryLocator, accessoryLocalGrabPosition, accessoryLocalGrabOrientation, accessoryTransformSpace);
        }

        void SetHandAccessory(GameObject accessory, Transform handLocator, Vector3 accessoryLocalGrabPosition, Quaternion accessoryLocalGrabOrientation, AccessoryTransformSpace accessoryTransformSpace)
        {
            switch (accessoryTransformSpace)
            {
                case AccessoryTransformSpace.Acccessory:
                    accessory.transform.localRotation = accessoryLocalGrabOrientation;
                    accessory.transform.SetParent(handLocator, false);
                    accessory.transform.localRotation = Quaternion.Euler(15.0f, 0.0f, 0.0f) * accessory.transform.localRotation;
                    accessory.transform.localPosition = handLocator.InverseTransformPoint(accessory.transform.TransformPoint(-accessoryLocalGrabPosition)) + Vector3.forward * 0.4f;
                    break;
                case AccessoryTransformSpace.BodyPart:
                    accessory.transform.SetParent(handLocator);
                    accessory.transform.localRotation = accessoryLocalGrabOrientation;
                    accessory.transform.localPosition = accessoryLocalGrabPosition;
                    break;
                case AccessoryTransformSpace.OriginalPose:
                    throw new InvalidOperationException("Cannot transform hand accessories in original pose for the minifig.");
            }
        }

        public void SetTorsoMaterial(Material material)
        {
            SetMaterial(torsoGeometry, material);
        }

        public Material GetTorsoMaterial()
        {
            return GetMaterial(torsoGeometry);
        }

        public void SetTorsoFrontDecorationMaterial(Material material)
        {
            SetMaterial(torsoFrontDecorationGeometry, material);
        }

        public Material GetTorsoFrontDecorationMaterial()
        {
            return GetMaterial(torsoFrontDecorationGeometry);
        }

        public Transform GetTorsoFrontDecoration()
        {
            return torsoFrontDecorationGeometry;
        }

        public void SetTorsoBackDecorationMaterial(Material material)
        {
            SetMaterial(torsoBackDecorationGeometry, material);
        }

        public Material GetTorsoBackDecorationMaterial()
        {
            return GetMaterial(torsoBackDecorationGeometry);
        }

        public Transform GetTorsoBackDecoration()
        {
            return torsoBackDecorationGeometry;
        }

        public void SetLeftArmMaterial(Material material)
        {
            SetMaterial(armsGeometry[0], material);
        }

        public Material GetLeftArmMaterial()
        {
            return GetMaterial(armsGeometry[0]);
        }

        public void SetRightArmMaterial(Material material)
        {
            SetMaterial(armsGeometry[1], material);
        }

        public Material GetRightArmMaterial()
        {
            return GetMaterial(armsGeometry[1]);
        }

        public void SetArmsMaterial(Material material)
        {
            SetMaterial(armsGeometry, material);
        }

        public void SetLeftHandMaterial(Material material)
        {
            SetMaterial(handsGeometry[0], material);
        }

        public Material GetLeftHandMaterial()
        {
            return GetMaterial(handsGeometry[0]);
        }

        public void SetRightHandMaterial(Material material)
        {
            SetMaterial(handsGeometry[1], material);
        }

        public Material GetRightHandMaterial()
        {
            return GetMaterial(handsGeometry[1]);
        }

        public void SetHandsMaterial(Material material)
        {
            SetMaterial(handsGeometry, material);
        }

        public void SetHipMaterial(Material material)
        {
            SetMaterial(legsGeometry[0], material);
        }

        public Material GetHipMaterial()
        {
            return GetMaterial(legsGeometry[0]);
        }

        public void SetHipDecorationMaterial(Material material)
        {
            SetMaterial(legsGeometry[1], material);
        }

        public Material GetHipDecorationMaterial()
        {
            return GetMaterial(legsGeometry[1]);
        }

        public Transform GetHipDecoration()
        {
            return legsGeometry[1];
        }

        public void SetLeftLegMaterial(Material material)
        {
            SetMaterial(legsGeometry[2], material);
        }

        public Material GetLeftLegMaterial()
        {
            return GetMaterial(legsGeometry[2]);
        }

        public void SetLeftLegFrontDecorationMaterial(Material material)
        {
            SetMaterial(legsGeometry[3], material);
        }

        public Material GetLeftLegFrontDecorationMaterial()
        {
            return GetMaterial(legsGeometry[3]);
        }

        public Transform GetLeftLegFrontDecoration()
        {
            return legsGeometry[3];
        }

        public void SetLeftLegSideDecorationMaterial(Material material)
        {
            SetMaterial(legsGeometry[4], material);
        }

        public Material GetLeftLegSideDecorationMaterial()
        {
            return GetMaterial(legsGeometry[4]);
        }

        public Transform GetLeftLegSideDecoration()
        {
            return legsGeometry[4];
        }

        public void SetRightLegMaterial(Material material)
        {
            SetMaterial(legsGeometry[5], material);
        }

        public Material GetRightLegMaterial()
        {
            return GetMaterial(legsGeometry[5]);
        }

        public void SetRightLegFrontDecorationMaterial(Material material)
        {
            SetMaterial(legsGeometry[6], material);
        }

        public Material GetRightLegFrontDecorationMaterial()
        {
            return GetMaterial(legsGeometry[6]);
        }

        public Transform GetRightLegFrontDecoration()
        {
            return legsGeometry[6];
        }

        public void SetRightLegSideDecorationMaterial(Material material)
        {
            SetMaterial(legsGeometry[7], material);
        }

        public Material GetRightLegSideDecorationMaterial()
        {
            return GetMaterial(legsGeometry[7]);
        }

        public Transform GetRightLegSideDecoration()
        {
            return legsGeometry[7];
        }

        public void SetHeadMaterial(Material material)
        {
            SetMaterial(headGeometry, material);
        }

        public Material GetHeadMaterial()
        {
            return GetMaterial(headGeometry);
        }

        public void SetFaceMaterial(Material material)
        {
            SetMaterial(faceGeometry, material);
        }

        public Material GetFaceMaterial()
        {
            return GetMaterial(faceGeometry);
        }

        public List<Transform> GetTorso()
        {
            return new List<Transform> { torsoGeometry, torsoFrontDecorationGeometry, torsoBackDecorationGeometry };
        }

        public List<Transform> GetLeftArm()
        {
            return armsGeometry.GetRange(0, 1);
        }

        public List<Transform> GetRightArm()
        {
            return armsGeometry.GetRange(1, 1);
        }

        public List<Transform> GetHip()
        {
            return legsGeometry.GetRange(0, 2);
        }

        public List<Transform> GetLeftLeg()
        {
            return legsGeometry.GetRange(2, 3);
        }

        public List<Transform> GetRightLeg()
        {
            return legsGeometry.GetRange(5, 3);
        }

        public Transform GetLeftHand()
        {
            return handsGeometry[0];
        }

        public Transform GetRightHand()
        {
            return handsGeometry[1];
        }

        public List<Transform> GetHead()
        {
            return new List<Transform> { headGeometry, faceGeometry };
        }

        public Transform GetFace()
        {
            return faceGeometry;
        }

        public Transform GetHipLocator()
        {
            return hipLocator;
        }

        public Transform GetTorsoLocator()
        {
            return torsoLocator;
        }

        public Transform GetHeadAccessory()
        {
            return GetAccessory(headAccessoryLocator);
        }

        public Transform GetNeckAccessory()
        {
            return GetAccessory(neckAccessoryLocator, 2);
        }

        public Transform GetCapeAccessory()
        {
            return GetAccessory(capeAccessoryLocator, 2);
        }

        public Transform GetWaistAccessory()
        {
            return GetAccessory(hipLocator);
        }

        public Transform GetLeftHandAccessory()
        {
            return GetAccessory(leftHandAccessoryLocator);
        }

        public Transform GetRightHandAccessory()
        {
            return GetAccessory(rightHandAccessoryLocator);
        }

        Transform GetAccessory(Transform accessoryLocator, int childIndex = 0)
        {
            if (accessoryLocator.childCount > childIndex)
            {
                return accessoryLocator.GetChild(childIndex);
            }
            else
            {
                return null;
            }
        }

        void SetMaterial(List<Transform> transforms, Material material)
        {
            foreach (var transform in transforms)
            {
                SetMaterial(transform, material);
            }
        }

        void SetMaterial(Transform transform, Material material)
        {
            if (material)
            {
                transform.gameObject.SetActive(true);
                transform.GetComponent<Renderer>().material = material;
            }
            else
            {
                transform.gameObject.SetActive(false);
            }
        }

        Material GetMaterial(Transform transform)
        {
            var renderer = transform.GetComponent<Renderer>();
            if (renderer)
            {
                return renderer.material;
            }

            return null;
        }
    }

}
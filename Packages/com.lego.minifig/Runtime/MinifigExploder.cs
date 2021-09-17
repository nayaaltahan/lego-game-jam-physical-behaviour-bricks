using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations.Rigging;

namespace LEGOMinifig
{

    public static class MinifigExploder
    {
        private class EnableCollider : MonoBehaviour
        {
            private Collider theCollider;
            private int fixedUpdateDelay = 20;

            void Start()
            {
                theCollider = GetComponent<Collider>();
            }

            void FixedUpdate()
            {
                if (fixedUpdateDelay <= 0)
                {
                    theCollider.isTrigger = false;
                    Destroy(this);
                }

                fixedUpdateDelay--;
            }
        }

        private class BlinkAndDestroy : MonoBehaviour
        {
            public float timeLeft = 4.0f;

            private float blinkPeriod = 0.8f;
            private float blinkFrequency = 0.1f;

            private Renderer theRenderer;
            private float timeBlink; 

            void Start()
            {
                theRenderer = GetComponent<Renderer>();
                timeLeft += Random.Range(-0.3f, 0.3f);
            }

            void Update()
            {
                timeLeft -= Time.deltaTime;

                if (timeLeft <= blinkPeriod)
                {
                    if (timeBlink <= 0.0f)
                    {
                        timeBlink += blinkFrequency;
                        theRenderer.enabled = !theRenderer.enabled;
                    }

                    timeBlink -= Time.deltaTime;
                }

                if (timeLeft <= 0.0f)
                {
                    Destroy(gameObject);
                }
            }
        }

        public static void Explode(Minifig minifig, Rig leftArmRig, Rig rightArmRig, Rig leftLegRig, Rig rightLegRig, Rig headRig, Vector3 speed, float angularSpeed)
        {
            const float spreadForce = 6.0f;
            const float removeDelay = 3.0f;

            // FIXME Add speed, angularSpeed and spreadForce to rigid body.

            var leftArmIKConstraint = leftArmRig.GetComponentInChildren<TwoBoneIKConstraint>();
            var rightArmIKConstraint = rightArmRig.GetComponentInChildren<TwoBoneIKConstraint>();
            ExplodeWithTwoBoneIKConstraint(minifig.transform, "Left arm", minifig.GetLeftArm(), leftArmIKConstraint, 0.3f, speed, angularSpeed, spreadForce, removeDelay, 0.16f, -0.16f);
            ExplodeWithTwoBoneIKConstraint(minifig.transform, "Right arm", minifig.GetRightArm(), rightArmIKConstraint, 0.3f, speed, angularSpeed, spreadForce, removeDelay, 0.16f, -0.16f);

            var leftHandTransforms = new List<Transform> { leftArmIKConstraint.data.tip, leftArmIKConstraint.data.tip.parent };
            var rightHandTransforms = new List<Transform> { rightArmIKConstraint.data.tip, rightArmIKConstraint.data.tip.parent };
            var leftHandColliderCenters = new List<Vector3> { Vector3.forward * 0.2f, Vector3.zero };
            var rightHandColliderCenters = new List<Vector3> { Vector3.back * 0.2f, Vector3.zero };
            var handColliderSizes = new List<Vector3> { new Vector3(0.4f, 0.4f, 0.4f), new Vector3(0.2f, 0.4f, 0.2f) };
            var handColliderInitialTriggers = new List<bool> { false, true };
            ExplodeWithTransforms(minifig.transform, "Left hand", new List<Transform> { minifig.GetLeftHand() }, leftHandTransforms, leftHandColliderCenters, handColliderSizes, handColliderInitialTriggers, speed, angularSpeed, spreadForce, removeDelay);
            ExplodeWithTransforms(minifig.transform, "Right hand", new List<Transform> { minifig.GetRightHand() }, rightHandTransforms, rightHandColliderCenters, handColliderSizes, handColliderInitialTriggers, speed, angularSpeed, spreadForce, removeDelay);

            var leftLegIKConstraint = leftLegRig.GetComponentInChildren<TwoBoneIKConstraint>();
            var rightLegIKConstraint = rightLegRig.GetComponentInChildren<TwoBoneIKConstraint>();
            ExplodeWithTwoBoneIKConstraint(minifig.transform, "Left leg", minifig.GetLeftLeg(), leftLegIKConstraint, 0.5f, speed, angularSpeed, spreadForce, removeDelay, 0.16f);
            ExplodeWithTwoBoneIKConstraint(minifig.transform, "Right leg", minifig.GetRightLeg(), rightLegIKConstraint, 0.5f, speed, angularSpeed, spreadForce, removeDelay, 0.16f);

            var hipTransforms = new List<Transform> { leftLegIKConstraint.data.root.parent, leftLegIKConstraint.data.root.parent, leftLegIKConstraint.data.root.parent, leftLegIKConstraint.data.root.parent };
            var hipColliderCenters = new List<Vector3> { Vector3.up * 0.4f, Vector3.zero, new Vector3(0.0f, 0.7f, 0.4f), new Vector3(0.0f, 0.7f, -0.4f) };
            var hipColliderSizes = new List<Vector3> { new Vector3(0.8f, 0.2f, 1.6f), new Vector3(0.7f, 0.7f, 0.2f), new Vector3(0.4f, 0.4f, 0.4f), new Vector3(0.4f, 0.4f, 0.4f) };
            var hipColliderInitialTriggers = new List<bool> { false, false, true, true };
            ExplodeWithTransforms(minifig.transform, "Hip", minifig.GetHip(), hipTransforms, hipColliderCenters, hipColliderSizes, hipColliderInitialTriggers, speed, angularSpeed, spreadForce, removeDelay);

            var headAimConstraint = headRig.GetComponentInChildren<MultiAimConstraint>();
            // Use neck, spine05 and spine01 transforms.
            var torsoTransforms = new List<Transform> { minifig.GetTorsoLocator().parent.GetChild(0).GetChild(0).GetChild(0).GetChild(0).GetChild(0), minifig.GetTorsoLocator().parent };
            var torsoColliderCenters = new List<Vector3> { Vector3.up * 0.1f, Vector3.up * 0.3f };
            var torsoColliderSizes = new List<Vector3> { new Vector3(0.8f, 0.5f, 1.2f), new Vector3(0.8f, 0.5f, 1.4f) };
            var torsoColliderInitialTriggers = new List<bool> { false, false };
            ExplodeWithTransforms(minifig.transform, "Torso", minifig.GetTorso(), torsoTransforms, torsoColliderCenters, torsoColliderSizes, torsoColliderInitialTriggers, speed, angularSpeed, spreadForce, removeDelay);

            var headTransforms = new List<Transform> { headAimConstraint.data.constrainedObject };
            var headColliderCenters = new List<Vector3> { Vector3.up * 0.48f };
            var headColliderSizes = new List<Vector3> { new Vector3(0.8f, 0.96f, 0.8f) };
            var headColliderInitialTriggers = new List<bool> { false };
            ExplodeWithTransforms(minifig.transform, "Head", minifig.GetHead(), headTransforms, headColliderCenters, headColliderSizes, headColliderInitialTriggers, speed, angularSpeed, spreadForce, removeDelay);

            var headAccessory = minifig.GetHeadAccessory();
            if (headAccessory)
            {
                var headAccessoryTransforms = new List<Transform> { headAimConstraint.data.constrainedObject.GetChild(0).GetChild(0) };
                // FIXME Get correct colliders for accessories.
                var headAccessoryColliderCenters = new List<Vector3> { Vector3.zero };
                var headAccessoryColliderSizes = new List<Vector3> { new Vector3(0.8f, 0.96f, 0.8f) };
                var headAccessoryColliderInitialTriggers = new List<bool> { true };
                ExplodeWithTransforms(minifig.transform, "Head accessory", new List<Transform> { headAccessory }, headAccessoryTransforms, headAccessoryColliderCenters, headAccessoryColliderSizes, headAccessoryColliderInitialTriggers, speed, angularSpeed, spreadForce + 1.5f, removeDelay, false);
            }

            var leftHandAccessory = minifig.GetLeftHandAccessory();
            if (leftHandAccessory)
            {
                var leftHandAccessoryTransforms = new List<Transform> { leftArmIKConstraint.data.tip.GetChild(0).GetChild(2) };
                // FIXME Get correct colliders for accessories including offset between accessory center and grab position. This should be retrievable from Minifig.
                var leftHandAccessoryColliderCenters = new List<Vector3> { Vector3.forward * 0.4f };
                var leftHandAccessoryColliderSizes = new List<Vector3> { new Vector3(0.8f, 0.96f, 0.8f) };
                var leftHandAccessoryColliderInitialTriggers = new List<bool> { true };
                ExplodeWithTransforms(minifig.transform, "Left hand accessory", new List<Transform> { leftHandAccessory }, leftHandAccessoryTransforms, leftHandAccessoryColliderCenters, leftHandAccessoryColliderSizes, leftHandAccessoryColliderInitialTriggers, speed, angularSpeed, spreadForce, removeDelay, false);
            }

            var rightHandAccessory = minifig.GetRightHandAccessory();
            if (rightHandAccessory)
            {
                var rightHandAccessoryTransforms = new List<Transform> { rightArmIKConstraint.data.tip.GetChild(0).GetChild(2) };
                // FIXME Get correct colliders for accessories including offset between accessory center and grab position. This should be retrievable from Minifig.
                var rightHandAccessoryColliderCenters = new List<Vector3> { Vector3.forward * 0.4f };
                var rightHandAccessoryColliderSizes = new List<Vector3> { new Vector3(0.8f, 0.96f, 0.8f) };
                var rightHandAccessoryColliderInitialTriggers = new List<bool> { true };
                ExplodeWithTransforms(minifig.transform, "Right hand accessory", new List<Transform> { rightHandAccessory }, rightHandAccessoryTransforms, rightHandAccessoryColliderCenters, rightHandAccessoryColliderSizes, rightHandAccessoryColliderInitialTriggers, speed, angularSpeed, spreadForce, removeDelay, false);
            }
        }

        private static void ExplodeWithTwoBoneIKConstraint(Transform parentTransform, string name, List<Transform> meshes, TwoBoneIKConstraint twoBoneIKConstraint, float colliderWidth, Vector3 speed, float angularSpeed, float spreadForce, float removeDelay, float rootColliderLengthModification = 0.0f, float tipColliderLengthModification = 0.0f)
        {
            var resultGO = CreateMeshGO(name, meshes, removeDelay, true);

            AddOrientedColliderFromTwoPositions(resultGO, twoBoneIKConstraint.data.root.position, twoBoneIKConstraint.data.mid.position, colliderWidth, rootColliderLengthModification);
            AddOrientedColliderFromTwoPositions(resultGO, twoBoneIKConstraint.data.tip.position, twoBoneIKConstraint.data.mid.position, colliderWidth, tipColliderLengthModification);

            AddAndMoveRigidBody(resultGO, parentTransform, speed, angularSpeed, spreadForce);
        }

        private static void ExplodeWithTransforms(Transform parentTransform, string name, List<Transform> meshes, List<Transform> transforms, List<Vector3> colliderCenters, List<Vector3> colliderSizes, List<bool> colliderInitialTriggers, Vector3 speed, float angularSpeed, float spreadForce, float removeDelay, bool bakeMesh = true)
        {
            var resultGO = CreateMeshGO(name, meshes, removeDelay, bakeMesh);

            for (var i = 0; i < transforms.Count; ++i)
            {
                AddOrientedColliderFromTransform(resultGO, transforms[i], colliderCenters[i], colliderSizes[i], colliderInitialTriggers[i]);
            }

            AddAndMoveRigidBody(resultGO, parentTransform, speed, angularSpeed, spreadForce);
        }

        private static GameObject CreateMeshGO(string name, List<Transform> meshes, float removeDelay, bool bakeMesh)
        {
            var resultGO = new GameObject(name);
            resultGO.transform.localPosition = Vector3.zero;
            resultGO.transform.localRotation = Quaternion.identity;

            var resultMesh = new Mesh();

            var resultFilter = resultGO.AddComponent<MeshFilter>();
            resultFilter.sharedMesh = resultMesh;

            var resultRenderer = resultGO.AddComponent<MeshRenderer>();
            var materials = new List<Material>();

            var combineInstances = new List<CombineInstance>();
            for (var i = 0; i < meshes.Count; ++i)
            {
                var mesh = meshes[i];

                // Skip inactive meshes.
                if (!mesh.gameObject.activeInHierarchy)
                {
                    continue;
                }

                // Just parent the result with the parent of the first active mesh.
                if (!resultGO.transform.parent)
                {
                    resultGO.transform.SetParent(mesh.parent, false);
                }

                var worldToLocalRotation = Quaternion.Inverse(resultGO.transform.rotation);

                if (bakeMesh)
                {
                    var renderer = mesh.GetComponent<SkinnedMeshRenderer>();
                    materials.Add(renderer.sharedMaterial);

                    var bakedMesh = new Mesh();
                    renderer.BakeMesh(bakedMesh);

                    combineInstances.Add(
                        new CombineInstance
                        {
                            mesh = bakedMesh,
                            transform = Matrix4x4.TRS(resultGO.transform.InverseTransformPoint(mesh.transform.position), worldToLocalRotation * mesh.transform.rotation, Vector3.one)
                        }
                    );
                }
                else
                {
                    var renderers = mesh.GetComponentsInChildren<MeshRenderer>();
                    foreach (var renderer in renderers)
                    {
                        materials.Add(renderer.sharedMaterial);
                    }

                    var filters = mesh.GetComponentsInChildren<MeshFilter>();
                    foreach (var filter in filters)
                    {
                        combineInstances.Add(
                            new CombineInstance
                            {
                                mesh = filter.sharedMesh,
                                transform = Matrix4x4.TRS(resultGO.transform.InverseTransformPoint(filter.transform.position), worldToLocalRotation * filter.transform.rotation, Vector3.one)
                            });
                    }
                }

                mesh.gameObject.SetActive(false);
            }

            resultRenderer.sharedMaterials = materials.ToArray();

            resultMesh.CombineMeshes(combineInstances.ToArray(), false, true);

            if (removeDelay > 0.0f)
            {
                var blinkAndDestroy = resultGO.AddComponent<BlinkAndDestroy>();
                blinkAndDestroy.timeLeft = removeDelay;
            }

            return resultGO;
        }

        private static void AddOrientedColliderFromTwoPositions(GameObject parent, Vector3 positionA, Vector3 positionB, float colliderWidth, float colliderLengthModification = 0.0f)
        {
            var direction = positionA - positionB;
            var position = (positionA + positionB) * 0.5f + direction.normalized * colliderLengthModification * 0.5f;

            var colliderGO = new GameObject("Collider");
            colliderGO.transform.parent = parent.transform;

            colliderGO.transform.position = position;
            colliderGO.transform.rotation = Quaternion.LookRotation(direction);

            var collider = colliderGO.AddComponent<BoxCollider>();
            collider.size = new Vector3(colliderWidth, colliderWidth, direction.magnitude + colliderLengthModification);
        }

        private static void AddOrientedColliderFromTransform(GameObject parent, Transform transform, Vector3 colliderCenter, Vector3 colliderSize, bool colliderInitialTrigger)
        {
            var colliderGO = new GameObject("Collider");
            colliderGO.transform.parent = parent.transform;

            colliderGO.transform.position = transform.position;
            colliderGO.transform.rotation = transform.rotation;

            var collider = colliderGO.AddComponent<BoxCollider>();
            collider.center = colliderCenter;
            collider.size = colliderSize;
            collider.isTrigger = colliderInitialTrigger;

            if (colliderInitialTrigger)
            {
                colliderGO.AddComponent<EnableCollider>();
            }
        }

        private static void AddAndMoveRigidBody(GameObject go, Transform parentTransform, Vector3 speed, float angularSpeed, float spreadForce)
        {
            var rigidBody = go.AddComponent<Rigidbody>();
            rigidBody.AddForce(speed, ForceMode.VelocityChange);
            rigidBody.AddRelativeTorque(0.0f, angularSpeed * Mathf.Deg2Rad, 0.0f, ForceMode.VelocityChange);
            rigidBody.AddExplosionForce(spreadForce, parentTransform.position, 0.0f, 0.0f, ForceMode.VelocityChange);
        }
    }
}

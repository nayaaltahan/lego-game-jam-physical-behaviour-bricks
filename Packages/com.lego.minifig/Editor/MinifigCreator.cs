// Copyright (C) LEGO System A/S - All Rights Reserved
// Unauthorized copying of this file, via any medium is strictly prohibited

using UnityEngine;
using UnityEditor;

namespace LEGOMinifig
{

    public class MinifigCreator
    {
        public static GameObject InstantiateMinifig(MinifigUtility.AccessoryInfo hatOrHair, Color[] hatOrHairColours, MinifigUtility.FaceInfo face, bool[] includeFaceAnimations, MinifigUtility.BodyInfo body, Color legColour)
        {
            var minifigPrefab = MinifigUtility.LoadMinifigPrefab();
            var minifigGO = Object.Instantiate<GameObject>(minifigPrefab);

            Undo.RegisterCreatedObjectUndo(minifigGO, "Import");

            var minifig = minifigGO.GetComponent<Minifig>();

            // Clean out unincluded face animations.
            for (var i = face.animations.Count - 1; i >= 0; i--)
            {
                if (!includeFaceAnimations[i])
                {
                    face.animations.RemoveAt(i);
                }
            }

            // Setup hat or hair.
            if (hatOrHair != null)
            {
                var objectToInstantiate = MinifigUtility.LoadHatFBX(hatOrHair.name);
                var normalMap = MinifigUtility.LoadHatNormalMap(hatOrHair.name);
                if (!objectToInstantiate)
                {
                    objectToInstantiate = MinifigUtility.LoadHairFBX(hatOrHair.name);
                    normalMap = MinifigUtility.LoadHairNormalMap(hatOrHair.name);
                }

                var headAccessory = Object.Instantiate(objectToInstantiate);

                // Deactivate decoration surfaces.
                var decorationSurfaces = headAccessory.transform.Find("DecorationSurfaces");
                if (decorationSurfaces)
                {
                    decorationSurfaces.gameObject.SetActive(false);
                }

                // Assign correct colours.
                var headAccessoryRenderers = headAccessory.GetComponentsInChildren<Renderer>();
                foreach (var renderer in headAccessoryRenderers)
                {
                    // Choose first colour for shell and legacy mesh.
                    var chosenColour = hatOrHairColours[0];

                    // Colour change surface, so find the right colour.
                    if (renderer.name.StartsWith("Exp_"))
                    {
                        var colourIndex = int.Parse(renderer.name.Substring(renderer.name.Length - 1));
                        if (colourIndex < hatOrHairColours.Length)
                        {
                            chosenColour = hatOrHairColours[colourIndex];
                        }
                    }

                    var material = MinifigUtility.GetHatOrHairMaterial(hatOrHair.name, chosenColour, normalMap);
                    renderer.sharedMaterial = material;
                }

                minifig.SetHeadAccessory(headAccessory);
            }

            // Setup body materials.
            Texture2D bodyFrontTexture = null;
            Texture2D bodyBackTexture = null;
            if (!string.IsNullOrEmpty(body.frontTextureName))
            {
                MinifigUtility.UnpackBody(body.frontTextureName);
                bodyFrontTexture = MinifigUtility.LoadBodyTexture(body.frontTextureName);
            }
            if (!string.IsNullOrEmpty(body.backTextureName))
            {
                MinifigUtility.UnpackBody(body.backTextureName);
                bodyBackTexture = MinifigUtility.LoadBodyTexture(body.backTextureName);
            }

            var torsoMaterials = MinifigUtility.GetTorsoMaterials(body.torsoColour, bodyFrontTexture, bodyBackTexture);
            var armMaterial = MinifigUtility.GetArmMaterial(body.armColour);
            var handMaterial = MinifigUtility.GetHandMaterial(body.handColour);
            var legMaterials = MinifigUtility.GetLegMaterials(legColour);

            minifig.SetTorsoMaterial(torsoMaterials[0]);
            minifig.SetTorsoFrontDecorationMaterial(torsoMaterials[1]);
            minifig.SetTorsoBackDecorationMaterial(torsoMaterials[2]);
            minifig.SetArmsMaterial(armMaterial);
            minifig.SetHandsMaterial(handMaterial);

            minifig.SetHipMaterial(legMaterials[0]);
            minifig.SetHipDecorationMaterial(legMaterials[1]);
            minifig.SetLeftLegMaterial(legMaterials[2]);
            minifig.SetLeftLegFrontDecorationMaterial(legMaterials[3]);
            minifig.SetLeftLegSideDecorationMaterial(legMaterials[4]);
            minifig.SetRightLegMaterial(legMaterials[5]);
            minifig.SetRightLegFrontDecorationMaterial(legMaterials[6]);
            minifig.SetRightLegSideDecorationMaterial(legMaterials[7]);

            // Setup face animations.
            if (!MinifigUtility.CheckIfFaceIsUnpacked(face))
            {
                MinifigUtility.UnpackFace(face);
            };
            var faceTexture = MinifigUtility.LoadFaceTexture(face.name);
            var faceMaterial = MinifigUtility.GetFaceMaterial(face.name, faceTexture);

            minifig.SetFaceMaterial(faceMaterial);

            MinifigUtility.AddFaceAnimationController(minifig, face);

            return minifigGO;
        }
    }

}
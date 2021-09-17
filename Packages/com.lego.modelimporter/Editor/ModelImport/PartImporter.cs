// Copyright (C) LEGO System A/S - All Rights Reserved
// Unauthorized copying of this file, via any medium is strictly prohibited

using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;
using System.Text.RegularExpressions;
using LEGOMaterials;

namespace LEGOModelImporter
{

    public class PartImporter
    {
        /// <summary>
        /// Instantiate game objects for a specified brick
        /// </summary>
        /// <param name="designId">The design id of the part</param>
        /// <param name="colourId">The colour id of the part</param>
        /// <param name="importSettings">Import settings to use</param>
        public static Brick InstantiateBrick(string designId, int colourId, ModelGroupImportSettings importSettings)
        {
            // Determine whether or not to be static and to generate light map UVs.
            var brickStatic = importSettings.isStatic;
            var brickLightmapped = brickStatic && importSettings.lightmapped;
            var brickLod = importSettings.lod;

            var brickGO = new GameObject(designId, typeof(Brick));
            var brickComp = brickGO.GetComponent<Brick>();

            GameObject partToInstantiate = null;

            var partExistenceResult = PartUtility.UnpackPart(designId, brickLightmapped, brickLod);

            if (partExistenceResult.existence != PartUtility.PartExistence.None)
            {
                // FIXME Make a note of changed design ids.
                partToInstantiate = PartUtility.LoadPart(partExistenceResult.designID, brickLightmapped, partExistenceResult.existence == PartUtility.PartExistence.Legacy, brickLod);
            }

            if (partToInstantiate == null)
            {
                Debug.LogError("Missing part FBX -> " + partExistenceResult.designID);
                return null;
            }
            var partGO = Object.Instantiate(partToInstantiate);
            partGO.name = partToInstantiate.name;

            // Assign legacy, material IDs and set up references.
            var partComp = partGO.AddComponent<Part>();
            partComp.designID = Convert.ToInt32(partExistenceResult.designID);
            partComp.legacy = partExistenceResult.existence == PartUtility.PartExistence.Legacy;

            var materialCount = 1;
            var colourChangeSurfaces = partComp.transform.Find("ColourChangeSurfaces");
            if (colourChangeSurfaces)
            {
                materialCount += colourChangeSurfaces.childCount;
            }
            var materials = new LXFMLDoc.Brick.Part.Material[materialCount];
            for(var i = 0; i < materialCount; ++i)
            {
                var material = new LXFMLDoc.Brick.Part.Material() { colorId = colourId, shaderId = 0 };
                materials[i] = material;
                partComp.materialIDs.Add(material.colorId);
            }

            partComp.brick = brickComp;
            brickComp.parts.Add(partComp);

            if (partExistenceResult.existence == PartUtility.PartExistence.New)
            {
                // Instantiate and setup knobs and tubes.
                // FIXME Handle normal mapped model.
                InstantiateKnobsAndTubes(partComp, brickLightmapped, brickLod);

                // Setup inside details.
                var detail = partComp.transform.Find("Detail");
                if (detail)
                {
                    foreach(Transform detailTransform in detail)
                    {
                        var insideDetailComp = detailTransform.gameObject.AddComponent<InsideDetail>();
                        insideDetailComp.part = partComp;
                        partComp.insideDetails.Add(insideDetailComp);
                    }
                }
            }

            // Create collider and connectivity information.
            var brickColliders = importSettings.colliders;
            var brickConnectivity = brickColliders && importSettings.connectivity;

            if (brickColliders)
            {
                GameObject collidersToInstantiate = null;

                var collidersAvailable = PartUtility.UnpackCollidersForPart(partExistenceResult.designID);
                if (collidersAvailable)
                {
                    collidersToInstantiate = PartUtility.LoadCollidersPrefab(partExistenceResult.designID);
                }

                if (collidersToInstantiate == null && partExistenceResult.existence != PartUtility.PartExistence.Legacy)
                {
                    Debug.LogError("Missing part collider information -> " + partExistenceResult.designID);
                }

                if (collidersToInstantiate)
                {
                    var collidersGO = Object.Instantiate(collidersToInstantiate);
                    collidersGO.name = "Colliders";
                    collidersGO.transform.SetParent(partGO.transform, false);
                    var colliderComp = collidersGO.GetComponent<Colliders>();
                    partComp.colliders = colliderComp;
                    colliderComp.part = partComp;
                }
            }

            if (brickConnectivity)
            {
                GameObject connectivityToInstantiate = null;

                var connectivityAvailable = PartUtility.UnpackConnectivityForPart(partExistenceResult.designID);
                if (connectivityAvailable)
                {
                    connectivityToInstantiate = PartUtility.LoadConnectivityPrefab(partExistenceResult.designID);
                }

                if (connectivityToInstantiate == null && partExistenceResult.existence != PartUtility.PartExistence.Legacy)
                {
                    Debug.LogError("Missing part connectivity information -> " + partExistenceResult.designID);
                }

                if (connectivityToInstantiate)
                {
                    var connectivityGO = Object.Instantiate(connectivityToInstantiate);
                    connectivityGO.name = "Connectivity";
                    connectivityGO.transform.SetParent(partGO.transform, false);
                    var connectivityComp = connectivityGO.GetComponent<Connectivity>();
                    partComp.connectivity = connectivityComp;
                    connectivityComp.part = partComp;

                    foreach (var field in connectivityComp.planarFields)
                    {
                        foreach (var connection in field.connections)
                        {
                            MatchConnectionWithKnob(connection, partComp.knobs);
                            MatchConnectionWithTubes(connection, partComp.tubes);
                        }

                        MatchFieldWithInsideDetail(field, partComp.insideDetails);
                    }
                }
            }

            SetMaterials(partComp, materials, partExistenceResult.existence == PartUtility.PartExistence.Legacy);
            // TODO Add decoration support.
            SetDecorations(partComp, null, partExistenceResult.existence == PartUtility.PartExistence.Legacy);

            SetStaticAndGIParams(partGO, brickStatic, brickLightmapped, true);

            brickGO.transform.position = partGO.transform.position;
            brickGO.transform.rotation = partGO.transform.rotation;
            brickGO.transform.localScale = Vector3.one;
            partGO.transform.SetParent(brickGO.transform, true);

            // If all parts were missing, discard brick.
            if (brickGO.transform.childCount == 0)
            {
                Object.DestroyImmediate(brickGO);
                return null;
            }

            Undo.RegisterCreatedObjectUndo(brickGO, "Creating part");

            SetStaticAndGIParams(brickGO, brickStatic, brickLightmapped);

            // Assign uuid
            brickComp.designID = Convert.ToInt32(designId);
            brickComp.uuid = Guid.NewGuid().ToString();

            var oldBrickPos = brickGO.transform.position;
            var oldBrickRot = brickGO.transform.rotation;

            brickGO.transform.position = Vector3.zero;
            brickGO.transform.rotation = Quaternion.identity;

            var bounds = ComputeBounds(brickGO.transform);
            var corners = BrickBuildingUtility.GetBoundingCorners(bounds, Matrix4x4.identity);

            BrickBuildingUtility.GetMinMax(corners, out Vector3 min, out Vector3 max);
            bounds.SetMinMax(min, max);

            brickGO.transform.position = oldBrickPos;
            brickGO.transform.rotation = oldBrickRot;

            brickComp.totalBounds = bounds;

            return brickComp;
        }

        public static void MatchConnectionWithKnob(PlanarFeature connection, List<Knob> knobs)
        {
            var POS_EPSILON = 0.01f;
            var ROT_EPSILON = 0.01f;
            var position = connection.GetPosition();
            foreach (var knob in knobs)
            {
                if (Vector3.Distance(position, knob.transform.position) < POS_EPSILON && 1.0f - Vector3.Dot(connection.field.transform.up, knob.transform.up) < ROT_EPSILON)
                {
                    connection.knob = knob;
                    knob.connectionIndex = connection.index;
                    knob.field = connection.field as PlanarField;
                    return;
                }
            }
        }

        public static void MatchConnectionWithTubes(PlanarFeature connection, List<Tube> tubes)
        {
            // FIXME Temporary fix to tube removal while we work on connections that are related/non-rejecting but not connected.
            if (connection.IsRelevantForTube())
            {
                var position = connection.GetPosition();
                var DIST_EPSILON = 0.01f * 0.01f;
                var ROT_EPSILON = 0.01f;
                foreach (var tube in tubes)
                {
                    var meshFilter = tube.GetComponent<MeshFilter>();
                    if(!meshFilter || !meshFilter.sharedMesh)
                    {
                        continue;
                    }

                    var bounds = meshFilter.sharedMesh.bounds;
                    var extents = bounds.extents;
                    extents.x += 0.4f;
                    extents.z += 0.4f;
                    bounds.extents = extents;
                    var localConnectionPosition = tube.transform.InverseTransformPoint(position);

                    if (bounds.SqrDistance(localConnectionPosition) < DIST_EPSILON && 1.0f - Vector3.Dot(connection.field.transform.up, tube.transform.up) < ROT_EPSILON)
                    {
                        connection.tubes.Add(tube);
                        tube.connections.Add(connection.index);
                        tube.field = connection.field as PlanarField;
                    }

                    if (connection.tubes.Count == 4)
                    {
                        return;
                    }
                }
            }
        }

        public static void MatchFieldWithInsideDetail(PlanarField field, List<InsideDetail> insideDetails)
        {
            foreach (var insideDetail in insideDetails)
            {
                var meshFilter = insideDetail.GetComponent<MeshFilter>();
                if (!meshFilter || !meshFilter.sharedMesh)
                {
                    continue;
                }

                var bounds = meshFilter.sharedMesh.bounds;
                var extents = bounds.extents;
                extents += Vector3.one * 0.02f; // This should match the chamfer size.
                bounds.extents = extents;

                // Find the distance between the bounds and field in local 2D space of the inside details.
                // The height difference check is an extra filter
                var fieldSize = new Vector3(field.gridSize.x, 0.0f, field.gridSize.y) * BrickBuildingUtility.LU_5 * 0.5f;
                var localCenter = new Vector3(-fieldSize.x, 0.0f, fieldSize.z);
                var fieldCenter = field.transform.TransformPoint(localCenter);
                var centerLocal = insideDetail.transform.InverseTransformPoint(fieldCenter);
                var centerLocalXZ = new Vector2(centerLocal.x, centerLocal.z);
                var boundsCenterXZ = new Vector2(bounds.center.x, bounds.center.z);

                if (Vector2.Distance(centerLocalXZ, boundsCenterXZ) < BrickBuildingUtility.LU_5 && Mathf.Abs(centerLocal.y - bounds.center.y) < BrickBuildingUtility.LU_10)
                {
                    field.insideDetail = insideDetail;
                    insideDetail.field = field;
                }
            }
        }

        private static Bounds ComputeBounds(Transform root)
        {
            var meshRenderers = root.GetComponentsInChildren<MeshRenderer>();
            if (meshRenderers.Length > 0)
            {
                var bounds = meshRenderers[0].bounds;
                foreach (var renderer in meshRenderers)
                {
                    bounds.Encapsulate(renderer.bounds);
                }                
                return bounds;
            }
            return new Bounds(root.position, Vector3.zero);
        }
        
        /// <summary>
        /// Applying materials to imported objects.
        /// Ignores shader id of material.
        /// </summary>
        /// <param name="part"></param>
        /// <param name="materials"></param>
        /// <param name="isLegacy"></param>
        public static void SetMaterials(Part part, LXFMLDoc.Brick.Part.Material[] materials, bool isLegacy)
        {
            if (materials.Length > 0)
            {
                if (isLegacy)
                {
                    var mr = part.GetComponent<MeshRenderer>();
                    mr.sharedMaterial = GetMaterial(materials[0].colorId);
                }
                else
                {
                    if (part.transform.childCount > 0)
                    {
                        var colourChangeSurfaces = part.transform.Find("ColourChangeSurfaces");

                        // Assign materials to shell, knobs, tubes and colour change surfaces
                        for (var i = 0; i < materials.Length; ++i)
                        {
                            if (i == 0)
                            {
                                // Shell.
                                var shell = part.transform.Find("Shell");
                                if (shell)
                                {
                                    var mr = shell.GetComponent<MeshRenderer>();
                                    mr.sharedMaterial = GetMaterial(materials[i].colorId);
                                }
                                else
                                {
                                    Debug.LogError("Missing shell submesh on item " + part.name);
                                }

                                // Detail.
                                var details = part.transform.Find("Detail");
                                if (details)
                                {
                                    foreach(Transform detail in details)
                                    {
                                        var mr = detail.GetComponent<MeshRenderer>();
                                        mr.sharedMaterial = GetMaterial(materials[i].colorId);
                                    }
                                }

                                // Knobs.
                                foreach (var knob in part.knobs)
                                {
                                    var mr = knob.GetComponent<MeshRenderer>();
                                    mr.sharedMaterial = GetMaterial(materials[i].colorId);
                                }

                                // Tubes.
                                foreach (var tube in part.tubes)
                                {
                                    var mr = tube.GetComponent<MeshRenderer>();
                                    mr.sharedMaterial = GetMaterial(materials[i].colorId);
                                }
                            }
                            else
                            {
                                // Colour change surfaces.
                                if (colourChangeSurfaces)
                                {
                                    var surface = colourChangeSurfaces.GetChild(i - 1);
                                    if (surface)
                                    {
                                        var mr = surface.GetComponent<MeshRenderer>();
                                        mr.sharedMaterial = GetMaterial(materials[i].colorId);
                                    }
                                    else
                                    {
                                        Debug.LogError("Missing colour change surface " + (i - 1) + " on item " + part.name);
                                    }
                                }
                                else
                                {
                                    Debug.LogError("Missing colour change surface group on multi material item " + part.name);
                                }
                            }
                        }

                        // Check if all colour change surfaces have been assigned a material.
                        if (colourChangeSurfaces)
                        {
                            if (materials.Length - 1 < colourChangeSurfaces.childCount)
                            {
                                Debug.LogError("Missing material for colour change surface(s) on item " + part.name);

                                for (var i = materials.Length - 1; i < colourChangeSurfaces.childCount; ++i)
                                {
                                    var surface = colourChangeSurfaces.GetChild(i);
                                    if (surface)
                                    {
                                        var mr = surface.GetComponent<MeshRenderer>();
                                        mr.sharedMaterial = GetMaterial(materials[materials.Length - 1].colorId);
                                    }
                                    else
                                    {
                                        Debug.LogError("Missing colour change surface " + i + " on item " + part.name);
                                    }
                                }
                            }
                        }
                    }
                }

            }
        }

        private static Material GetMaterial(int colourId)
        {
            var materialExistence = MaterialUtility.CheckIfMaterialExists(colourId);

            if (materialExistence == MaterialUtility.MaterialExistence.Legacy)
            {
                Debug.LogWarning("Legacy material " + colourId);
            } else if(materialExistence == MaterialUtility.MaterialExistence.None)
            {
                Debug.LogError("Missing material " + colourId);
            }

            if (materialExistence != MaterialUtility.MaterialExistence.None)
            {
                return MaterialUtility.LoadMaterial(colourId, materialExistence == MaterialUtility.MaterialExistence.Legacy);
            }

            return null;
        }

        private static void SetStaticAndGIParams(GameObject go, bool isStatic, bool lightmapped, bool recursive = false)
        {
            if (isStatic)
            {
                go.isStatic = true;

                var mr = go.GetComponent<MeshRenderer>();
                if (mr)
                {
                    if (lightmapped)
                    {
                        mr.receiveGI = ReceiveGI.Lightmaps;
                    }
                    else
                    {
                        mr.receiveGI = ReceiveGI.LightProbes;
                    }
                }

                if (recursive)
                {
                    foreach (Transform child in go.transform)
                    {
                        SetStaticAndGIParams(child.gameObject, isStatic, lightmapped, recursive);
                    }
                }
            }
        }

        private static void InstantiateKnobsAndTubes(Part part, bool lightmapped, int lod)
        {
            var knobs = part.transform.Find("Knobs_loc");
            if (knobs)
            {
                InstantiateCommonParts<Knob>(part, part.knobs, knobs, lightmapped, lod);
                knobs.name = "Knobs";
            }

            var tubes = part.transform.Find("Tubes_loc");
            if (tubes)
            {
                InstantiateCommonParts<Tube>(part, part.tubes, tubes, lightmapped, lod);
                tubes.name = "Tubes";
            }
        }

        public static void InstantiateCommonParts<T>(Part part, List<T> partsList, Transform parent, bool lightmapped, int lod) where T : CommonPart
        {
            int count = parent.childCount;
            // Instantiate common parts using locators.
            for (int i = 0; i < count; i++)
            {
                var commonPartLocation = parent.GetChild(i);
                var name = Regex.Split(commonPartLocation.name, "(_[0-9]+ 1)");

                GameObject commonPartToInstantiate = null;

                var commonPartAvailable = PartUtility.UnpackCommonPart(name[0], lightmapped, lod);
                if (commonPartAvailable)
                {
                    commonPartToInstantiate = PartUtility.LoadCommonPart(name[0], lightmapped, lod);
                }

                if (commonPartToInstantiate == null)
                {
                    Debug.LogError("Missing Common Part -> " + name[0]);
                    continue;
                }

                var commonPartGO = Object.Instantiate(commonPartToInstantiate);
                commonPartGO.name = commonPartToInstantiate.name;

                var commonPartComponent = commonPartGO.AddComponent<T>();
                commonPartComponent.part = part;

                // Set position and rotation.
                commonPartGO.transform.position = commonPartLocation.position;
                commonPartGO.transform.rotation = commonPartLocation.rotation;
                
                commonPartGO.transform.SetParent(parent, true);

                partsList.Add(commonPartComponent);
            }
            // Remove locators.
            for (int i = 0; i < count; i++)
            {
                Object.DestroyImmediate(parent.GetChild(0).gameObject);
            }
        }

        /// <summary>
        /// For setting decorations on imported objects. Not modified.
        /// </summary>
        /// <param name="part"></param>
        /// <param name="decorations"></param>
        /// <param name="isLegacy"></param>
        public static void SetDecorations(Part part, LXFMLDoc.Brick.Part.Decoration[] decorations, bool isLegacy)
        {
            if (isLegacy)
            {
            }
            else
            {
                // Disable decoration surfaces.
                var decorationSurfaces = part.transform.Find("DecorationSurfaces");
                if (decorationSurfaces)
                {
                    decorationSurfaces.gameObject.SetActive(false);
                }
            }
            /*
            for (var i = 0; i < obj.transform.childCount; ++i)
            {
                var t = obj.transform.GetChild(i);

                if (t.gameObject.name.StartsWith("Decoration_"))
                {
                    if (decorations != null && i < decorations.Length && decorations[i] != 0)
                    {
                        if (!mats.ContainsKey(decorations[i]))
                        {
                            var t2d = Util.LoadObjectFromResources<Texture2D>("Decorations/" + decorations[i]);
                            if (t2d != null)
                            {
                                // Generate new material for our prefabs
                                t2d.wrapMode = TextureWrapMode.Clamp;
                                t2d.anisoLevel = 4;
                                var newDecoMat = new Material(decoCutoutMaterial);
                                newDecoMat.SetTexture("_MainTex", t2d);
                                AssetDatabase.CreateAsset(newDecoMat,
                                    decorationMaterialsPath + "/" + decorations[i] + ".mat");
                                mats.Add(decorations[i], newDecoMat);
                                t.gameObject.GetComponent<Renderer>().sharedMaterial = mats[decorations[i]];
                            }
                            else
                            {
                                Debug.Log("Missing decoration -> " + decorations[i]);
                            }
                        }
                        else
                        {
                            t.gameObject.GetComponent<Renderer>().sharedMaterial = mats[decorations[i]];
                        }
                    }
                    else
                    {
                        Object.DestroyImmediate(t.gameObject);
                    }
                }
            }
            */
        }
    }
}
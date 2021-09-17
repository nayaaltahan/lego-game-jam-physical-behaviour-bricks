using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using ICSharpCode.SharpZipLib.Zip;
using System.IO;
using System.Text.RegularExpressions;
using System.Xml;
using System;
using LEGOMaterials;

namespace LEGOMinifig
{

    public static class MinifigUtility
    {
        public static readonly string hatsPath = "Packages/com.lego.minifig/Data/HatsAndHair/Hats";
        public static readonly string hairPath = "Packages/com.lego.minifig/Data/HatsAndHair/Hair";

        public static readonly string hatOrHairMaterialPath = "Assets/LEGO/Data/Materials/Minifig/HatsAndHair";
        public static readonly string bodyMaterialPath = "Assets/LEGO/Data/Materials/Minifig/Bodies";

        public static readonly string minifigSourceMaterialsPath = "Packages/com.lego.minifig/Materials";

        public static readonly string faceZipPath = "Packages/com.lego.minifig/Data/Faces.zip";
        public static readonly string faceTexturePath = "Assets/LEGO/Data/Textures/Minifig/Faces";
        public static readonly string faceMaterialPath = "Assets/LEGO/Data/Materials/Minifig/Faces";
        static ZipFile faceZipFile;

        public static readonly string bodyZipPath = "Packages/com.lego.minifig/Data/Bodies.zip";
        public static readonly string bodyTexturePath = "Assets/LEGO/Data/Textures/Minifig/Bodies";
        static ZipFile bodyZipFile;

        public static readonly string prefabPath = "Packages/com.lego.minifig/Prefabs/Minifig.prefab";

        public static readonly Rect leftFootRect = new Rect(378f / 667f, 617f / 742f, 264f / 667f, 105f / 742f);
        public static readonly Rect rightFootRect = new Rect(25f / 667f, 617f / 742f, 264f / 667f, 105f / 742f);

        public static readonly Rect leftLegRect = new Rect(370f / 667f, 91f / 742f, 272f / 667f, 410f / 742f);
        public static readonly Rect rightLegRect = new Rect(25f / 667f, 91f / 742f, 272f / 667f, 410f / 742f);

        public static readonly Rect hipRect = new Rect(38f / 667f, 8f / 742f, 590f / 667f, 70f / 742f);
        public static readonly Rect crotchRect = new Rect(297f / 667f, 91f / 742f, 73f / 667f, 290f / 742f);

        public class AccessoryInfo
        {
            public string name;
            public Texture2D texture;
            public int numColours;
        }

        public class FaceInfo
        {
            public string name;
            public Texture2D texture;
            public List<MinifigFaceAnimationController.FaceAnimation> animations = new List<MinifigFaceAnimationController.FaceAnimation>();
        }

        public class BodyInfo
        {
            public string name;
            public Color torsoColour;
            public Color armColour;
            public Color handColour;
            public string frontTextureName;
            public string backTextureName;
            public Texture2D texture;
        }

        public static void RefreshDB()
        {
            if (faceZipFile != null)
            {
                faceZipFile.Close();
                faceZipFile = null;
            }

            if (bodyZipFile != null)
            {
                bodyZipFile.Close();
                bodyZipFile = null;
            }

            OpenDB();
        }

        static void OpenDB()
        {
            if (faceZipFile == null)
            {
                faceZipFile = new ZipFile(faceZipPath);
            }

            if (bodyZipFile == null)
            {
                bodyZipFile = new ZipFile(bodyZipPath);
            }
        }

        public static List<AccessoryInfo> ListHats(Color[] colours)
        {
            var hatMeshFilenames = Directory.GetFiles(hatsPath, "*.fbx");

            return RenderPreviews(hatMeshFilenames, colours);
        }

        public static List<AccessoryInfo> ListHair(Color[] colours)
        {
            var hairMeshFilenames = Directory.GetFiles(hairPath, "*.fbx");

            return RenderPreviews(hairMeshFilenames, colours);
        }

        public static GameObject LoadHatFBX(string name)
        {
#if UNITY_EDITOR
            return AssetDatabase.LoadAssetAtPath<GameObject>(Path.Combine(hatsPath, name + ".fbx"));
#else
        return null;
#endif
        }

        public static Texture2D LoadHatNormalMap(string name)
        {
#if UNITY_EDITOR
            return AssetDatabase.LoadAssetAtPath<Texture2D>(Path.Combine(hatsPath, "NormalMaps", name + "_N.png"));
#else
        return null;
#endif
        }

        public static GameObject LoadHairFBX(string name)
        {
#if UNITY_EDITOR
            return AssetDatabase.LoadAssetAtPath<GameObject>(Path.Combine(hairPath, name + ".fbx"));
#else
        return null;
#endif
        }

        public static Texture2D LoadHairNormalMap(string name)
        {
#if UNITY_EDITOR
            return AssetDatabase.LoadAssetAtPath<Texture2D>(Path.Combine(hairPath, "NormalMaps", name + "_N.png"));
#else
        return null;
#endif
        }

        public static Material GetHatOrHairMaterial(string name, Color colour, Texture2D normalMap)
        {
#if UNITY_EDITOR
            var materialPath = Path.Combine(hatOrHairMaterialPath, name + "_" + MouldingColour.GetId(colour) + (normalMap ? "_N" : "") + ".mat");
            var material = AssetDatabase.LoadAssetAtPath<Material>(materialPath);
            if (!material)
            {
                material = new Material(Shader.Find("Universal Render Pipeline/Lit"));
                material.SetColor("_BaseColor", colour);
                material.SetFloat("_Smoothness", 0.75f);

                if (colour.a < 1.0f)
                {
                    material.SetFloat("_Surface", 1);
                    material.SetOverrideTag("RenderType", "Transparent");
                    material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.One);
                    material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                    material.SetInt("_ZWrite", 0);
                    material.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Transparent;
                    material.SetShaderPassEnabled("ShadowCaster", false);
                }

                if (normalMap)
                {
                    material.EnableKeyword("_NORMALMAP");
                    material.SetTexture("_BumpMap", normalMap);
                }

                var directoryName = Path.GetDirectoryName(materialPath);
                if (directoryName.Length > 0)
                {
                    Directory.CreateDirectory(directoryName);
                }

                AssetDatabase.CreateAsset(material, materialPath);

                material = AssetDatabase.LoadAssetAtPath<Material>(materialPath);
            }
            return material;
#else
        return null;
#endif
        }

        static List<AccessoryInfo> RenderPreviews(string[] meshFilenames, Color[] colours)
        {
            List<AccessoryInfo> result = new List<AccessoryInfo>();

            var renderer = new PreviewRenderUtility();

            renderer.camera.transform.position = Vector3.one * 5.0f;
            renderer.camera.transform.LookAt(Vector3.zero, Vector3.up);
            renderer.camera.farClipPlane = 10;
            renderer.camera.backgroundColor = new Color32(0, 0, 0, 0);
            renderer.camera.clearFlags = CameraClearFlags.Color | CameraClearFlags.Depth;

            renderer.lights[0].intensity = 0.6f;
            renderer.lights[0].color = Color.white;
            renderer.lights[0].transform.rotation = Quaternion.LookRotation(Vector3.back, Vector3.up);// Quaternion.Euler(30f, 30f, 0f);
            renderer.lights[1].intensity = 0.4f;
            renderer.lights[1].color = Color.white;
            renderer.lights[1].transform.rotation = Quaternion.LookRotation(Vector3.down, Vector3.back);// Quaternion.Euler(0f, -30f, 30f);

            foreach (var meshFilename in meshFilenames)
            {
                var meshDirectory = Path.GetDirectoryName(meshFilename);
                var meshName = Path.GetFileNameWithoutExtension(meshFilename);
                var assets = AssetDatabase.LoadAllAssetsAtPath(meshFilename);

                var normalMapPath = Path.Combine(meshDirectory, "NormalMaps", $"{meshName}_N.png");
                var normalMap = AssetDatabase.LoadAssetAtPath<Texture2D>(normalMapPath);

                renderer.BeginPreview(new Rect(0.0f, 0.0f, 400.0f, 400.0f), GUIStyle.none);

                var numColours = 0;
                foreach(var asset in assets)
                {
                    if (asset.GetType() == typeof(Mesh))
                    {
                        var mesh = (Mesh)asset;

                        // If a decoration surface, skip the mesh.
                        if (mesh.name.StartsWith("VME_"))
                        {
                            continue;
                        }

                        // Choose first colour for shell and legacy mesh.
                        var colour = colours[0];
                        if (mesh.name.StartsWith("Exp_"))
                        {
                            // Colour change surface, so find the right colour.
                            var colourIndex = int.Parse(mesh.name.Substring(mesh.name.Length - 1));
                            if (colourIndex < colours.Length)
                            {
                                colour = colours[colourIndex];
                            }
                        }

                        // Setup material.
                        var material = new Material(Shader.Find("Universal Render Pipeline/Lit"));
                        material.SetColor("_BaseColor", colour);
                        material.SetFloat("_Smoothness", 0.75f);

                        if (normalMap)
                        {
                            material.EnableKeyword("_NORMALMAP");
                            material.SetTexture("_BumpMap", normalMap);
                        }
                        else
                        {
                            material.DisableKeyword("_NORMALMAP");
                        }

                        if (colour.a < 1.0f)
                        {
                            material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.One);
                            material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                            material.SetInt("_ZWrite", 0);
                            material.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Transparent;
                        }

                        numColours++;

                        renderer.DrawMesh(mesh, Matrix4x4.identity, material, 0);
                    }
                }

                renderer.camera.Render();

                var preview = renderer.EndPreview();

                // FIXME Uses experimental API.
                Texture2D tex = new Texture2D(preview.width, preview.height, preview.graphicsFormat, UnityEngine.Experimental.Rendering.TextureCreationFlags.None);
                Graphics.CopyTexture(preview, tex);

                var info = new AccessoryInfo();
                info.name = meshName;
                info.texture = tex;
                info.numColours = numColours;

                result.Add(info);
            }

            renderer.Cleanup();

            return result;
        }

        public static List<FaceInfo> ListFaces()
        {
            OpenDB();

            var entryEnumerator = faceZipFile.GetEnumerator();

            Dictionary<string, FaceInfo> foundFaces = new Dictionary<string, FaceInfo>();

            while (entryEnumerator.MoveNext())
            {
                var entry = (ZipEntry)entryEnumerator.Current;
                if (entry.IsFile && entry.CanDecompress)
                {
                    var faceId = Path.GetDirectoryName(entry.Name);

                    if (!foundFaces.ContainsKey(faceId))
                    {
                        var info = new FaceInfo();
                        info.name = faceId;
                        foundFaces.Add(faceId, info);
                    }

                    var frameName = Path.GetFileNameWithoutExtension(entry.Name);
                    var animationName = Regex.Split(frameName, "([_\\.]+)")[0];

                    // Base texture.
                    if (animationName == "base")
                    {
                        var zipStream = faceZipFile.GetInputStream(entry);
                        var memoryStream = new MemoryStream();
                        zipStream.CopyTo(memoryStream);

                        Texture2D tex = new Texture2D(1, 1);
                        tex.LoadImage(memoryStream.ToArray());

                        foundFaces[faceId].texture = tex;

                        continue;
                    }

                    // Animation frame texture.
                    try
                    {
                        var faceAnimation = (MinifigFaceAnimationController.FaceAnimation)Enum.Parse(typeof(MinifigFaceAnimationController.FaceAnimation), animationName, true);

                        if (!foundFaces[faceId].animations.Contains(faceAnimation))
                        {
                            foundFaces[faceId].animations.Add(faceAnimation);
                        }
                    }
                    catch
                    {
                        Debug.LogErrorFormat("Unknown animation name found {0} in {1}", animationName, entry.Name);
                    }
                }
            }

            foreach (var foundFace in foundFaces.Values)
            {
                foundFace.animations.Sort();
            }

            return new List<FaceInfo>(foundFaces.Values);
        }

        public static bool CheckIfFaceIsUnpacked(FaceInfo face)
        {
            var facePath = Path.Combine(MinifigUtility.faceTexturePath, face.name);
            var directoryExists = Directory.Exists(facePath);
            if (!directoryExists)
            {
                return false;
            }

            // Check base.
            if (!File.Exists(Path.Combine(facePath, "base.png")))
            {
                return false;
            }

            // Check all given face animations.
            foreach (var animation in face.animations)
            {
                var animationExists = Directory.GetFiles(facePath, animation.ToString().ToLower() + "*").Length > 0;
                if (!animationExists)
                {
                    return false;
                }
            }

            return true;
        }

        public static bool UnpackFace(FaceInfo face)
        {
            OpenDB();

            var facePath = Path.Combine(MinifigUtility.faceTexturePath, face.name);

            var baseFaceUnpacked = File.Exists(Path.Combine(facePath, "base.png"));
            var baseFaceFound = false;
            var animationAlreadyUnpacked = new Dictionary<MinifigFaceAnimationController.FaceAnimation, bool>();
            var animationFound = new Dictionary<MinifigFaceAnimationController.FaceAnimation, bool>();
            foreach (var animation in face.animations)
            {
                animationAlreadyUnpacked.Add(animation, Directory.Exists(facePath) && Directory.GetFiles(facePath, animation.ToString().ToLower() + "*").Length > 0);
                animationFound.Add(animation, false);
            }

            var entryEnumerator = faceZipFile.GetEnumerator();

            while (entryEnumerator.MoveNext())
            {
                var entry = (ZipEntry)entryEnumerator.Current;
                if (entry.IsFile && entry.CanDecompress)
                {
                    var faceId = Path.GetDirectoryName(entry.Name);

                    if (faceId == face.name)
                    {
                        var fileName = Path.GetFileName(entry.Name);
                        var frameName = Path.GetFileNameWithoutExtension(entry.Name);
                        var animationName = Regex.Split(frameName, "([_\\.]+)")[0];

                        // Base texture.
                        if (animationName == "base")
                        {
                            if (!baseFaceUnpacked)
                            {
                                Directory.CreateDirectory(facePath);

                                var filePath = Path.Combine(facePath, "base.png");
                                var zipStream = faceZipFile.GetInputStream(entry);
                                var fileStream = File.Create(filePath);
                                zipStream.CopyTo(fileStream);
                                fileStream.Dispose();

                                baseFaceUnpacked = true;
                            }

                            baseFaceFound = true;

                            continue;
                        }

                        // Animation frame texture.
                        try
                        {
                            var faceAnimation = (MinifigFaceAnimationController.FaceAnimation)Enum.Parse(typeof(MinifigFaceAnimationController.FaceAnimation), animationName, true);

                            if (face.animations.Contains(faceAnimation))
                            {
                                if (!animationAlreadyUnpacked[faceAnimation])
                                {
                                    Directory.CreateDirectory(facePath);

                                    var filePath = Path.Combine(facePath, fileName);
                                    var zipStream = faceZipFile.GetInputStream(entry);
                                    var fileStream = File.Create(filePath);
                                    zipStream.CopyTo(fileStream);
                                    fileStream.Dispose();
                                }
                                animationFound[faceAnimation] = true;
                            }
                        }
                        catch
                        {
                            Debug.LogErrorFormat("Unknown animation name found {0} in {1}", animationName, entry.Name);
                        }
                    }
                }
            }
#if UNITY_EDITOR
            AssetDatabase.Refresh();
#endif
            var result = baseFaceFound;
            foreach (var found in animationFound)
            {
                result &= found.Value;
            }

            return result;
        }

        public static Texture2D LoadFaceTexture(string name)
        {
#if UNITY_EDITOR
            return AssetDatabase.LoadAssetAtPath<Texture2D>(Path.Combine(faceTexturePath, name, "base.png"));
#else
        return null;
#endif

        }

        static Texture2D[] LoadFaceAnimationTextures(string name, MinifigFaceAnimationController.FaceAnimation animation)
        {
#if UNITY_EDITOR
            var fileNames = Directory.GetFiles(Path.Combine(faceTexturePath, name), animation.ToString().ToLower() + "*.png");
            var fileNumbers = new int[fileNames.Length];
            var result = new Texture2D[fileNames.Length];
            for (var i = 0; i < fileNames.Length; ++i)
            {
                result[i] = AssetDatabase.LoadAssetAtPath<Texture2D>(fileNames[i]);
                fileNumbers[i] = int.Parse(Regex.Match(fileNames[i], ".*[\\.](?<number>[0-9]+)\\.png").Groups["number"].Value);
            }

            Array.Sort(fileNumbers, result);

            return result;
#else
        return null;
#endif

        }

        public static Material GetFaceMaterial(string name, Texture2D texture)
        {
#if UNITY_EDITOR
            var materialPath = Path.Combine(faceMaterialPath, name + ".mat");
            var material = AssetDatabase.LoadAssetAtPath<Material>(materialPath);
            if (!material)
            {
                // Create a new material from the minifig source material.
                material = new Material(AssetDatabase.LoadAssetAtPath<Material>(Path.Combine(minifigSourceMaterialsPath, "Face_Anim.mat")));

                material.SetTexture("_BaseMap", texture);

                var directoryName = Path.GetDirectoryName(materialPath);
                if (directoryName.Length > 0)
                {
                    Directory.CreateDirectory(directoryName);
                }

                AssetDatabase.CreateAsset(material, materialPath);

                material = AssetDatabase.LoadAssetAtPath<Material>(materialPath);
            }
            return material;
#else
        return null;
#endif
        }

        public static void AddFaceAnimationController(Minifig minifig, FaceInfo faceInfo)
        {
            var texture = LoadFaceTexture(faceInfo.name);

            var faceAnimationController = minifig.gameObject.AddComponent<MinifigFaceAnimationController>();
            faceAnimationController.Init(minifig.GetFace(), texture);

            foreach (var animation in faceInfo.animations)
            {
                var animationTextures = LoadFaceAnimationTextures(faceInfo.name, animation);
                faceAnimationController.AddAnimation(animation, animationTextures);
            }
        }

        public static List<BodyInfo> ListBodies()
        {
            OpenDB();

            List<BodyInfo> result = new List<BodyInfo>();

            var bodiesEntry = bodyZipFile.GetEntry("bodies.xml");
            if (bodiesEntry.IsFile && bodiesEntry.CanDecompress)
            {
                XmlDocument bodiesDoc = new XmlDocument();
                bodiesDoc.Load(bodyZipFile.GetInputStream(bodiesEntry));

                var bodiesRoot = bodiesDoc.DocumentElement;
                var bodyNodes = bodiesRoot.SelectNodes("Body");
                foreach (XmlNode bodyNode in bodyNodes)
                {
                    var bodyInfo = new BodyInfo();
                    bodyInfo.name = bodyNode.Attributes["name"].Value;
                    bodyInfo.torsoColour = MouldingColour.GetColour(bodyNode.Attributes["torsoColor"].Value);
                    bodyInfo.armColour = MouldingColour.GetColour(bodyNode.Attributes["armColor"].Value);
                    bodyInfo.handColour = MouldingColour.GetColour(bodyNode.Attributes["handColor"].Value);

                    bodyInfo.frontTextureName = bodyNode.Attributes["frontTexture"].Value;
                    bodyInfo.backTextureName = bodyNode.Attributes["backTexture"].Value;

                    if (!string.IsNullOrEmpty(bodyInfo.frontTextureName))
                    {
                        var frontEntry = bodyZipFile.GetEntry(bodyInfo.frontTextureName + ".png");
                        var zipStream = bodyZipFile.GetInputStream(frontEntry);
                        var memoryStream = new MemoryStream();
                        zipStream.CopyTo(memoryStream);

                        Texture2D tex = new Texture2D(1, 1);
                        tex.LoadImage(memoryStream.ToArray());

                        bodyInfo.texture = tex;
                    }
                    else if (!string.IsNullOrEmpty(bodyInfo.backTextureName))
                    {
                        var backEntry = bodyZipFile.GetEntry(bodyInfo.backTextureName + ".png");
                        var zipStream = bodyZipFile.GetInputStream(backEntry);
                        var memoryStream = new MemoryStream();
                        zipStream.CopyTo(memoryStream);

                        Texture2D tex = new Texture2D(1, 1);
                        tex.LoadImage(memoryStream.ToArray());

                        bodyInfo.texture = tex;
                    }

                    result.Add(bodyInfo);
                }
            }

            return result;
        }

        public static bool UnpackBody(string name)
        {
            if (File.Exists(Path.Combine(bodyTexturePath, name + ".png")))
            {
                return true;
            }

            OpenDB();

            int index = bodyZipFile.FindEntry(name + ".png", true);
            if (index >= 0)
            {
                var entry = bodyZipFile.GetEntry(name + ".png");
                if (entry.IsFile && entry.CanDecompress)
                {
                    var fileName = entry.Name;
                    var filePath = Path.Combine(bodyTexturePath, fileName);

                    var directoryName = Path.GetDirectoryName(filePath);
                    if (directoryName.Length > 0)
                    {
                        Directory.CreateDirectory(directoryName);
                    }

                    var zipStream = bodyZipFile.GetInputStream(entry);
                    var fileStream = File.Create(filePath);
                    zipStream.CopyTo(fileStream);
                    fileStream.Dispose();
#if UNITY_EDITOR
                    AssetDatabase.ImportAsset(filePath);
#endif
                    return true;
                }
            }

            return false;
        }

        public static Texture2D LoadBodyTexture(string name)
        {
#if UNITY_EDITOR
            return AssetDatabase.LoadAssetAtPath<Texture2D>(Path.Combine(bodyTexturePath, name + ".png"));
#else
        return null;
#endif

        }

        public static List<Material> GetTorsoMaterials(Color colour, Texture2D frontTexture = null, Texture2D backTexture = null)
        {
            var result = new List<Material>();

            result.Add(GetPartMaterial("Torso", colour));
            result.Add(GetDecorationMaterial("Torso_Front_Decoration", frontTexture));
            result.Add(GetDecorationMaterial("Torso_Back_Decoration", backTexture));

            return result;
        }

        public static Material GetArmMaterial(Color colour)
        {
            return GetPartMaterial("Arm", colour);
        }

        public static Material GetHandMaterial(Color colour)
        {
            return GetPartMaterial("Hand", colour);
        }

        public static List<Material> GetLegMaterials(Color colour, Texture2D hipTexture = null, Texture2D leftLegFrontTexture = null, Texture2D leftLegSideTexture = null, Texture2D rightLegFrontTexture = null, Texture2D rightLegSideTexture = null)
        {
            var result = new List<Material>();

            result.Add(GetPartMaterial("Hip", colour));
            result.Add(GetDecorationMaterial("Hip_Decoration", hipTexture));
            result.Add(GetPartMaterial("Leg_Left", colour));
            result.Add(GetDecorationMaterial("Leg_Left_Front_Decoration", leftLegFrontTexture));
            result.Add(GetDecorationMaterial("Leg_Left_Side_Decoration", leftLegSideTexture));
            result.Add(GetPartMaterial("Leg_Right", colour));
            result.Add(GetDecorationMaterial("Leg_Right_Front_Decoration", rightLegFrontTexture));
            result.Add(GetDecorationMaterial("Leg_Right_Side_Decoration", rightLegSideTexture));

            return result;
        }

        static Material GetPartMaterial(string name, Color colour)
        {
#if UNITY_EDITOR
            var materialPath = Path.Combine(bodyMaterialPath, name + "_" + MouldingColour.GetId(colour) + ".mat");
            var material = AssetDatabase.LoadAssetAtPath<Material>(materialPath);
            if (!material)
            {
                // Create a new material from the minifig source material.
                material = new Material(AssetDatabase.LoadAssetAtPath<Material>(Path.Combine(minifigSourceMaterialsPath, name + ".mat")));

                if (colour.a < 1.0f)
                {
                    material.SetFloat("_Surface", 1);
                    material.SetOverrideTag("RenderType", "Transparent");
                    material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.One);
                    material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                    material.SetInt("_ZWrite", 0);
                    material.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Transparent;
                    material.SetShaderPassEnabled("ShadowCaster", false);
                }

                material.SetColor("_BaseColor", colour);
                material.SetTexture("_BaseMap", null);

                var directoryName = Path.GetDirectoryName(materialPath);
                if (directoryName.Length > 0)
                {
                    Directory.CreateDirectory(directoryName);
                }

                AssetDatabase.CreateAsset(material, materialPath);

                material = AssetDatabase.LoadAssetAtPath<Material>(materialPath);
            }
            return material;
#else
        return null;
#endif
        }

        static Material GetDecorationMaterial(string name, Texture2D texture = null)
        {
            if (!texture)
            {
                return null;
            }

#if UNITY_EDITOR
            var materialPath = Path.Combine(bodyMaterialPath, name + "_" + texture.name + ".mat");
            var material = AssetDatabase.LoadAssetAtPath<Material>(materialPath);
            if (!material)
            {
                // Create a new material from the minifig source material.
                material = new Material(AssetDatabase.LoadAssetAtPath<Material>(Path.Combine(minifigSourceMaterialsPath, name + ".mat")));

                material.SetColor("_BaseColor", Color.white);
                material.SetTexture("_BaseMap", texture);

                var directoryName = Path.GetDirectoryName(materialPath);
                if (directoryName.Length > 0)
                {
                    Directory.CreateDirectory(directoryName);
                }

                AssetDatabase.CreateAsset(material, materialPath);

                material = AssetDatabase.LoadAssetAtPath<Material>(materialPath);
            }
            return material;
#else
        return null;
#endif
        }

        public static GameObject LoadMinifigPrefab()
        {
#if UNITY_EDITOR
            return AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
#else
        return null;
#endif
        }

        public static Texture2D CreateLegFrontDecoration(Texture2D leftFoot, Texture2D rightFoot, Texture2D leftLeg, Texture2D rightLeg, Texture2D hip, Texture2D crotch, Vector2Int resolution)
        {
            var renderTexture = RenderTexture.GetTemporary(resolution.x, resolution.y, 0, RenderTextureFormat.ARGB32, RenderTextureReadWrite.sRGB);

            var previousActiveRenderTexture = RenderTexture.active;
            RenderTexture.active = renderTexture;

            GL.Clear(true, true, Color.clear);

            GL.PushMatrix();
            GL.LoadPixelMatrix(0, resolution.x, resolution.y, 0);

            // Copy over textures into result.
            DrawLegFrontDecorationPart(leftFoot, leftFootRect, resolution);
            DrawLegFrontDecorationPart(rightFoot, rightFootRect, resolution);
            DrawLegFrontDecorationPart(leftLeg, leftLegRect, resolution);
            DrawLegFrontDecorationPart(rightLeg, rightLegRect, resolution);
            DrawLegFrontDecorationPart(hip, hipRect, resolution);
            DrawLegFrontDecorationPart(crotch, crotchRect, resolution);

            GL.PopMatrix();

            var result = new Texture2D(resolution.x, resolution.y, TextureFormat.RGBA32, true);

            result.ReadPixels(new Rect(0, 0, renderTexture.width, renderTexture.height), 0, 0, true);
            result.Apply();

            RenderTexture.active = previousActiveRenderTexture;
            RenderTexture.ReleaseTemporary(renderTexture);

            return result;
        }

        static void DrawLegFrontDecorationPart(Texture2D texture, Rect rect, Vector2Int resolution)
        {
            if (texture)
            {
                Graphics.DrawTexture(new Rect(rect.position * resolution, rect.size * resolution), texture);
            }
        }
    }

}
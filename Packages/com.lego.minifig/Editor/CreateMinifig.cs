// Copyright (C) LEGO System A/S - All Rights Reserved
// Unauthorized copying of this file, via any medium is strictly prohibited

using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using LEGOMaterials;

namespace LEGOMinifig
{

    public class CreateMinifig : EditorWindow
    {
        static List<MinifigUtility.AccessoryInfo> foundHats;
        static Texture2D[] hatOrHairTextures;
        static List<MinifigUtility.AccessoryInfo> foundHair;
        static List<MinifigUtility.FaceInfo> foundFaces;
        static Texture2D[] faceTextures;
        static List<MinifigUtility.BodyInfo> foundBodies;
        static Texture2D[] bodyTextures;
        static Vector2 scrollPosition;

        int hatOrHair = 0;
        [SerializeField, MouldingColour(true)]
        Color[] hatOrHairColours;
        Color[] previousHatOrHairColours;
        int face = 0;
        int previousFace = -1;
        bool[] animations;
        int body = 0;
        [SerializeField, MouldingColour(true)]
        Color torsoColour;
        [SerializeField, MouldingColour(true)]
        Color armColour;
        [SerializeField, MouldingColour(true)]
        Color handColour;
        [SerializeField, MouldingColour(true, true)]
        Color legColour;

        SerializedObject serializedObject;
        SerializedProperty hatOrHairColoursProp;
        SerializedProperty torsoColourProp;
        SerializedProperty armColourProp;
        SerializedProperty handColourProp;
        SerializedProperty legColourProp;

        private void Awake()
        {
            // Assumes that no hats or hair moulds contain more than three colours. 
            hatOrHairColours = new Color[] { MouldingColour.GetColour(MouldingColour.Id.White), MouldingColour.GetColour(MouldingColour.Id.White), MouldingColour.GetColour(MouldingColour.Id.White) };
            previousHatOrHairColours = new Color[] { MouldingColour.GetColour(MouldingColour.Id.White), MouldingColour.GetColour(MouldingColour.Id.White), MouldingColour.GetColour(MouldingColour.Id.White) };
            torsoColour = MouldingColour.GetColour(MouldingColour.Id.White);
            armColour = MouldingColour.GetColour(MouldingColour.Id.White);
            handColour = MouldingColour.GetColour(MouldingColour.Id.BrightYellow);
            legColour = MouldingColour.GetColour(MouldingColour.Id.BrightBlue);
        }

        [MenuItem("LEGO Tools/Create Minifigure &%m", priority = 0)]
        public static void PrepareWindow()
        {
            var window = GetWindow<CreateMinifig>(true, "LEGO Minifigure Creator");

            RefreshHatOrHairPreviews(window.hatOrHairColours);

            if (faceTextures != null)
            {
                foreach (var texture in faceTextures)
                {
                    DestroyImmediate(texture);
                }
            }

            foundFaces = MinifigUtility.ListFaces();
            faceTextures = new Texture2D[foundFaces.Count];
            int i = 0;
            foreach (var foundFace in foundFaces)
            {
                faceTextures[i++] = foundFace.texture;
            }

            if (bodyTextures != null)
            {
                foreach (var texture in bodyTextures)
                {
                    DestroyImmediate(texture);
                }
            }

            foundBodies = MinifigUtility.ListBodies();
            bodyTextures = new Texture2D[foundBodies.Count + 1];
            i = 1;
            foreach (var foundBody in foundBodies)
            {
                bodyTextures[i++] = foundBody.texture;
            }

            // Set up serialized object and properties for use with moulding colour drawer.
            window.serializedObject = new SerializedObject(window);
            window.hatOrHairColoursProp = window.serializedObject.FindProperty("hatOrHairColours");
            window.torsoColourProp = window.serializedObject.FindProperty("torsoColour");
            window.armColourProp = window.serializedObject.FindProperty("armColour");
            window.handColourProp = window.serializedObject.FindProperty("handColour");
            window.legColourProp = window.serializedObject.FindProperty("legColour");
        }

        private static void RefreshHatOrHairPreviews(Color[] colours)
        {
            if (hatOrHairTextures != null)
            {
                foreach (var texture in hatOrHairTextures)
                {
                    DestroyImmediate(texture);
                }
            }

            foundHats = MinifigUtility.ListHats(colours);
            foundHair = MinifigUtility.ListHair(colours);
            hatOrHairTextures = new Texture2D[foundHats.Count + foundHair.Count + 1];
            int i = 1;
            foreach (var foundHat in foundHats)
            {
                hatOrHairTextures[i++] = foundHat.texture;
            }
            foreach (var foundHairpiece in foundHair)
            {
                hatOrHairTextures[i++] = foundHairpiece.texture;
            }
        }

        private void OnGUI()
        {
            serializedObject.Update();

            var previewRefreshNeeded = false;
            for(var i = 0; i < hatOrHairColours.Length; ++i)
            {
                if (hatOrHairColours[i] != previousHatOrHairColours[i])
                {
                    previewRefreshNeeded = true;
                    break;
                }
            }

            if (previewRefreshNeeded)
            {
                RefreshHatOrHairPreviews(hatOrHairColours);

                for(var i = 0; i < hatOrHairColours.Length; ++i)
                {
                    previousHatOrHairColours[i] = hatOrHairColours[i];
                }
            }

            scrollPosition = GUILayout.BeginScrollView(scrollPosition, false, true);

            GUILayout.Label("Hats And Hair");

            var hatOrHairColumnCount = Mathf.FloorToInt((position.width - 19) / (100.0f + 3.0f));
            var hatOrHairStyle = new GUIStyle(GUI.skin.button);
            hatOrHairStyle.fixedHeight = 100.0f;

            hatOrHair = GUILayout.SelectionGrid(hatOrHair, hatOrHairTextures, hatOrHairColumnCount, hatOrHairStyle, new GUILayoutOption[] { GUILayout.Width(position.width - 19) });

            if (hatOrHair > 0)
            {
                MinifigUtility.AccessoryInfo hatOrHairInfo = null;
                if (hatOrHair > foundHats.Count)
                {
                    hatOrHairInfo = foundHair[hatOrHair - foundHats.Count - 1];
                }
                else if (hatOrHair > 0)
                {
                    hatOrHairInfo = foundHats[hatOrHair - 1];
                }
                for (var i = 0; i < hatOrHairInfo.numColours; ++i)
                {
                    EditorGUILayout.PropertyField(hatOrHairColoursProp.GetArrayElementAtIndex(i), new GUIContent((hatOrHair <= foundHats.Count ? "Hat" : "Hair") + " Colour" + (hatOrHairInfo.numColours > 1 ? " " + (i+1) : "")));
                }
            }

            EditorGUILayout.Separator();

            GUILayout.Label("Faces");

            var faceColumnCount = Mathf.FloorToInt((position.width - 19) / (100.0f + 3.0f));
            var faceStyle = new GUIStyle(GUI.skin.button);
            faceStyle.fixedHeight = 70.0f;

            face = GUILayout.SelectionGrid(face, faceTextures, faceColumnCount, faceStyle, new GUILayoutOption[] { GUILayout.Width(position.width - 19) });

            if (face != previousFace)
            {
                var newFaceInfo = foundFaces[face];

                animations = new bool[newFaceInfo.animations.Count];
                for (var i = 0; i < animations.Length; ++i)
                {
                    animations[i] = true;
                }

                previousFace = face;
            }

            // Show available animations.
            var faceInfo = foundFaces[face];
            if (faceInfo.animations.Count > 0)
            {
                GUILayout.Label("Animations");
                for (var i = 0; i < faceInfo.animations.Count; ++i)
                {
                    animations[i] = GUILayout.Toggle(animations[i], ObjectNames.NicifyVariableName(faceInfo.animations[i].ToString()));
                }
            }
            else
            {
                GUILayout.Label("No Animations");
            }

            EditorGUILayout.Separator();

            GUILayout.Label("Bodies");

            var bodyColumnCount = Mathf.FloorToInt((position.width - 19) / (100.0f + 3.0f));
            var bodyStyle = new GUIStyle(GUI.skin.button);
            bodyStyle.fixedHeight = 100.0f;

            body = GUILayout.SelectionGrid(body, bodyTextures, bodyColumnCount, bodyStyle, new GUILayoutOption[] { GUILayout.Width(position.width - 19) });

            if (body <= 0)
            {
                // Show colours.
                EditorGUILayout.PropertyField(torsoColourProp);
                EditorGUILayout.PropertyField(armColourProp);
                EditorGUILayout.PropertyField(handColourProp);
            }

            EditorGUILayout.Separator();

            EditorGUILayout.PropertyField(legColourProp);

            // FIXME Add equipment later.
            /*EditorGUILayout.Separator();

            GUILayout.Label("Equipment");*/

            EditorGUILayout.Separator();

            EditorGUILayout.Separator();

            var createPressed = GUILayout.Button("Create Minifigure");
            if (createPressed)
            {
                MinifigUtility.AccessoryInfo hatOrHairInfo = null;
                if (hatOrHair > foundHats.Count)
                {
                    hatOrHairInfo = foundHair[hatOrHair - foundHats.Count - 1];
                }
                else if (hatOrHair > 0)
                {
                    hatOrHairInfo = foundHats[hatOrHair - 1];
                }

                MinifigUtility.BodyInfo bodyInfo;
                if (body > 0)
                {
                    bodyInfo = foundBodies[body - 1];
                }
                else
                {
                    bodyInfo = new MinifigUtility.BodyInfo()
                    {
                        name = "Custom",
                        torsoColour = torsoColour,
                        armColour = armColour,
                        handColour = handColour
                    };
                }

                MinifigCreator.InstantiateMinifig(hatOrHairInfo, hatOrHairColours, foundFaces[face], animations, bodyInfo, legColour);
            }

            GUILayout.EndScrollView();

            if (createPressed)
            {
                Close();
            }
        }

        private void OnEnable()
        {
            Undo.undoRedoPerformed += Repaint;
        }

        private void OnDisable()
        {
            Undo.undoRedoPerformed -= Repaint;
        }
    }

}
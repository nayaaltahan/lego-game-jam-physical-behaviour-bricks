// Copyright (C) LEGO System A/S - All Rights Reserved
// Unauthorized copying of this file, via any medium is strictly prohibited

using UnityEngine;
using UnityEditor;
using System.IO;

namespace LEGOMinifig
{
    public class CreateLegFrontDecoration : EditorWindow
    {
        static Texture decorationBackgroundTexture;

        readonly Vector2 decorationBackgroundSize = new Vector2(333.5f, 371f);
        const float scale = 0.5f;

        Texture2D leftFootTexture;
        Texture2D rightFootTexture;

        Texture2D leftLegTexture;
        Texture2D rightLegTexture;

        Texture2D hipTexture;
        Texture2D crotchTexture;

        Vector2Int resolution = new Vector2Int(1024, 1024);

        [MenuItem("LEGO Tools/Dev/Create Leg Front Decoration #&%f")]
        static void FindTextures()
        {
            GetWindow<CreateLegFrontDecoration>(true, "LEGO Leg Front Decoration Creator");
        }

        private void OnEnable()
        {
            decorationBackgroundTexture = AssetDatabase.LoadAssetAtPath<Texture>("Packages/com.lego.minifig/Textures/LEGO Minifigure Leg Front Decoration Guide.png");
        }


        private void OnGUI()
        {
            minSize = decorationBackgroundSize + Vector2.up * 62f;
            maxSize = decorationBackgroundSize + Vector2.up * 62f;

            GUI.DrawTexture(new Rect(Vector2.zero, decorationBackgroundSize), decorationBackgroundTexture);

            ShowTextureButton(ref leftFootTexture, MinifigUtility.leftFootRect, 0);
            ShowTextureButton(ref rightFootTexture, MinifigUtility.rightFootRect, 1);
            ShowTextureButton(ref leftLegTexture, MinifigUtility.leftLegRect, 2);
            ShowTextureButton(ref rightLegTexture, MinifigUtility.rightLegRect, 3);
            ShowTextureButton(ref hipTexture, MinifigUtility.hipRect, 4);
            ShowTextureButton(ref crotchTexture, MinifigUtility.crotchRect, 5);

            // Reserve the space for the GUILayout.
            GUILayout.Space(decorationBackgroundSize.y);

            resolution = EditorGUILayout.Vector2IntField("Resolution", resolution);
            resolution = Vector2Int.Max(resolution, Vector2Int.one);

            if (GUILayout.Button("Create"))
            {
                var path = EditorUtility.SaveFilePanelInProject("Decoration texture", "LegFrontDecoration.png", "png", "Choose file location");
                if (path != "")
                {
                    var resultTexture = MinifigUtility.CreateLegFrontDecoration(leftFootTexture, rightFootTexture, leftLegTexture, rightLegTexture, hipTexture, crotchTexture, resolution);
                    var bytes = resultTexture.EncodeToPNG();
                    File.WriteAllBytes(path, bytes);
                    AssetDatabase.Refresh();

                    var importer = AssetImporter.GetAtPath(path) as TextureImporter;
                    importer.alphaIsTransparency = true;
                    importer.SaveAndReimport();

                    this.Close();
                }
            }
        }

        private void ShowTextureButton(ref Texture2D texture, Rect rect, int controlId)
        {
            if (texture)
            {
                GUI.DrawTexture(new Rect(rect.position * decorationBackgroundSize, rect.size * decorationBackgroundSize), texture);
                if (GUI.Button(new Rect(rect.position * decorationBackgroundSize, rect.size * decorationBackgroundSize), "", EditorStyles.label))
                {
                    EditorGUIUtility.ShowObjectPicker<Texture2D>(texture, false, "", controlId);
                }
            }
            else
            {
                if (GUI.Button(new Rect(rect.position * decorationBackgroundSize, rect.size * decorationBackgroundSize), "Pick", EditorStyles.centeredGreyMiniLabel))
                {
                    EditorGUIUtility.ShowObjectPicker<Texture2D>(texture, false, "", controlId);
                }
            }

            if (Event.current.type == EventType.ExecuteCommand && Event.current.commandName == "ObjectSelectorUpdated")
            {
                if (EditorGUIUtility.GetObjectPickerControlID() == controlId)
                {
                    texture = (Texture2D)EditorGUIUtility.GetObjectPickerObject();
                }
            }
        }
    }
}
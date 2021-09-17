using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;

#if UNITY_EDITOR
namespace LEGOModelImporter.DuckSample
{
    public class KnobLayerCheck : MonoBehaviour
    {
        static readonly string knobLayerAttemptPrefsKey = "com.lego.modelimporter.attemptCreatingMissingKnobLayer";

        [InitializeOnLoadMethod]
        static void DoCheckConnectivityFeatureLayer()
        {
            // Do not perform the check when playing.
            if (EditorApplication.isPlayingOrWillChangePlaymode)
            {
                return;
            }

            var existingKnobLayer = LayerMask.NameToLayer(AnimatedGroup.KnobLayerName);
            var attempt = EditorPrefs.GetBool(knobLayerAttemptPrefsKey, true);

            if (existingKnobLayer == -1 && attempt)
            {
                EditorApplication.delayCall += CreateKnobLayer;
            }
        }

        private static void ReportError(string layer, string prefsKey)
        {
            EditorUtility.DisplayDialog("Knob collider layer required by LEGO packages", "Could not set up layer used for knob colliders automatically. Please add a layer called '" + layer + "'", "Ok");
            EditorPrefs.SetBool(prefsKey, false);
        }

        private static void AddLayer(string layer, string prefsKey)
        {
            var tagManagerAsset = AssetDatabase.LoadAssetAtPath<Object>(Path.Combine("ProjectSettings", "TagManager.asset"));
            if (tagManagerAsset == null)
            {
                ReportError(layer, prefsKey);
                return;
            }

            SerializedObject tagManagerObject = new SerializedObject(tagManagerAsset);
            if (tagManagerObject == null)
            {
                ReportError(layer, prefsKey);
                return;
            }

            SerializedProperty layersProp = tagManagerObject.FindProperty("layers");
            if (layersProp == null || !layersProp.isArray)
            {
                ReportError(layer, prefsKey);
                return;
            }

            for (int i = 8; i < layersProp.arraySize; i++)
            {
                SerializedProperty layerProp = layersProp.GetArrayElementAtIndex(i);
                if (layerProp.stringValue == layer)
                {
                    return;
                }
            }

            for (int i = 8; i < layersProp.arraySize; i++)
            {
                SerializedProperty layerProp = layersProp.GetArrayElementAtIndex(i);
                if (layerProp.stringValue == "")
                {
                    layerProp.stringValue = layer;
                    EditorUtility.DisplayDialog("Knob collider layer required by LEGO packages", "Set up layer used for knob colliders called '" + layer + "' at index " + i, "Ok");
                    break;
                }
            }

            EditorPrefs.SetBool(prefsKey, true);

            tagManagerObject.ApplyModifiedProperties();
        }

        public static void CreateKnobLayer()
        {
            AddLayer(AnimatedGroup.KnobLayerName, knobLayerAttemptPrefsKey);
        }
    }
}
#endif
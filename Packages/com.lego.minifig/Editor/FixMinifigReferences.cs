using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEditor;

namespace LEGOMinifig
{
    public static class FixMinifigReferences
    {
        [MenuItem("LEGO Tools/Dev/Fix Minifig References")]
        static void FixSelectedMinifigReferences()
        {
            var selection = Selection.activeTransform;

            var minifig = selection.GetComponent<Minifig>();
            var children = minifig.GetComponentsInChildren<Transform>(true);

            var minifigSerializedObject = new SerializedObject(minifig);

            minifigSerializedObject.FindProperty("headAccessoryLocator").objectReferenceValue = children.First(x => x.name == "hat_loc");
            minifigSerializedObject.FindProperty("neckAccessoryLocator").objectReferenceValue = children.First(x => x.name == "head_loc");
            minifigSerializedObject.FindProperty("leftHandAccessoryLocator").objectReferenceValue = children.First(x => x.name == "hand_L_loc");
            minifigSerializedObject.FindProperty("rightHandAccessoryLocator").objectReferenceValue = children.First(x => x.name == "hand_R_loc");
            minifigSerializedObject.FindProperty("waistAccessoryLocator").objectReferenceValue = children.First(x => x.name == "detatchTorso");
            minifigSerializedObject.FindProperty("capeAccessoryLocator").objectReferenceValue = children.First(x => x.name == "neckModule_grp");
            minifigSerializedObject.FindProperty("torsoLocator").objectReferenceValue = children.First(x => x.name == "torso_loc");
            minifigSerializedObject.FindProperty("hipLocator").objectReferenceValue = children.First(x => x.name == "hip_loc");
            minifigSerializedObject.FindProperty("torsoBackDecorationGeometry").objectReferenceValue = children.First(x => x.name == "VME_11003814_2DP_Back_2");
            minifigSerializedObject.FindProperty("torsoFrontDecorationGeometry").objectReferenceValue = children.First(x => x.name == "VME_11003814_2DP_Front_1");
            minifigSerializedObject.FindProperty("torsoGeometry").objectReferenceValue = children.First(x => x.name == "Torso 1");

            var armsGeometryProp = minifigSerializedObject.FindProperty("armsGeometry");
            armsGeometryProp.arraySize = 2;
            armsGeometryProp.GetArrayElementAtIndex(0).objectReferenceValue = children.First(x => x.name == "Arm_Left");
            armsGeometryProp.GetArrayElementAtIndex(1).objectReferenceValue = children.First(x => x.name == "Arm_Right");

            var handsGeometryProp = minifigSerializedObject.FindProperty("handsGeometry");
            handsGeometryProp.arraySize = 2;
            handsGeometryProp.GetArrayElementAtIndex(0).objectReferenceValue = children.First(x => x.name == "Hand_Left");
            handsGeometryProp.GetArrayElementAtIndex(1).objectReferenceValue = children.First(x => x.name == "Hand_Right");

            var legsGeometryProp = minifigSerializedObject.FindProperty("legsGeometry");
            legsGeometryProp.arraySize = 8;
            legsGeometryProp.GetArrayElementAtIndex(0).objectReferenceValue = children.First(x => x.name == "Hip");
            legsGeometryProp.GetArrayElementAtIndex(1).objectReferenceValue = children.First(x => x.name == "VME_11003815_2DP_Front_1");
            legsGeometryProp.GetArrayElementAtIndex(2).objectReferenceValue = children.First(x => x.name == "Leg_Left");
            legsGeometryProp.GetArrayElementAtIndex(3).objectReferenceValue = children.First(x => x.name == "VME_11003817_2DP_Front_1");
            legsGeometryProp.GetArrayElementAtIndex(4).objectReferenceValue = children.First(x => x.name == "VME_11003817_2DP_Side_1");
            legsGeometryProp.GetArrayElementAtIndex(5).objectReferenceValue = children.First(x => x.name == "Leg_Right");
            legsGeometryProp.GetArrayElementAtIndex(6).objectReferenceValue = children.First(x => x.name == "VME_11003816_2DP_Front_1");
            legsGeometryProp.GetArrayElementAtIndex(7).objectReferenceValue = children.First(x => x.name == "VME_11003816_2DP_Side_1");

            minifigSerializedObject.FindProperty("headGeometry").objectReferenceValue = children.First(x => x.name == "Head");
            minifigSerializedObject.FindProperty("faceGeometry").objectReferenceValue = children.First(x => x.name == "Face");

            minifigSerializedObject.ApplyModifiedPropertiesWithoutUndo();

            minifigSerializedObject.Dispose();

            EditorUtility.SetDirty(minifig);
        }

        [MenuItem("LEGO Tools/Dev/Fix Minifig References", true)]
        static bool ValidateSelectedMinifigReferences()
        {
            var selection = Selection.activeTransform;
            if (selection)
            {
                return selection.GetComponent<Minifig>();
            }

            return false;

        }
    }
}


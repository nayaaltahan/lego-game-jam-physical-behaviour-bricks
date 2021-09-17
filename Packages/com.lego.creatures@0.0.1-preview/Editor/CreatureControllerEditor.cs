using UnityEditor;
using UnityEngine;

namespace LEGO.Creatures.Editor
{
    [CustomEditor(typeof(CreatureController))]
    public class CreatureControllerEditor : UnityEditor.Editor
    {
        SerializedProperty AddCollidersProp;
        SerializedProperty ClipsProp;

        CreatureController creatureController;

        void OnEnable()
        {
            creatureController = (CreatureController)target;

            AddCollidersProp = serializedObject.FindProperty("addBoundsCollider");
            ClipsProp = serializedObject.FindProperty("clips");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.PropertyField(AddCollidersProp);

            EditorGUILayout.Space(); // Insert space.

            foreach (var clip in ClipsProp)
            {
                if (!creatureController.GotAnimationController())
                {
                    EditorGUILayout.HelpBox("No animation controller attached to Animator.\nPlease setup and attach one for this script to work.", MessageType.Warning);
                }

                EditorGUI.BeginDisabledGroup(EditorApplication.isPlaying);

                var serializedClip = clip as SerializedProperty;

                var serializedAnimationClip = serializedClip.FindPropertyRelative("AnimationClip");

                EditorGUI.EndDisabledGroup();

                EditorGUILayout.PropertyField(serializedAnimationClip);
                EditorGUILayout.PropertyField(serializedClip.FindPropertyRelative("AudioClip"));
                EditorGUILayout.PropertyField(serializedClip.FindPropertyRelative("AudioDelay"));

                EditorGUI.BeginDisabledGroup(!EditorApplication.isPlaying);

                if (GUILayout.Button("Play"))
                {
                    creatureController.Play((serializedAnimationClip.objectReferenceValue as AnimationClip).name);
                }

                EditorGUI.EndDisabledGroup();

                EditorGUILayout.Space(); // Insert space.
            }

            if (GUILayout.Button("Refresh Clips"))
            {
                creatureController.UpdateAnimationClipReferences();
                EditorUtility.SetDirty(creatureController);
            }

            serializedObject.ApplyModifiedProperties();
        }
    }
}

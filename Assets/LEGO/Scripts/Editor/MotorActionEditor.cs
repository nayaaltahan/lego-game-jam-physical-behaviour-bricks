using UnityEditor;
using UnityEngine;
using Unity.LEGO.Behaviours;
using Unity.LEGO.Behaviours.Actions;

namespace Unity.LEGO.EditorExt
{
    [CustomEditor(typeof(MotorAction), true)]
    public class MotorActionEditor : RepeatableActionEditor
    {
        SerializedProperty m_SpeedProp;
        SerializedProperty m_DirectionProp;
        SerializedProperty m_TimeProp;
        SerializedProperty m_PortProp;

        protected override void OnEnable()
        {
            base.OnEnable();

            m_SpeedProp = serializedObject.FindProperty("m_Speed");
            m_DirectionProp = serializedObject.FindProperty("m_Direction");
            m_TimeProp = serializedObject.FindProperty("m_Time");
            m_PortProp = serializedObject.FindProperty("m_Port");
        }

        protected override void CreateGUI()
        {
            EditorGUILayout.PropertyField(m_SpeedProp);
            EditorGUILayout.PropertyField(m_DirectionProp);
            EditorGUILayout.PropertyField(m_TimeProp);

            EditorGUI.BeginDisabledGroup(EditorApplication.isPlaying);

            EditorGUILayout.PropertyField(m_PortProp);

            EditorGUI.EndDisabledGroup();

            EditorGUILayout.PropertyField(m_PauseProp);

            EditorGUI.BeginDisabledGroup(EditorApplication.isPlaying);

            EditorGUILayout.PropertyField(m_RepeatProp);

            EditorGUI.EndDisabledGroup();
        }
    }
}

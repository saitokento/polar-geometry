using UnityEditor;
using UnityEngine;

namespace PolarGeometry.Editor
{
    [CustomEditor(typeof(StringPolarFunction))]
    public class StringPolarFunctionEditor
        : UnityEditor.Editor
    {
        private SerializedProperty expression;
        private SerializedProperty parameters;
        private SerializedProperty thetaSpanDegrees;

        private void OnEnable()
        {
            expression =
                serializedObject.FindProperty(
                    "expression"
                );

            parameters =
                serializedObject.FindProperty(
                    "parameters"
                );

            thetaSpanDegrees =
                serializedObject.FindProperty(
                    "thetaSpanDegrees"
                );
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.PropertyField(
                expression
            );

            EditorGUILayout.PropertyField(
                parameters,
                true
            );

            EditorGUILayout.PropertyField(
                thetaSpanDegrees
            );

            serializedObject.ApplyModifiedProperties();

            EditorGUILayout.Space();

            StringPolarFunction function =
                (StringPolarFunction)target;

            if (GUILayout.Button("Apply"))
            {
                function.Apply();
            }

            if (!string.IsNullOrEmpty(
                function.ErrorMessage))
            {
                EditorGUILayout.HelpBox(
                    function.ErrorMessage,
                    MessageType.Error
                );
            }
        }
    }
}

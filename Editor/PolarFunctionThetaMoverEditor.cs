using UnityEditor;

namespace PolarGeometry.Editor
{
    [CustomEditor(typeof(PolarFunctionThetaMover))]
    public class PolarFunctionThetaMoverEditor
        : UnityEditor.Editor
    {
        private SerializedProperty function;
        private SerializedProperty movementMode;

        private SerializedProperty useFunctionThetaSpan;
        private SerializedProperty startThetaDegrees;
        private SerializedProperty endThetaDegrees;

        private SerializedProperty angularSpeedDegreesPerSecond;
        private SerializedProperty loop;

        private SerializedProperty deltaThetaDegrees;

        private void OnEnable()
        {
            function =
                serializedObject.FindProperty("function");

            movementMode =
                serializedObject.FindProperty("movementMode");

            useFunctionThetaSpan =
                serializedObject.FindProperty("useFunctionThetaSpan");

            startThetaDegrees =
                serializedObject.FindProperty("startThetaDegrees");

            endThetaDegrees =
                serializedObject.FindProperty("endThetaDegrees");

            angularSpeedDegreesPerSecond =
                serializedObject.FindProperty(
                    "angularSpeedDegreesPerSecond"
                );

            loop =
                serializedObject.FindProperty("loop");

            deltaThetaDegrees =
                serializedObject.FindProperty("deltaThetaDegrees");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.PropertyField(function);
            EditorGUILayout.PropertyField(movementMode);

            EditorGUILayout.Space();

            EditorGUILayout.PropertyField(
                useFunctionThetaSpan
            );

            EditorGUILayout.PropertyField(
                startThetaDegrees
            );

            if (!useFunctionThetaSpan.boolValue)
            {
                EditorGUILayout.PropertyField(
                    endThetaDegrees
                );
            }

            EditorGUILayout.Space();

            EditorGUILayout.PropertyField(
                angularSpeedDegreesPerSecond
            );

            EditorGUILayout.PropertyField(loop);

            if (
                movementMode.enumValueIndex ==
                (int)PolarFunctionThetaMover.MovementMode.Discrete
            )
            {
                EditorGUILayout.Space();

                EditorGUILayout.LabelField(
                    "Discrete",
                    EditorStyles.boldLabel
                );

                EditorGUILayout.PropertyField(
                    deltaThetaDegrees
                );
            }

            serializedObject.ApplyModifiedProperties();
        }
    }
}
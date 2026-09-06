using UnityEditor;

namespace PolarGeometry.Editor
{
    [CustomEditor(typeof(PolarFunctionLineRenderer))]
    public class PolarFunctionLineRendererEditor : UnityEditor.Editor
    {
        private SerializedProperty function;

        private SerializedProperty useFunctionThetaSpan;
        private SerializedProperty startThetaDegrees;
        private SerializedProperty endThetaDegrees;

        private SerializedProperty useAdaptiveSampling;
        private SerializedProperty deltaThetaDegrees;
        private SerializedProperty maxCoordinateDelta;
        private SerializedProperty maxDeltaThetaDegrees;
        private SerializedProperty maxDepth;
        private SerializedProperty tolerance;

        private void OnEnable()
        {
            function =
                serializedObject.FindProperty("function");

            useFunctionThetaSpan =
                serializedObject.FindProperty("useFunctionThetaSpan");

            startThetaDegrees =
                serializedObject.FindProperty("startThetaDegrees");

            endThetaDegrees =
                serializedObject.FindProperty("endThetaDegrees");

            useAdaptiveSampling =
                serializedObject.FindProperty("useAdaptiveSampling");

            deltaThetaDegrees =
                serializedObject.FindProperty("deltaThetaDegrees");

            maxCoordinateDelta =
                serializedObject.FindProperty("maxCoordinateDelta");

            maxDeltaThetaDegrees =
                serializedObject.FindProperty("maxDeltaThetaDegrees");

            maxDepth =
                serializedObject.FindProperty("maxDepth");

            tolerance =
                serializedObject.FindProperty("tolerance");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.PropertyField(function);

            EditorGUILayout.Space();

            EditorGUILayout.LabelField(
                "Range",
                EditorStyles.boldLabel
            );

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

            EditorGUILayout.LabelField(
                "Sampling",
                EditorStyles.boldLabel
            );

            EditorGUILayout.PropertyField(
                useAdaptiveSampling
            );

            if (useAdaptiveSampling.boolValue)
            {
                EditorGUILayout.PropertyField(
                    maxCoordinateDelta
                );

                EditorGUILayout.PropertyField(
                    maxDeltaThetaDegrees
                );

                EditorGUILayout.PropertyField(
                    maxDepth
                );
            }
            else
            {
                EditorGUILayout.PropertyField(
                    deltaThetaDegrees
                );
            }

            EditorGUILayout.PropertyField(
                tolerance
            );

            serializedObject.ApplyModifiedProperties();
        }
    }
}
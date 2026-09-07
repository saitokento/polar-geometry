using UnityEditor;

namespace PolarGeometry.Editor
{
    [CustomEditor(typeof(PolarFunctionEdgeCollider2D))]
    public class PolarFunctionEdgeCollider2DEditor : UnityEditor.Editor
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

        private SerializedProperty loop;
        private SerializedProperty matchLineRendererWidth;

        private SerializedProperty autoUpdate;

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

            loop =
                serializedObject.FindProperty("loop");

            matchLineRendererWidth =
                serializedObject.FindProperty("matchLineRendererWidth");

            autoUpdate =
                serializedObject.FindProperty("autoUpdate");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.PropertyField(function);

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

            EditorGUILayout.Space();

            EditorGUILayout.PropertyField(
                loop
            );

            EditorGUILayout.PropertyField(
                matchLineRendererWidth
            );

            EditorGUILayout.Space();

            EditorGUILayout.PropertyField(
                autoUpdate
            );

            serializedObject.ApplyModifiedProperties();
        }
    }
}
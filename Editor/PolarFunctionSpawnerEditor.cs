using UnityEditor;

namespace PolarGeometry.Editor
{
    [CustomEditor(typeof(PolarFunctionSpawner))]
    public class PolarFunctionSpawnerEditor
        : UnityEditor.Editor
    {
        private SerializedProperty function;

        private SerializedProperty useFunctionThetaSpan;
        private SerializedProperty startThetaDegrees;
        private SerializedProperty endThetaDegrees;

        private SerializedProperty prefab;
        private SerializedProperty spawnParent;

        private SerializedProperty spawnMode;

        private SerializedProperty deltaThetaDegrees;
        private SerializedProperty alignToPath;

        private SerializedProperty angularSpeedDegreesPerSecond;

        private SerializedProperty autoUpdate;

        private void OnEnable()
        {
            function =
                serializedObject.FindProperty("function");

            useFunctionThetaSpan =
                serializedObject.FindProperty(
                    "useFunctionThetaSpan"
                );

            startThetaDegrees =
                serializedObject.FindProperty(
                    "startThetaDegrees"
                );

            endThetaDegrees =
                serializedObject.FindProperty(
                    "endThetaDegrees"
                );

            prefab =
                serializedObject.FindProperty("prefab");

            spawnParent =
                serializedObject.FindProperty("spawnParent");

            spawnMode =
                serializedObject.FindProperty(
                    "spawnMode"
                );

            deltaThetaDegrees =
                serializedObject.FindProperty(
                    "deltaThetaDegrees"
                );

            alignToPath =
                serializedObject.FindProperty(
                    "alignToPath"
                );

            angularSpeedDegreesPerSecond =
                serializedObject.FindProperty(
                    "angularSpeedDegreesPerSecond"
                );

            autoUpdate =
                serializedObject.FindProperty("autoUpdate");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.PropertyField(
                function
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

            EditorGUILayout.PropertyField(
                prefab
            );

            EditorGUILayout.PropertyField(
                spawnParent
            );

            EditorGUILayout.PropertyField(
                spawnMode
            );

            EditorGUILayout.PropertyField(
                deltaThetaDegrees
            );

            EditorGUILayout.PropertyField(
                alignToPath
            );

            if (
                spawnMode.enumValueIndex ==
                (int)PolarFunctionSpawner.SpawnMode.Progressive
            )
            {
                EditorGUILayout.PropertyField(
                    angularSpeedDegreesPerSecond
                );
            }

            EditorGUILayout.Space();

            EditorGUILayout.PropertyField(
                autoUpdate
            );

            serializedObject.ApplyModifiedProperties();
        }
    }
}

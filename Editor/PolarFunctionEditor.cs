using UnityEditor;
using UnityEngine;

namespace PolarGeometry.Editor
{
    [CustomEditor(typeof(PolarFunction), true)]
    public class PolarFunctionEditor : UnityEditor.Editor
    {
        private GUIStyle formulaStyle;

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            PolarFunction function = (PolarFunction)target;

            if (formulaStyle == null)
            {
                formulaStyle = new GUIStyle(EditorStyles.label)
                {
                    wordWrap = true
                };
            }

            GUIContent formulaContent = new GUIContent(function.Formula);

            float valueWidth =
                EditorGUIUtility.currentViewWidth
                - EditorGUIUtility.labelWidth
                - 20f;

            float height = formulaStyle.CalcHeight(
                formulaContent,
                Mathf.Max(valueWidth, 1f)
            );

            Rect rect = EditorGUILayout.GetControlRect(
                false,
                Mathf.Max(
                    EditorGUIUtility.singleLineHeight,
                    height
                )
            );

            Rect labelRect = new Rect(
                rect.x,
                rect.y,
                EditorGUIUtility.labelWidth,
                EditorGUIUtility.singleLineHeight
            );

            Rect valueRect = new Rect(
                rect.x + EditorGUIUtility.labelWidth,
                rect.y,
                rect.width - EditorGUIUtility.labelWidth,
                rect.height
            );

            EditorGUI.LabelField(
                labelRect,
                "Formula"
            );

            EditorGUI.LabelField(
                valueRect,
                formulaContent,
                formulaStyle
            );

            DrawDefaultInspector();

            serializedObject.ApplyModifiedProperties();
        }
    }
}
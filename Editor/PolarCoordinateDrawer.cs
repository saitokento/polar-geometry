using UnityEditor;
using UnityEngine;

namespace PolarGeometry.Editor
{
    [CustomPropertyDrawer(typeof(PolarCoordinate))]
    public class PolarCoordinateDrawer : PropertyDrawer
    {
        private static readonly GUIContent[] Labels =
        {
            new("r"),
            new("θ")
        };

        public override void OnGUI(
            Rect position,
            SerializedProperty property,
            GUIContent label)
        {
            SerializedProperty r =
                property.FindPropertyRelative("r");

            SerializedProperty theta =
                property.FindPropertyRelative("theta");

            EditorGUI.BeginProperty(position, label, property);

            float[] values =
            {
                r.floatValue,
                theta.floatValue * Mathf.Rad2Deg
            };

            EditorGUI.MultiFloatField(
                position,
                label,
                Labels,
                values
            );

            r.floatValue = values[0];
            theta.floatValue = values[1] * Mathf.Deg2Rad;

            EditorGUI.EndProperty();
        }
    }
}
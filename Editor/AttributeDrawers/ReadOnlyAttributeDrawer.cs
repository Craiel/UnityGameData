namespace Craiel.UnityGameData.Editor.AttributeDrawers
{
    using System.Globalization;
    using Attributes;
    using UnityEditor;
    using UnityEngine;

    [CustomPropertyDrawer(typeof(ReadOnlyAttribute))]
    [CanEditMultipleObjects]
    public class ReadOnlyAttributeDrawer : PropertyDrawer
    {
        // -------------------------------------------------------------------
        // Public
        // -------------------------------------------------------------------
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            position.width -= 40;
            if (property.hasMultipleDifferentValues)
            {
                EditorGUI.LabelField(position, label.text, "-");
            }
            else
            {
                switch (property.propertyType)
                {
                    case SerializedPropertyType.String:
                    {
                        EditorGUI.LabelField(position, label.text, property.stringValue);
                        break;
                    }

                    case SerializedPropertyType.Integer:
                    {
                        EditorGUI.LabelField(position, label.text, property.intValue.ToString());
                        break;
                    }

                    case SerializedPropertyType.Float:
                    {
                        EditorGUI.LabelField(position, label.text, property.floatValue.ToString(CultureInfo.InvariantCulture));
                        break;
                    }

                    default:
                    {
                        UnityEngine.Debug.LogError("Unsupported format for ReadOnlyAttributeDrawer: " + property.propertyType);
                        break;
                    }
                }
                
                position.x = position.xMax;
                position.width = 40;
                if (GUI.Button(position, "Copy"))
                {
                    GUIUtility.systemCopyBuffer = property.stringValue;
                }
            }
        }
    }
}
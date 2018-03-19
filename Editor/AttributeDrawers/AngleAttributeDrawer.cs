namespace Craiel.UnityGameData.Editor.AttributeDrawers
{
    using System.Reflection;
    using Attributes;
    using UnityEditor;
    using UnityEngine;

    [CustomPropertyDrawer(typeof(AngleAttribute))]
    public class AngleAttributeDrawer : PropertyDrawer
    {
        private static readonly MethodInfo KnobMethodInfo = typeof(EditorGUI).GetMethod(
            "Knob",
            BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Static);

        // -------------------------------------------------------------------
        // Public
        // -------------------------------------------------------------------
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            if (property.propertyType == SerializedPropertyType.Float)
            {
                using (new EditorGUI.PropertyScope(position, label, property))
                {
                    EditorGUI.LabelField(position, label);
                    var knobRect = new Rect(position);
                    knobRect.x += EditorGUIUtility.labelWidth;
                    property.floatValue = this.Knob(
                        knobRect,
                        Vector2.one * this.AngleAttribute.KnobSize,
                        property.floatValue,
                        this.AngleAttribute.Min,
                        this.AngleAttribute.Max,
                        this.AngleAttribute.Unit,
                        this.AngleAttribute.BackgroundColor,
                        this.AngleAttribute.ActiveColor,
                        this.AngleAttribute.ShowValue);
                }
            }
            else
            {
                EditorGUI.PropertyField(position, property, label);
            }
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            var height = base.GetPropertyHeight(property, label);
            return property.propertyType != SerializedPropertyType.Float ? height : this.AngleAttribute.KnobSize + 4;
        }

        // -------------------------------------------------------------------
        // Private
        // -------------------------------------------------------------------
        private AngleAttribute AngleAttribute
        {
            get
            {
                return (AngleAttribute)this.attribute;
            }
        }

        private float Knob(Rect position, Vector2 knobSize, float currentValue, float start, float end, string unit, Color backgroundColor, Color activeColor, bool showValue)
        {
            var invoke = KnobMethodInfo.Invoke(null, new object[] { position, knobSize, currentValue, start, end, unit, backgroundColor, activeColor, showValue, GUIUtility.GetControlID("Knob".GetHashCode(), FocusType.Passive, position) });
            return (float)(invoke ?? 0);
        }
    }
}
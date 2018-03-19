namespace Craiel.UnityGameData.Editor.AttributeDrawers
{
    using Attributes;
    using UnityEditor;
    using UnityEngine;

    [CustomPropertyDrawer(typeof(ProgressBarAttribute))]
    public class ProgressBarAttributeDrawer : PropertyDrawer
    {
        private Rect rect;
        private ProgressBarAttribute attr;
        private GUIStyle labelStyleRight;


        public override void OnGUI(Rect position, SerializedProperty prop, GUIContent label)
        {
            this.attr = this.attribute as ProgressBarAttribute;
            this.rect = position;
            if (this.labelStyleRight == null)
            {
                this.labelStyleRight = new GUIStyle("Label");
                this.labelStyleRight.alignment = TextAnchor.MiddleRight;
            }
            
            EditorGUI.showMixedValue = prop.hasMultipleDifferentValues;
            
            if (prop.propertyType == SerializedPropertyType.Integer)
            {
                this.ProgressBar(label, prop.intValue);
            }
            else if (prop.propertyType == SerializedPropertyType.Float)
            {
                this.ProgressBar(label, prop.floatValue);
            }
        }

        private void ProgressBar(GUIContent label, float val)
        {
            var diff = this.attr.Max - this.attr.Min;
            var progress = val / diff;

            EditorGUI.PrefixLabel(this.rect, label);
            this.rect.x += EditorGUIUtility.labelWidth;
            this.rect.width -= EditorGUIUtility.labelWidth;
            EditorGUI.ProgressBar(this.rect, progress, val.ToString());


           

            GUI.Label(this.rect, this.attr.Min.ToString());
            GUI.Label(this.rect, this.attr.Max.ToString(), this.labelStyleRight);

        }
    }
}
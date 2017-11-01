namespace Assets.Scripts.Craiel.GameData.Editor.AttributeDrawers
{
    using Attributes;
    using NLog;
    using UnityEditor;
    using UnityEngine;

    [CustomPropertyDrawer(typeof(HelpBoxAttribute))]
    public class HelpBoxDrawer : DecoratorDrawer
    {
        // -------------------------------------------------------------------
        // Public
        // -------------------------------------------------------------------
        public override float GetHeight()
        {
            HelpBoxAttribute helpAttr = (HelpBoxAttribute) this.attribute;

            return Mathf.Max(40,GUI.skin.GetStyle("HelpBox").CalcHeight(new GUIContent(helpAttr.Message), EditorGUIUtility.currentViewWidth));
        }

        public override void OnGUI(Rect position)
        {
            HelpBoxAttribute helpAttr = (HelpBoxAttribute) this.attribute;

            MessageType type = MessageType.Info;
            if (helpAttr.Level == LogLevel.Error)
            {
                type = MessageType.Error;
            }

            EditorGUI.HelpBox(position, helpAttr.Message, type);
        }
    }
}
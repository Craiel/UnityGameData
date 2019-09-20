namespace Craiel.UnityGameData.Editor.Common
{
    using System.Linq;
    using UnityEditor;
    using UnityEngine;
    using UnityEssentials.Editor.UserInterface;
    using UnityEssentials.Runtime;

    [CustomPropertyDrawer(typeof(GameResourceRefBase), true)]
    public class StaticResourceRefEditor : BaseRefEditor
    {
        // -------------------------------------------------------------------
        // Protected
        // -------------------------------------------------------------------
        protected override string GetActiveObjectString(SerializedProperty property)
        {
            Object activeObject = GetActiveObject(property);
            return activeObject == null ? null : activeObject.name;
        }

        protected override Object GetActiveObject(SerializedProperty property)
        {
            this.SetParameters(property);

            SerializedProperty valueProperty = property.FindPropertyRelative<GameResourceRefBase>(x => x.Resource);
            return valueProperty.objectReferenceValue;
        }

        protected override void SelectObject(Object activeObject, SerializedProperty property)
        {
            if (activeObject != null)
            {
                Selection.activeObject = activeObject;
                EditorGUIUtility.PingObject(activeObject);
            }
        }

        protected override void PickObject(SerializedProperty property, Rect displayRect)
        {
            var popup = ScriptableObject.CreateInstance<StaticResourceFinderPopUp>();
            popup.Initialize(property, selected => this.SetObject(property, selected.FirstOrDefault()));
            popup.ShowAsDropDown(displayRect, GameDataStyles.FinderPopupSize);
        }

        // -------------------------------------------------------------------
        // Private
        // -------------------------------------------------------------------
        private void SetObject(SerializedProperty property, Object obj)
        {
            property.FindPropertyRelative<GameResourceRefBase>(x => x.Resource).objectReferenceValue = obj;
            property.serializedObject.ApplyModifiedProperties();
        }

        // -------------------------------------------------------------------
        // Private
        // -------------------------------------------------------------------
        private void SetParameters(SerializedProperty property)
        {
            if (property.type == TypeCache<GameResourceSpriteRef>.Value.Name)
            {
                this.IconSelector = UnityObjectHelper.SpriteIconSelector;
                return;
            }

            if (property.type == TypeCache<GameResourceAnimationClipRef>.Value.Name)
            {
                this.NameSelector = UnityObjectHelper.AnimationNameSelector;
                return;
            }
        }
    }
}
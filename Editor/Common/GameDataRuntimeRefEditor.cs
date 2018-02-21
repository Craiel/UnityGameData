namespace Assets.Scripts.Craiel.GameData.Editor.Common
{
    using System.Linq;
    using Essentials.Editor.UserInterface;
    using GameData;
    using UnityEditor;
    using UnityEngine;
    using Window;

    [CustomPropertyDrawer(typeof(GameDataRuntimeRefBase), true)]
    public class GameDataRuntimeRefEditor : BaseRefEditor
    {
        // -------------------------------------------------------------------
        // Constructor
        // -------------------------------------------------------------------
        public GameDataRuntimeRefEditor()
        {
            this.IconSelector = GameDataHelpers.DefaultIconSelector;
        }

        // -------------------------------------------------------------------
        // Protected
        // -------------------------------------------------------------------
        protected override string GetActiveObjectString(SerializedProperty property)
        {
            return property.FindPropertyRelative<GameDataRuntimeRefBase>(x => x.RefGuid).stringValue;
        }

        protected override Object GetActiveObject(SerializedProperty property)
        {
            var guid = GetActiveObjectString(property);
            if (string.IsNullOrEmpty(guid))
            {
                return null;
            }
            
            return GetObject(guid);
        }
        
        protected override void SelectObject(Object activeObject, SerializedProperty property)
        {
            var window = EditorWindow.GetWindow<GameDataEditorWindow>();
            if (window != null)
            {
                window.SelectRef(property);
            }
        }

        protected override void PickObject(SerializedProperty property, Rect displayRect)
        {
            var popup = ScriptableObject.CreateInstance<BaseObjectFinderPopUp>();
            popup.Initialize(property, selected => SetObject(property, selected.FirstOrDefault()));
            popup.ShowAsDropDown(displayRect, new Vector2(400, 300));
        }

        // -------------------------------------------------------------------
        // Private
        // -------------------------------------------------------------------
        private GameDataObject GetObject(string guid)
        {
            var path = AssetDatabase.GUIDToAssetPath(guid);
            return AssetDatabase.LoadAssetAtPath<GameDataObject>(path);
        }

        private static void SetObject(SerializedProperty property, GameDataObject obj)
        {
            var key = AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(obj));
            property.FindPropertyRelative<GameDataRuntimeRefBase>(x => x.RefGuid).stringValue = key;
            property.serializedObject.ApplyModifiedProperties();
        }
    }
}
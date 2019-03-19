namespace Craiel.UnityGameData.Editor.Common
{
    using System;
    using System.Collections.Generic;
    using Editor;
    using UnityEditor;
    using UnityEngine;

    public class BaseObjectFinderPopUp : BaseFinderPopUp<GameDataObject>
    {
        private Action<IList<GameDataObject>> selectCallback;

        // -------------------------------------------------------------------
        // Public
        // -------------------------------------------------------------------
        public void Initialize(SerializedProperty property, Action<IList<GameDataObject>> callback, bool multiSelect = false)
        {
            this.selectCallback = callback;
            this.AllowMultiSelect = multiSelect;

            this.IconSelector = GameDataHelpers.DefaultIconSelector;
            this.NameSelector = (sourceObject, path, root) => ((GameDataObject) sourceObject).Name;

            this.SetParameters(property);
        }

        // -------------------------------------------------------------------
        // Protected
        // -------------------------------------------------------------------
        protected override bool RefreshEntry(string assetPath, out GameDataObject asset)
        {
            asset = AssetDatabase.LoadAssetAtPath<GameDataObject>(assetPath);
            return asset != null;
        }
        
        protected override void Select(IList<GameDataObject> entries)
        {
            if (this.selectCallback != null)
            {
                this.selectCallback(entries);
            }
        }

        // -------------------------------------------------------------------
        // Private
        // -------------------------------------------------------------------
        private void SetParameters(SerializedProperty property)
        {
            string rawTypeName = property.type;
            
            if (rawTypeName.EndsWith("Ref"))
            {
                rawTypeName = property.type.Remove(property.type.Length - 3);
            }

            if (rawTypeName.Contains("Runtime"))
            {
                rawTypeName = rawTypeName.Replace("Runtime", string.Empty);
            }

            if (property.serializedObject != null
                && property.serializedObject.targetObject != null)
            {
                var virtualRefParent = property.serializedObject.targetObject as GameDataRefVirtualHolder;
                if (virtualRefParent != null && !string.IsNullOrEmpty(virtualRefParent.TypeFilter))
                {
                    rawTypeName = virtualRefParent.TypeFilter;
                }
            }

            this.TypeFilters = new []{rawTypeName};
        }
    }
}
namespace Craiel.UnityGameData.Editor.Common
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Reflection;
    using Attributes;
    using UnityEditor;
    using UnityEngine;
    using UnityEssentials;
    using UnityEssentials.Runtime;
    using Object = UnityEngine.Object;

    public class StaticResourceFinderPopUp : BaseFinderPopUp<Object>
    {
        private Action<IList<Object>> selectCallback;

        // -------------------------------------------------------------------
        // Public
        // -------------------------------------------------------------------
        public void Initialize(SerializedProperty property, Action<IList<Object>> callback, string subFolder = null, bool allowMultiSelect = false)
        {
            this.selectCallback = callback;
            this.AllowMultiSelect = allowMultiSelect;

            this.SetParameters(property);

            this.Root = EssentialsCore.AssetsPath.ToDirectory(EssentialsCore.ResourcesFolderName);
            if (!string.IsNullOrEmpty(subFolder))
            {
                this.Root = this.Root.ToDirectory(subFolder);
            }
        }

        // -------------------------------------------------------------------
        // Protected
        // -------------------------------------------------------------------
        protected override bool RefreshEntry(string assetPath, out Object asset)
        {
            if (!Path.HasExtension(assetPath))
            {
                asset = null;
                return false;
            }

            asset = AssetDatabase.LoadAssetAtPath(assetPath, this.ObjectType);
            return asset != null;
        }
        
        protected override void Select(IList<Object> entries)
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
            GameDataResourceFinderSettingsAttribute settings = null;
            if (property.serializedObject != null
                && property.serializedObject.targetObject != null)
            {
                Type targetType = property.serializedObject.targetObject.GetType();
                FieldInfo targetField = targetType.GetField(property.propertyPath);
                settings = targetField?.GetCustomAttribute<GameDataResourceFinderSettingsAttribute>();
            }

            if (settings != null)
            {
                this.Style = settings.Style;
            }
            
            if (property.type == typeof(GameResourceGameObjectRef).Name)
            {
                this.ObjectType = typeof(GameObject);
                this.TypeFilters = new []{this.ObjectType.Name};
                return;
            }

            if (property.type == typeof(GameResourceSpriteRef).Name)
            {
                this.ObjectType = typeof(Sprite);
                this.TypeFilters = new []{this.ObjectType.Name};
                this.IconSelector = UnityObjectHelper.SpriteIconSelector;
                return;
            }

            if (property.type == typeof(GameResourceAudioClipRef).Name)
            {
                this.ObjectType = typeof(AudioClip);
                this.TypeFilters = new []{this.ObjectType.Name};
                return;
            }

            if (property.type == typeof(GameResourcePrefabRef).Name)
            {
                this.TypeFilters = new []{"Prefab"};
                this.NameSelector = UnityObjectHelper.DefaultPathAndNameSelector;
                return;
            }

            if (property.type == typeof(GameResourceAnimationClipRef).Name)
            {
                this.ObjectType = typeof(AnimationClip);
                this.TypeFilters = new []{this.ObjectType.Name};
                this.NameSelector = UnityObjectHelper.AnimationNameSelector;
                return;
            }
            
            if (property.type == typeof(GameResourceCustomRef).Name)
            {
                // TODO
                this.TypeFilters = new []{settings == null ? "Prefab" : settings.AttachedScript?.Name};
                this.NameSelector = UnityObjectHelper.DefaultPathAndNameSelector;

                return;
            }
        }
    }
}
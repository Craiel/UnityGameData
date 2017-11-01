namespace Assets.Scripts.Craiel.GameData.Editor.Common
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using Essentials;
    using UnityEditor;
    using UnityEngine;
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
            if (property.type == typeof(GameResourceGameObjectRef).Name)
            {
                this.ObjectType = typeof(GameObject);
                this.TypeFilter = this.ObjectType.Name;
                return;
            }

            if (property.type == typeof(GameResourceSpriteRef).Name)
            {
                this.ObjectType = typeof(Sprite);
                this.TypeFilter = this.ObjectType.Name;
                this.IconSelector = UnityObjectHelper.SpriteIconSelector;
                return;
            }

            if (property.type == typeof(GameResourceAudioClipRef).Name)
            {
                this.ObjectType = typeof(AudioClip);
                this.TypeFilter = this.ObjectType.Name;
                return;
            }

            if (property.type == typeof(GameResourcePrefabRef).Name)
            {
                this.TypeFilter = "Prefab";
                this.NameSelector = UnityObjectHelper.DefaultPathAndNameSelector;
                return;
            }

            if (property.type == typeof(GameResourceAnimationClipRef).Name)
            {
                this.ObjectType = typeof(AnimationClip);
                this.TypeFilter = this.ObjectType.Name;
                this.NameSelector = UnityObjectHelper.AnimationNameSelector;
                return;
            }
        }
    }
}
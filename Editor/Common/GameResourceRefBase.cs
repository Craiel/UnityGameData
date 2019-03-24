namespace Craiel.UnityGameData.Editor.Common
{
    using System;
    using System.Collections.Generic;
    using Builder;
    using UnityEngine;
    using UnityEssentials;
    using UnityEssentials.Runtime;
    using UnityEssentials.Runtime.IO;
#if UNITY_EDITOR
    using UnityEditor;
#endif

    // ---------------------------------------------------------------------------------------------
    // Base class, non generic, for use with PropertyDrawer
    // ---------------------------------------------------------------------------------------------
    [Serializable]
    public abstract class GameResourceRefBase
    {
        private string cachedAssetPath;
        private string cachedPath;

        // ---------------------------------------------------------------------------------------------
        // Public
        // ---------------------------------------------------------------------------------------------
        [SerializeField]
        public UnityEngine.Object Resource;

        public bool IsValid()
        {
            return this.Resource != null;
        }

        public void Reset()
        {
            this.Resource = null;
        }

        public void Validate(object owner, GameDataBuildValidationContext context, bool isOptional = true)
        {
            if (this.Resource == null && !isOptional)
            {
                context.Error(owner, this, null, "Resource Ref is not Optional");
            }
        }
        
        public static void ValidateRefList<T>(object owner, object source, IList<T> refList, GameDataBuildValidationContext context)
            where T : GameResourceRefBase
        {
            if (refList.Count == 0)
            {
                return;
            }

            IList<T> emptyEntries = new List<T>();
            foreach (T refData in refList)
            {
                if (refData == null || !refData.IsValid())
                {
                    emptyEntries.Add(refData);
                }
            }

            if (emptyEntries.Count > 0)
            {
                context.WarningFormat(owner, source,
                    ((localOwner, entry) => GameDataBuildValidationFixers.ListRemoveInvalidFixer(localOwner, refList, emptyEntries)),
                    "Resource Ref List has {0} empty entries", emptyEntries.Count);
            }
        }

        public string GetPath()
        {
            if (!this.IsValid())
            {
                return string.Empty;
            }

#if UNITY_EDITOR
            var assetPath = AssetDatabase.GetAssetPath(this.Resource);
            if (string.IsNullOrEmpty(assetPath))
            {
                GameDataEditorCore.Logger.Warn("StaticResourceRef did not return a valid asset path: {0}", this.Resource);
                return string.Empty;
            }

            if (this.cachedAssetPath == assetPath)
            {
                return this.cachedPath;
            }

            this.cachedAssetPath = assetPath;
            
            int extensionIndex = assetPath.LastIndexOf(".", StringComparison.Ordinal);
            int resourceIndex = assetPath.IndexOf(EssentialsCore.ResourcesFolderName, StringComparison.Ordinal);
            if (resourceIndex < 0)
            {
                resourceIndex = 0;
            }
            else
            {
                resourceIndex += EssentialsCore.ResourceFolderNameSize + 1;
            }
            
            this.cachedPath = assetPath.Substring(resourceIndex, extensionIndex - resourceIndex);
            return this.cachedPath;
#else
            throw new InvalidOperationException();
#endif
        }
        
        public void SetByPath(ManagedFile file)
        {
            this.Reset();

            UnityEngine.Object asset = AssetDatabase.LoadAssetAtPath<GameObject>(file.GetUnityPath());
            if(asset == null)
            {              
                UnityEngine.Debug.LogErrorFormat("Could not set Ref via path: {0}", file);
                return;
            }

            this.Resource = asset;
        }
    }
}
namespace Craiel.UnityGameData.Editor.Common
{
    using System;
    using System.Collections.Generic;
    using Builder;
    using NLog;
    using UnityEngine;
    using UnityEssentials;
    using UnityEssentials.IO;
#if UNITY_EDITOR
    using UnityEditor;
#endif

    // ---------------------------------------------------------------------------------------------
    // Base class, non generic, for use with PropertyDrawer
    // ---------------------------------------------------------------------------------------------
    [Serializable]
    public abstract class GameResourceRefBase
    {
        private static readonly NLog.Logger Logger = LogManager.GetCurrentClassLogger();

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
                Logger.Warn("StaticResourceRef did not return a valid asset path: {0}", this.Resource);
                return string.Empty;
            }

            if (this.cachedAssetPath == assetPath)
            {
                return this.cachedPath;
            }

            var path = new ManagedFile(assetPath);

            // Format the path for better processing
            path = new ManagedDirectory(path.GetDirectory().GetPathUsingDefaultSeparator()).ToFile(path.FileNameWithoutExtension);

            // Contains check first since the FindParent call is somewhat heavy
            if (path.GetPath().Contains(EssentialsCore.ResourcesFolderName))
            {
                var resourceParent = path.GetDirectory().FindParent(EssentialsCore.ResourcesFolderName);
                if (resourceParent != null)
                {
                    // This file is a resource, we only save the relative path from there
                    var relativePath = path.GetPath().Replace(resourceParent.GetPath(), string.Empty);
                    path = new ManagedFile(relativePath);
                }
            }

            this.cachedAssetPath = assetPath;
            this.cachedPath = path.GetUnityPath();

            return this.cachedPath;
#else
            throw new InvalidOperationException();
#endif
        }
    }
}
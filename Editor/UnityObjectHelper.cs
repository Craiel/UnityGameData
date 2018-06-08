using ManagedFile = Craiel.UnityEssentials.Runtime.IO.ManagedFile;

namespace Craiel.UnityGameData.Editor
{
    using System;
    using UnityEditor;
    using UnityEngine;
    using Object = UnityEngine.Object;

    public delegate string UnityObjectNameSelectorDelegate(Object sourceObject, string path, string root);

    public delegate Texture UnityObjectIconSelectorDelegate(Object sourceObject, Type objectType);

    public static class UnityObjectHelper
    {
        private static readonly string[] AnimationFileSplitParameters = { "@" };

        // -------------------------------------------------------------------
        // Public
        // -------------------------------------------------------------------
        public static Texture DefaultIconSelector(Object sourceObject, Type objectType)
        {
            var content = EditorGUIUtility.ObjectContent(sourceObject, objectType);
            return content.image;
        }

        public static Texture TextureIconSelector(Object sourceObject, Type objectType)
        {
            return sourceObject as Texture;
        }

        public static Texture SpriteIconSelector(Object sourceObject, Type objectType)
        {
            if (sourceObject == null)
            {
                return null;
            }
            
            Texture2D texture = sourceObject as Texture2D;
            if (texture != null)
            {
                return texture;
            }

            return ((Sprite)sourceObject).texture;
        }

        public static string DefaultNameSelector(Object sourceObject, string path, string root)
        {
            return sourceObject.name;
        }

        public static string DefaultPathAndNameSelector(Object sourceObject, string path, string root)
        {
            string strippedPath = path;
            if (!string.IsNullOrEmpty(root))
            {
                strippedPath = path.Replace(root, string.Empty).Trim('/');
            }

            ManagedFile relativePath = new ManagedFile(strippedPath);
            return relativePath.GetDirectory().ToFile(sourceObject.name).GetUnityPath();
        }

        public static string AnimationNameSelector(Object sourceObject, string path, string root)
        {
            ManagedFile file = new ManagedFile(path);
            string fileName = file.FileNameWithoutExtension;
            string[] parts = fileName.Split(AnimationFileSplitParameters, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length > 0 && !parts[0].Equals(sourceObject.name, StringComparison.OrdinalIgnoreCase))
            {
                return string.Format("{0} - {1}", parts[0], sourceObject.name);
            }

            return sourceObject.name;
        }
    }
}

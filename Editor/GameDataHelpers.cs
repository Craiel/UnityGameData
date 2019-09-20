using GameDataCore = Craiel.UnityGameData.Runtime.GameDataCore;
using ManagedDirectory = Craiel.UnityEssentials.Runtime.IO.ManagedDirectory;
using UnityObjectHelper = Craiel.UnityGameData.Editor.UnityObjectHelper;

namespace Craiel.UnityGameData.Editor
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using Attributes;
    using Common;
    using UnityEditor;
    using UnityEngine;
    using UnityEssentials.Runtime;

    public static class GameDataHelpers
    {
        private static readonly IDictionary<string, Texture2D> IconCache = new Dictionary<string, Texture2D>();

        // -------------------------------------------------------------------
        // Public
        // -------------------------------------------------------------------
        public static List<GameDataObject> FindGameDataList(Type dataObjectType)
        {
            var guids = AssetDatabase.FindAssets("t:" + dataObjectType);

            if (guids.Length == 0)
            {
                return null;
            }

            var list = new List<GameDataObject>();

            foreach (var guid in guids)
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                var obj = AssetDatabase.LoadAssetAtPath(path, dataObjectType);
                list.Add((GameDataObject)obj);
            }

            return list;
        }

        public static T FindGameDataByGuid<T>(string guidToLook) where T : GameDataObject
        {
            var path = AssetDatabase.GUIDToAssetPath(guidToLook);
            if (string.IsNullOrEmpty(path))
            {
                return null;
            }

            return AssetDatabase.LoadAssetAtPath<T>(path);
        }

        public static GameDataObject FindGameDataByGuid(string guidToLook)
        {
            var path = AssetDatabase.GUIDToAssetPath(guidToLook);
            if (string.IsNullOrEmpty(path))
            {
                return null;
            }

            return AssetDatabase.LoadAssetAtPath<GameDataObject>(path);
        }

        public static GameDataObject CreateAsset(Type assetType, ManagedDirectory subFolder = null, string forceName = null, bool createUniqueIfExists = true)
        {
            ManagedDirectory directory = subFolder == null
                ? GameDataCore.GameDataPath
                : GameDataCore.GameDataPath.ToDirectory(subFolder);

            var asset = CreateScriptableObject(assetType, directory, forceName, createUniqueIfExists);

            asset.Guid = AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(asset));
            asset.Name = Path.GetFileNameWithoutExtension(AssetDatabase.GetAssetPath(asset));

            EditorUtility.SetDirty(asset);

            AssetDatabase.SaveAssets();

            return asset;
        }

        public static GameDataObject CreateScriptableObject(Type assetType, ManagedDirectory directory, string forceName = null, bool createUniqueIfExists = true)
        {
            var newObject = ScriptableObject.CreateInstance(assetType);

            string name = forceName;

            if (string.IsNullOrEmpty(name))
            {
                name = assetType.ToString().Split("."[0]).LastOrDefault();
            }

            if (!directory.Exists)
            {
                directory.Create();
            }

            var finalFilePath = directory.ToFile(name + ".asset").GetPath();

            if (createUniqueIfExists == false)
            {
                var existingAsset = AssetDatabase.LoadAssetAtPath(finalFilePath, assetType);
                if (existingAsset != null)
                {
                    return (GameDataObject)existingAsset;
                }
            }

            var newObjectPath = AssetDatabase.GenerateUniqueAssetPath(finalFilePath);

            AssetDatabase.CreateAsset(newObject, newObjectPath);

            AssetDatabase.Refresh();
            AssetDatabase.ImportAsset(newObjectPath);

            var asset = AssetDatabase.LoadAssetAtPath(newObjectPath, assetType);

            EditorUtility.SetDirty(asset);

            AssetDatabase.SaveAssets();

            return (GameDataObject)asset;
        }

        public static void DeleteAsset(GameDataObject item)
        {
            if (item == null)
            {
                return;
            }

            var path = AssetDatabase.GetAssetPath(item);

            if (string.IsNullOrEmpty(path))
            {
                return;
            }

            AssetDatabase.DeleteAsset(path);
        }

        public static Texture DefaultIconSelector(UnityEngine.Object sourceObject, Type objectType)
        {
            Type sourceObjectType = sourceObject.GetType();

            FieldInfo customIconField = sourceObjectType.GetFields()
                .FirstOrDefault(prop => Attribute.IsDefined(prop, TypeCache<GameDataIconAttribute>.Value));
            if (customIconField != null)
            {
                // Get the field value
                var value = customIconField.GetValue(sourceObject);

                // If it's a direct texture use as is
                var customIcon = value as Texture;
                if (customIcon != null)
                {
                    return customIcon;
                }

                // Check if the field is a sprite ref
                var spriteRef = value as GameResourceSpriteRef;
                if (spriteRef != null)
                {
                    // Forward to the sprite selector
                    customIcon = UnityObjectHelper.SpriteIconSelector(spriteRef.Resource, null);
                    if (customIcon != null)
                    {
                        return customIcon;
                    }
                }
            }

            // Fallback to the base icon selector
            return GetIconForBaseType(sourceObjectType);
        }

        public static Texture2D GetIconForBaseType(Type type)
        {
            return GetIcon(type.Name);
        }

        public static Texture2D GetIcon(string name)
        {
            Texture2D result;
            if (IconCache.TryGetValue(name, out result))
            {
                return result;
            }

            string path = string.Format("GameDataEditor/{0}.png", name);
            result = EditorGUIUtility.Load(path) as Texture2D;
            if (result == null)
            {
                UnityEngine.Debug.LogWarningFormat("Could not load Resource for GameData Type: {0}", path);
            }

            IconCache.Add(name, result);
            return result;
        }
    }
}
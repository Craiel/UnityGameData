namespace Assets.Scripts.Craiel.GameData.Editor
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using Attributes;
    using Common;
    using Essentials.IO;
    using UnityEditor;
    using UnityEngine;

    public static class GameDataHelpers
    {
        // -------------------------------------------------------------------
        // Public
        // -------------------------------------------------------------------
        public static List<T> FindGameDataList<T>() where T : GameDataObject
        {
            var guids = AssetDatabase.FindAssets("t:" + typeof(T));

            if (guids.Length == 0)
            {
                return null;
            }

            var list = new List<T>();

            foreach (var guid in guids)
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                var obj = AssetDatabase.LoadAssetAtPath<T>(path);
                list.Add(obj);
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

        public static T CreateAsset<T>(string subFolder = null, string nameForced = null, bool createUniqueIfExists = true) where T : GameDataObject
        {
            var obj = CreateScriptableObject<T>(GameDataCore.GameDataPath.ToDirectory(subFolder).GetPath(), nameForced, createUniqueIfExists);

            obj.Guid = AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(obj));
            obj.Name = Path.GetFileNameWithoutExtension(AssetDatabase.GetAssetPath(obj));

            EditorUtility.SetDirty(obj);

            AssetDatabase.SaveAssets();

            return obj;
        }
        
        public static T CreateScriptableObject<T>(string dirPath, string nameForced = null, bool createUniqueIfExists = true) where T : ScriptableObject
        {
            var newObject = ScriptableObject.CreateInstance<T>();
            
            string name = nameForced;

            if (string.IsNullOrEmpty(name))
            {
                name = typeof(T).ToString().Split("."[0]).LastOrDefault();
            }

            var dataDir = new CarbonDirectory(dirPath);
            if (!dataDir.Exists)
            {
                dataDir.Create();
            }

            var finalFilePath = dataDir.ToFile(name + ".asset").GetPath();

            if (createUniqueIfExists == false)
            {
                var existingAsset = AssetDatabase.LoadAssetAtPath<T>(finalFilePath);
                if (existingAsset != null)
                {
                    return existingAsset;
                }
            }
            
            var newObjectPath = AssetDatabase.GenerateUniqueAssetPath(finalFilePath);

            AssetDatabase.CreateAsset(newObject, newObjectPath);

            AssetDatabase.Refresh();
            AssetDatabase.ImportAsset(newObjectPath);

            var obj = AssetDatabase.LoadAssetAtPath<T>(newObjectPath);

            EditorUtility.SetDirty(obj);

            AssetDatabase.SaveAssets();

            return obj;
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
                .FirstOrDefault(prop => Attribute.IsDefined(prop, typeof(GameDataIconAttribute)));
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

        public static Texture GetIconForBaseType(Type type)
        {
            return EditorGUIUtility.Load(string.Format("GameDataEditor/{0}.png", type.Name)) as Texture2D;
        }
    }
}
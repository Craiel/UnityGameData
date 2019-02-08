namespace Craiel.UnityGameData.Editor
{
    using System.Reflection;
    using Attributes;
    using Common;
    using Runtime;
    using UnityEditor;
    using UnityEngine;
    using UnityEssentials.Editor.UserInterface;
    using UnityEssentials.Runtime;
    using UnityEssentials.Runtime.IO;

    [CustomPropertyDrawer (typeof (GameDataPrefabLink))]
    public class GameDataPrefabLinkEditor : GameDataPropertyEditor
    {
        // -------------------------------------------------------------------
        // Protected
        // -------------------------------------------------------------------
        protected override void DrawFull()
        {
            base.DrawFull();

            this.DrawProperties();
        }
        
        // -------------------------------------------------------------------
        // Private
        // -------------------------------------------------------------------
        private void DrawProperties()
        {
            this.DrawLinkedPrefabControls();
        }

        private void DrawLinkedPrefabControls()
        {
            var typedTarget = this.Target as GameDataPrefabLink;

            var linkAttribute = fieldInfo.GetCustomAttribute<GameDataPrefabLinkAttribute>();
            if (linkAttribute == null)
            {
                EditorGUILayout.HelpBox("Missing Prefab Link Attribute: " + fieldInfo, MessageType.Error);
                return;
            }
            
            this.DrawProperty<GameDataPrefabLink>(x => x.Ref, new GUIContent(linkAttribute.Name));
            
            var region = LayoutRegion.StartAligned(isHorizontal: true);
            
            GUILayout.Space(EditorGUIUtility.labelWidth + 10);
            if (GUILayout.Button("Check", GUILayout.Width(100)))
            {
                this.CheckPrefab(typedTarget);
            }
            else
            {
                this.CheckPrefab(typedTarget, false);
            }
            
            if (typedTarget.Ref == null || !typedTarget.Ref.IsValid())
            {
                if (GUILayout.Button("Create", GUILayout.Width(100)))
                {
                    this.CreatePrefab(typedTarget, linkAttribute);
                }
            }
            else
            {
                if (GUILayout.Button("Edit", GUILayout.Width(100)))
                {
                    this.EditPrefab(typedTarget, linkAttribute);
                }
            }
            
            region.End();
        }

        private void CheckPrefab(GameDataPrefabLink target, bool warnIfMissing = true)
        {
            ManagedFile prefabFile = this.GetPrefabFile();
            if (!prefabFile.Exists)
            {
                if (!warnIfMissing)
                {
                    return;
                }
                
                EditorUtility.DisplayDialog("Check Linked Prefab", "Prefab Missing: " + prefabFile, "OK");
            }
            else
            {
                if (target.Ref == null)
                {
                    target.Ref = new GameResourcePrefabRef();
                }

                if (!target.Ref.IsValid())
                {
                    target.Ref.SetByPath(prefabFile);
                }
            }
        }

        private void CreatePrefab(GameDataPrefabLink target, GameDataPrefabLinkAttribute attribute)
        {
            ManagedFile prefabFile = this.GetPrefabFile();
            if (prefabFile.Exists)
            {
                EditorUtility.DisplayDialog("Create Prefab", "File exists: " + prefabFile, "OK");
                return;
            }

            var prefab = new GameObject(this.GameDataParent.name + " Prefab");
            prefab.AddComponent(attribute.RootScriptType);

            ManagedFile directoryPlaceholder = prefabFile.GetDirectory().ToFile("delete.me");
            if (!prefabFile.GetDirectory().Exists)
            {
                prefabFile.GetDirectory().Create();
                directoryPlaceholder.WriteAsString("DELETE");
            }
            
            PrefabUtility.SaveAsPrefabAssetAndConnect(prefab, prefabFile.GetUnityPath(), InteractionMode.AutomatedAction);

            directoryPlaceholder.DeleteIfExists();
            
            this.CheckPrefab(target);
        }

        private void EditPrefab(GameDataPrefabLink target, GameDataPrefabLinkAttribute attribute)
        {
            if (!target.IsValid())
            {
                return;
            }
            
            PrefabUtility.InstantiatePrefab(target.Ref.Resource);
        }

        private ManagedFile GetPrefabFile()
        {
            return GameDataCore.GameDataPath
                .ToDirectory(EssentialsCore.ResourcesFolderName)
                .ToDirectory(GameDataCore.GameDataDirectoryName)
                .ToDirectory(this.GameDataParentAssetPath.GetDirectory().DirectoryNameWithoutPath)
                .ToFile(this.GameDataParentAssetPath.FileName)
                .ChangeExtension(EssentialsCore.PrefabExtension);
        }
    }
}
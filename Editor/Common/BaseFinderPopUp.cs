using ManagedDirectory = Craiel.UnityEssentials.IO.ManagedDirectory;
using Styles = Craiel.UnityEssentials.Editor.UserInterface.Styles;
using UnityObjectHelper = Craiel.UnityGameData.Editor.UnityObjectHelper;
using UnityObjectIconSelectorDelegate = Craiel.UnityGameData.Editor.UnityObjectIconSelectorDelegate;
using UnityObjectNameSelectorDelegate = Craiel.UnityGameData.Editor.UnityObjectNameSelectorDelegate;

namespace Craiel.UnityGameData.Editor.Common
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using UnityEditor;
    using UnityEditor.IMGUI.Controls;
    using UnityEngine;

    public abstract class BaseFinderPopUp<T> : EditorWindow
        where T : UnityEngine.Object
    {
        private IList<T> entries;
        private IList<string> entryNames;
        private IList<string> entryPaths;

        private SearchField filterField;
        private IList<T> selectedEntries;

        private Vector2 scrollPosition;
        private string filter;
        private bool refreshRequired = true;
        private bool enabled;

        // -------------------------------------------------------------------
        // Constructor
        // -------------------------------------------------------------------
        protected BaseFinderPopUp()
        {
            this.ObjectType = typeof(UnityEngine.Object);
            this.TypeFilter = this.ObjectType.Name;
            this.NameSelector = UnityObjectHelper.DefaultNameSelector;
            this.IconSelector = UnityObjectHelper.DefaultIconSelector;
        }

        // -------------------------------------------------------------------
        // Public
        // -------------------------------------------------------------------
        public bool AllowMultiSelect { get; protected set; }

        public void OnEnable()
        {
            this.entries = new List<T>();
            this.entryNames = new List<string>();
            this.entryPaths = new List<string>();
            this.selectedEntries = new List<T>();
            this.filterField = new SearchField();

            this.filter = string.Empty;
            this.filterField.SetFocus();

            this.enabled = true;
        }

        public void OnDisable()
        {
            this.enabled = false;
        }

        public void Update()
        {
            if (this.refreshRequired)
            {
                this.entries.Clear();
                this.entryNames.Clear();
                this.entryPaths.Clear();

                string[] candidateGuids;
                if (this.Root == null)
                {
                    candidateGuids = AssetDatabase.FindAssets("t:" + this.TypeFilter);
                }
                else
                {
                    candidateGuids = AssetDatabase.FindAssets("t:" + this.TypeFilter, new[] {this.Root.GetUnityPath()});
                }

                if (this.TypeFilter.Equals("Animation", StringComparison.OrdinalIgnoreCase) ||
                    this.TypeFilter.Equals("AnimationClip", StringComparison.OrdinalIgnoreCase))
                {
                    RefreshAssetsAnimationSpecial(candidateGuids);
                }

                RefreshAssets(candidateGuids);
            }
        }

        public void OnGUI()
        {
            if (!this.enabled)
            {
                return;
            }

            GUILayout.BeginVertical("box");

            if (!this.DrawTopButtons())
            {
                return;
            }

            GUILayout.Space(5);

            this.filter = this.filterField.OnToolbarGUI(this.filter);

            GUILayout.Space(5);

            if (!this.DrawScrollView())
            {
                return;
            }

            if (!this.DrawBottomButtons())
            {
                return;
            }


            GUILayout.EndVertical();
        }

        // -------------------------------------------------------------------
        // Protected
        // -------------------------------------------------------------------
        protected Type ObjectType { get; set; }

        protected string TypeFilter { get; set; }

        protected ManagedDirectory Root { get; set; }

        protected UnityObjectNameSelectorDelegate NameSelector { get; set; }

        protected UnityObjectIconSelectorDelegate IconSelector { get; set; }

        protected abstract bool RefreshEntry(string path, out T entry);
        
        protected abstract void Select(IList<T> entries);

        protected virtual void SelectEntry(T entry)
        {
            if (!this.selectedEntries.Contains(entry))
            {
                this.selectedEntries.Add(entry);
            }

            if (!this.AllowMultiSelect)
            {
                this.EndSelection();
            }
        }

        // -------------------------------------------------------------------
        // Private
        // -------------------------------------------------------------------
        private void RefreshAssets(string[] guids)
        {
            foreach (string guid in guids)
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);

                T asset;
                if (this.RefreshEntry(path, out asset))
                {
                    this.entries.Add(asset);
                    this.entryNames.Add(this.NameSelector(asset, path
                        ,
                        this.Root == null ? string.Empty : this.Root.GetUnityPath()));
                    this.entryPaths.Add(path);
                }
            }
        }

        private void RefreshAssetsAnimationSpecial(string[] guids)
        {
            var uniqueAnimationAssets = new HashSet<string>();
            foreach (string guid in guids)
            {
                uniqueAnimationAssets.Add(guid);
            }

            foreach (string guid in uniqueAnimationAssets)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                var model = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                if (model == null)
                {
                    continue;
                }

                IList<AnimationClip> clips = AssetDatabase.LoadAllAssetsAtPath(path)
                    .Where(x => x.GetType() == typeof(AnimationClip))
                    .Cast<AnimationClip>()
                    .ToList();

                foreach (AnimationClip clip in clips)
                {
                    this.entries.Add((T)(UnityEngine.Object)clip);
                    this.entryNames.Add(this.NameSelector(clip, path,
                        this.Root == null ? string.Empty : this.Root.GetUnityPath()));
                    this.entryPaths.Add(path);
                }
            }
        }

        private void ToggleEntry(T entry)
        {
            if (this.AllowMultiSelect && this.selectedEntries.Contains(entry))
            {
                this.selectedEntries.Remove(entry);
                return;
            }

            this.SelectEntry(entry);
        }

        private bool DrawTopButtons()
        {
            GUILayout.BeginHorizontal();
            if (this.AllowMultiSelect)
            {
                if (GUILayout.Button("Clear", GUILayout.Height(30)))
                {
                    this.selectedEntries.Clear();
                    return false;
                }
            }
            else
            {
                if (GUILayout.Button("Set to Null", GUILayout.Height(30)))
                {
                    this.EndSelection();
                    return false;
                }
            }

            if (GUILayout.Button("Refresh", GUILayout.Height(30), GUILayout.Width(60)))
            {
                this.refreshRequired = true;
                return false;
            }

            GUILayout.EndHorizontal();
            return true;
        }

        private bool DrawScrollView()
        {
            bool acceptFirstEntry = false;
            if (Event.current.type == EventType.KeyUp
                && Event.current.keyCode == KeyCode.Return
                && this.filterField.HasFocus())
            {
                acceptFirstEntry = true;
            }

            GameDataStyles.BeginStyle(GameDataStyles.FinderSkin);

            this.scrollPosition = GUILayout.BeginScrollView(this.scrollPosition);
            for (var i = 0; i < this.entries.Count; i++)
            {
                if (!string.IsNullOrEmpty(this.filter))
                {
                    if (!this.entryNames[i].ToLowerInvariant().Contains(this.filter.ToLowerInvariant()))
                    {
                        continue;
                    }
                }

                if (acceptFirstEntry)
                {
                    this.SelectEntry(this.entries[i]);
                }

                if (this.AllowMultiSelect && this.selectedEntries.Contains(this.entries[i]))
                {
                    GUI.backgroundColor = Styles.DefaulEditortBackgroundColor;
                }
                else
                {
                    GUI.backgroundColor = Styles.DefaulEditortSelectedTextColor;
                }

                GUIContent content = new GUIContent(this.entryNames[i], this.IconSelector(this.entries[i], this.ObjectType), this.entryPaths[i]);
                if (GUILayout.Button(content, GUILayout.Height(30)))
                {
                    this.ToggleEntry(this.entries[i]);
                }

                GUI.backgroundColor = Styles.DefaulEditortSelectedTextColor;
            }

            GUILayout.EndScrollView();

            GameDataStyles.EndStyle();

            return true;
        }

        private bool DrawBottomButtons()
        {
            GUILayout.BeginHorizontal();

            if (this.AllowMultiSelect && this.selectedEntries.Count > 0)
            {
                if (GUILayout.Button(string.Format("Select {0} Entries", this.selectedEntries.Count), GUILayout.Height(30)))
                {
                    this.EndSelection();
                    return false;
                }
            }

            GUILayout.EndHorizontal();
            return true;
        }

        private void EndSelection()
        {
            this.enabled = false;
            this.Select(this.selectedEntries);
            this.Close();
        }
    }
}

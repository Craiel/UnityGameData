namespace Craiel.UnityGameData.Editor.Common
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Enums;
    using UnityEditor;
    using UnityEditor.IMGUI.Controls;
    using UnityEngine;
    using UnityEssentials.Editor.UserInterface;
    using UnityEssentials.Runtime;
    using UnityEssentials.Runtime.IO;
    using UnityEssentials.Runtime.Utils;

    public abstract class BaseFinderPopUp<T> : EditorWindow
        where T : UnityEngine.Object
    {
        private const int CardSize = 80;
        private const int CardMaxLabelSize = 8;

        private IList<T> entries;
        private IList<string> entryNames;
        private IList<string> entryNamesInvariant;
        private IList<string> entryPaths;

        private SearchField filterField;
        private IList<T> selectedEntries;

        private float scrollPosition;
        private string filter;
        private string filterInvariant;
        private bool refreshRequired = true;
        private bool enabled;

        private int lastFilteredCount;

        private int cardsPerRow = 4;
        private int cardsPerColumn = 4;
        private int maxEntriesDisplayedInDefault = 10;
        private int maxEntriesDisplayedInCards = 13;

        private Vector2 finderContentArea;

        private GUIStyle defaultButtonStyle;
        private GUIStyle cardButtonStyle;

        // -------------------------------------------------------------------
        // Constructor
        // -------------------------------------------------------------------
        protected BaseFinderPopUp()
        {
            this.ObjectType = TypeCache<UnityEngine.Object>.Value;
            this.TypeFilters = new []{this.ObjectType.Name};
            this.NameSelector = UnityObjectHelper.DefaultNameSelector;
            this.IconSelector = UnityObjectHelper.DefaultIconSelector;
            this.ContentMargin = new Vector2(120, 50);
        }

        // -------------------------------------------------------------------
        // Public
        // -------------------------------------------------------------------
        public bool AllowMultiSelect { get; protected set; }

        public FinderPopUpStyle Style { get; protected set; }

        public void OnEnable()
        {
            this.entries = new List<T>();
            this.entryNames = new List<string>();
            this.entryNamesInvariant = new List<string>();
            this.entryPaths = new List<string>();
            this.selectedEntries = new List<T>();
            this.filterField = new SearchField();

            this.filter = string.Empty;
            this.filterInvariant = string.Empty;
            this.filterField.SetFocus();

            this.enabled = true;

            this.finderContentArea = GameDataStyles.FinderPopupSize - this.ContentMargin;

            this.cardsPerRow = Mathf.FloorToInt(finderContentArea.x / CardSize);
            this.cardsPerColumn = Mathf.FloorToInt(this.finderContentArea.y / (CardSize + 20f));

            this.maxEntriesDisplayedInDefault = Mathf.FloorToInt(this.finderContentArea.x / 30f);
            this.maxEntriesDisplayedInCards = 1 + (this.cardsPerRow * this.cardsPerColumn);
        }

        public void OnDisable()
        {
            this.enabled = false;
        }

        public void Update()
        {
            if (this.refreshRequired)
            {
                this.DoRefresh();
            }
        }

        public void OnGUI()
        {
            if (!this.enabled)
            {
                return;
            }

            if (this.cardButtonStyle == null)
            {
                this.cardButtonStyle = new GUIStyle(GUI.skin.button)
                {
                    alignment = TextAnchor.MiddleCenter,
                    fixedWidth = CardSize,
                    fixedHeight = CardSize
                };
            }

            if (this.defaultButtonStyle == null)
            {
                this.defaultButtonStyle = new GUIStyle(GUI.skin.button)
                {
                    fixedWidth = GameDataStyles.FinderPopupWidth - 60,
                    fixedHeight = 30,
                    alignment = TextAnchor.MiddleLeft
                };
            }

            if (this.HandleInput())
            {
                return;
            }

            GUILayout.BeginVertical("box");

            if (!this.DrawTopButtons())
            {
                return;
            }

            this.DrawCustomOptions();

            GUILayout.Space(5);

            this.filter = this.filterField.OnToolbarGUI(this.filter);
            this.filterInvariant = this.filter.ToLowerInvariant();

            GUILayout.Space(5);

            if (!this.DrawScrollView())
            {
                return;
            }

            if (!this.DrawBottomButtons())
            {
                return;
            }

            this.DrawStatusBar();

            GUILayout.EndVertical();
        }

        // -------------------------------------------------------------------
        // Protected
        // -------------------------------------------------------------------
        protected Type ObjectType { get; set; }

        protected string[] TypeFilters { get; set; }

        protected ManagedDirectory Root { get; set; }

        protected UnityObjectNameSelectorDelegate NameSelector { get; set; }

        protected UnityObjectIconSelectorDelegate IconSelector { get; set; }

        protected Vector2 ContentMargin { get; set; }

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

        protected virtual void DrawCustomOptions()
        {
        }

        protected void ForceRefresh()
        {
            this.refreshRequired = true;
        }

        // -------------------------------------------------------------------
        // Private
        // -------------------------------------------------------------------
        private void DoRefresh()
        {
            this.refreshRequired = false;

            this.entries.Clear();
            this.entryNames.Clear();
            this.entryNamesInvariant.Clear();
            this.entryPaths.Clear();

            IList<string> lookupStrings = new List<string>();
            foreach (string typeFilter in this.TypeFilters)
            {
                lookupStrings.Add(string.Concat("t: ", typeFilter));
            }

            string lookupString = string.Join(" ", lookupStrings);

            string[] candidateGuids;
            if (this.Root == null)
            {
                candidateGuids = AssetDatabase.FindAssets(lookupString);
            }
            else
            {
                candidateGuids = AssetDatabase.FindAssets(lookupString, new[] {this.Root.GetUnityPath()});
            }

            if (candidateGuids == null || candidateGuids.Length == 0)
            {
                return;
            }

            if (this.TypeFilters.Contains("Animation") ||
                this.TypeFilters.Contains("AnimationClip"))
            {
                RefreshAssetsAnimationSpecial(candidateGuids);
            }
            else
            {
                RefreshAssets(candidateGuids);
            }
        }

        private void RefreshAssets(string[] guids)
        {
            foreach (string guid in guids)
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);

                T asset;
                if (this.RefreshEntry(path, out asset))
                {
                    this.entries.Add(asset);
                    string assetName = this.NameSelector(asset, path
                        ,
                        this.Root == null ? string.Empty : this.Root.GetUnityPath());
                    this.entryNames.Add(assetName);
                    this.entryNamesInvariant.Add(assetName.ToLowerInvariant());
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
                    .Where(x => x.GetType() == TypeCache<AnimationClip>.Value)
                    .Cast<AnimationClip>()
                    .ToList();

                foreach (AnimationClip clip in clips)
                {
                    this.entries.Add((T)(UnityEngine.Object)clip);
                    string assetName = this.NameSelector(clip, path,
                        this.Root == null ? string.Empty : this.Root.GetUnityPath());
                    this.entryNames.Add(assetName);
                    this.entryNamesInvariant.Add(assetName.ToLowerInvariant());
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

        private void DrawStatusBar()
        {
            GUILayout.FlexibleSpace();

            string label;
            if (string.IsNullOrEmpty(this.filterInvariant))
            {
                label = string.Format("{0} Assets", this.entries.Count);
            }
            else
            {
                label = string.Format("{0} / {1} Assets", this.lastFilteredCount, this.entries.Count);
            }

            GUILayout.Box(label, GUILayout.ExpandWidth(true));
        }

        private void DrawScrollViewEntry(T entry, string entryName, string entryPath)
        {
            if (this.AllowMultiSelect && this.selectedEntries.Contains(entry))
            {
                GUI.backgroundColor = Styles.DefaulEditortBackgroundColor;
            }
            else
            {
                GUI.backgroundColor = Styles.DefaulEditortSelectedTextColor;
            }

            switch (this.Style)
            {
                case FinderPopUpStyle.Default:
                {
                    GUIContent content = new GUIContent(entryName, this.IconSelector(entry, this.ObjectType), entryPath);
                    if (GUILayout.Button(content, this.defaultButtonStyle))
                    {
                        this.ToggleEntry(entry);
                    }

                    break;
                }

                case FinderPopUpStyle.Cards:
                {
                    GUILayout.BeginVertical();
                    GUIContent content = new GUIContent(string.Empty, this.IconSelector(entry, this.ObjectType), entryPath);
                    if (GUILayout.Button(content, this.cardButtonStyle))
                    {
                        this.ToggleEntry(entry);
                    }

                    if (entryName.Length > CardMaxLabelSize)
                    {
                        entryName = entryName.Substring(0, CardMaxLabelSize) + " ...";
                    }

                    GUILayout.Label(entryName);
                    GUILayout.EndVertical();

                    break;
                }
            }

            GUI.backgroundColor = Styles.DefaulEditortSelectedTextColor;
        }

        private bool DrawScrollView()
        {
            bool acceptFirstEntry = Event.current.type == EventType.KeyUp
                                    && Event.current.keyCode == KeyCode.Return
                                    && this.filterField.HasFocus();

            IList<int> filteredEntryIndizes = new List<int>();
            for (var i = 0; i < this.entryNamesInvariant.Count; i++)
            {
                if (!this.entryNamesInvariant[i].Contains(this.filterInvariant))
                {
                    continue;
                }

                filteredEntryIndizes.Add(i);
            }

            this.lastFilteredCount = filteredEntryIndizes.Count;

            GameDataStyles.BeginStyle(GameDataStyles.FinderSkin);

            EditorGUILayout.BeginHorizontal();

            EditorGUILayout.BeginVertical();

            int maxEntriesDisplayed = this.maxEntriesDisplayedInDefault;
            switch (this.Style)
            {
                case FinderPopUpStyle.Cards:
                {
                    EditorGUILayout.BeginHorizontal();
                    maxEntriesDisplayed = this.maxEntriesDisplayedInCards;
                    break;
                }
            }

            int rowCount = 0;
            int startPosition = (int) this.scrollPosition.Clamp(0, filteredEntryIndizes.Count);
            for (int i = startPosition; i < startPosition + maxEntriesDisplayed - 1; i++)
            {
                if (i >= filteredEntryIndizes.Count)
                {
                    break;
                }

                int entryIndex = filteredEntryIndizes[i];
                T entry = this.entries[entryIndex];

                if (acceptFirstEntry)
                {
                    this.SelectEntry(entry);
                    break;
                }

                string entryName = this.entryNames[entryIndex];
                string entryPath = this.entryPaths[entryIndex];

                switch (this.Style)
                {
                    case FinderPopUpStyle.Default:
                    {
                        this.DrawScrollViewEntry(entry, entryName, entryPath);
                        break;
                    }

                    case FinderPopUpStyle.Cards:
                    {
                        this.DrawScrollViewEntry(entry, entryName, entryPath);
                        rowCount++;
                        if (rowCount >= this.cardsPerRow)
                        {
                            EditorGUILayout.EndHorizontal();
                            GUILayout.Space(2);
                            EditorGUILayout.BeginHorizontal();
                            rowCount = 0;
                        }

                        break;
                    }
                }
            }

            switch (this.Style)
            {
                case FinderPopUpStyle.Cards:
                {
                    EditorGUILayout.EndHorizontal();
                    break;
                }
            }

            EditorGUILayout.EndVertical();

            GUILayout.FlexibleSpace();

            if (this.scrollPosition > filteredEntryIndizes.Count)
            {
                // Reset the scroll position if we are past the content
                this.scrollPosition = 0;
            }

            if (filteredEntryIndizes.Count > maxEntriesDisplayed)
            {
                this.scrollPosition = GUILayout.VerticalScrollbar(this.scrollPosition, maxEntriesDisplayed, 0, filteredEntryIndizes.Count, GUILayout.ExpandHeight(true));
            }

            EditorGUILayout.EndHorizontal();

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

        private bool HandleInput()
        {
            int maxEntriesDisplayed = this.maxEntriesDisplayedInDefault - 1;
            switch (this.Style)
            {
                case FinderPopUpStyle.Cards:
                {
                    maxEntriesDisplayed = this.maxEntriesDisplayedInCards - 1;
                    break;
                }
            }

            if (this.entries.Count <= maxEntriesDisplayed)
            {
                return false;
            }

            if (Event.current.rawType == EventType.ScrollWheel)
            {
                if (Event.current.delta.y > 0)
                {
                    this.scrollPosition += maxEntriesDisplayed;
                }
                else
                {
                    if (this.scrollPosition > 0)
                    {
                        this.scrollPosition -= maxEntriesDisplayed;
                    }
                }

                this.Repaint();
                return true;
            }

            return false;
        }
    }
}

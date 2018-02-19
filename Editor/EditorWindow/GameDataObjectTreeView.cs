namespace Assets.Scripts.Craiel.GameData.Editor.EditorWindow
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Common;
    using Editor;
    using UnityEditor.IMGUI.Controls;
    using UnityEngine;
    using UnityEngine.Events;

    public class GameDataObjectTreeView : TreeView
    {
        private readonly Type dataObjectType;
        
        private TreeViewItem root;

        // -------------------------------------------------------------------
        // Constructor
        // -------------------------------------------------------------------
        public GameDataObjectTreeView(TreeViewState state, Type dataObjectType)
            : base(state)
        {
            this.dataObjectType = dataObjectType;
            this.SelectedData = new List<GameDataObject>();
            this.OnSelectionChanged = new UnityEvent();
            this.showAlternatingRowBackgrounds = true;
            this.Reload();
        }

        // -------------------------------------------------------------------
        // Public
        // -------------------------------------------------------------------
        public IList<GameDataObject> Data { get; private set; }

        public IList<GameDataObject> SelectedData { get; private set; }

        public UnityEvent OnSelectionChanged { get; private set; }

        public void SelectFirstItem()
        {
            if (this.Data != null && this.Data.Count > 0)
            {
                var selectedIndices = new List<int> { 0 };
                this.SetSelection(selectedIndices);
                this.SelectionChanged(selectedIndices);
                this.SetFocusAndEnsureSelectedItem();
            }
        }

        public void SelectItem(GameDataObject item)
        {
            if (this.Data != null && this.Data.Count > 0)
            {
                for (int i = 0; i < this.Data.Count; i++)
                {
                    if (this.Data[i] == item)
                    {
                        var selectedIndices = new List<int> { i };
                        this.SetSelection(selectedIndices);
                        this.SelectionChanged(selectedIndices);
                        this.SetFocusAndEnsureSelectedItem();
                        return;
                    }
                }
            }
        }
        
        public void SelectItemByGuid(string itemGuid)
        {
            if (this.Data != null)
            {
                for (int i = 0; i < this.Data.Count; i++)
                {
                    var obj = this.GetObject(i);
                    if (itemGuid == obj.Guid)
                    {
                        this.SelectItem(obj);
                        break;
                    }
                }
            }
        }
        
        public void SelectItemByObject(object target)
        {
            if (this.Data != null)
            {
                for (int i = 0; i < this.Data.Count; i++)
                {
                    var entry = this.GetObject(i);
                    if (ReferenceEquals(entry, target))
                    {
                        this.SelectItem(entry);
                        break;
                    }
                }
            }
        }

        // -------------------------------------------------------------------
        // Protected
        // -------------------------------------------------------------------
        protected override TreeViewItem BuildRoot()
        {
            this.root = new TreeViewItem {id = 0, depth = -1, displayName = "Root"};

            var entries = GameDataHelpers.FindGameDataList(this.dataObjectType);
            this.Data = entries != null ? entries.OrderBy(x => x.Name).ToList() : null;

            var rows = new List<TreeViewItem>();
            rows.Clear();

            if (this.Data == null || this.Data.Count == 0)
            {
            }
            else
            {
                for (var i = 0; i < this.Data.Count; i++)
                {
                    var d = this.Data[i];

                    if (d == null)
                    {
                        Debug.LogError("Null data. Check the stadata data editor window.");
                        continue;
                    }

                    var item = new TreeViewItem(i, -1, d.Name);

                    rows.Add(item);
                }
            }
            
            SetupParentsAndChildrenFromDepths(this.root, rows);
            
            return this.root;
        }
        
        protected override void RowGUI(RowGUIArgs args)
        {
            var obj = this.GetObject(args.item.id);

            if (obj == null)
            {
                return;
            }

            args.rowRect.x += 5;

            args.label = obj.Name;

            base.RowGUI(args);
        }
        
        protected override void SelectionChanged(IList<int> selectedIds)
        {
            this.SelectedData.Clear();
            foreach (var index in selectedIds)
            {
                this.SelectedData.Add(this.Data[index]);
            }

            this.OnSelectionChanged.Invoke();
        }

        // -------------------------------------------------------------------
        // Private
        // -------------------------------------------------------------------
        private GameDataObject GetObject(int index)
        {
            return this.Data[index];
        }
    }
}
namespace Assets.Scripts.Craiel.GameData.Editor.EditorWindow
{
    using System.Collections.Generic;
    using System.Linq;
    using Common;
    using Editor;
    using UnityEditor.IMGUI.Controls;
    using UnityEngine;
    using UnityEngine.Events;

    public class GameDataObjectTreeView<T> : TreeView where T : GameDataObject
    {
        private TreeViewItem root;

        // -------------------------------------------------------------------
        // Constructor
        // -------------------------------------------------------------------
        public GameDataObjectTreeView(TreeViewState state)
            : base(state)
        {
            this.SelectedData = new List<T>();
            this.OnSelectionChanged = new UnityEvent();
            this.showAlternatingRowBackgrounds = true;
            this.Reload();
        }

        // -------------------------------------------------------------------
        // Public
        // -------------------------------------------------------------------
        [SerializeField]
        public List<T> Data;

        [SerializeField]
        public readonly List<T> SelectedData;

        [SerializeField]
        public readonly UnityEvent OnSelectionChanged;

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

        public void SelectItem(T item)
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

        // -------------------------------------------------------------------
        // Protected
        // -------------------------------------------------------------------
        protected override TreeViewItem BuildRoot()
        {
            this.root = new TreeViewItem {id = 0, depth = -1, displayName = "Root"};

            var stuff = GameDataHelpers.FindGameDataList<T>();

            if (stuff != null)
            {
                this.Data = stuff.OrderBy(x => x.Name).ToList();
            }
            else
            {
                this.Data = null;
            }

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
        private T GetObject(int index)
        {
            return this.Data[index];
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
    }
}
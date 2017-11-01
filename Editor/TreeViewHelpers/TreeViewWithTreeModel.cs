namespace Assets.Scripts.Craiel.GameData.Editor.TreeViewHelpers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using UnityEditor;
    using UnityEditor.IMGUI.Controls;
    using UnityEngine;

    internal class TreeViewItem<T> : TreeViewItem where T : TreeElement
    {
        public T data { get; set; }

        public TreeViewItem (int id, int depth, string displayName, T data) : base (id, depth, displayName)
        {
            this.data = data;
        }
    }

    internal class TreeViewWithTreeModel<T> : TreeView where T : TreeElement
    {
        TreeModel<T> m_TreeModel;
        readonly List<TreeViewItem> m_Rows = new List<TreeViewItem>(100);
        public event Action treeChanged;

        public TreeModel<T> treeModel { get { return this.m_TreeModel; } }
        public event Action<IList<TreeViewItem>>  beforeDroppingDraggedItems;


        public TreeViewWithTreeModel (TreeViewState state, TreeModel<T> model) : base (state)
        {
            this.Init (model);
        }

        public TreeViewWithTreeModel (TreeViewState state, MultiColumnHeader multiColumnHeader, TreeModel<T> model)
            : base(state, multiColumnHeader)
        {
            this.Init (model);
        }

        void Init (TreeModel<T> model)
        {
            this.m_TreeModel = model;
            this.m_TreeModel.modelChanged += this.ModelChanged;
        }

        void ModelChanged ()
        {
            if (this.treeChanged != null)
                this.treeChanged ();

            this.Reload ();
        }

        protected override TreeViewItem BuildRoot()
        {
            int depthForHiddenRoot = -1;
            return new TreeViewItem<T>(this.m_TreeModel.root.id, depthForHiddenRoot, this.m_TreeModel.root.name, this.m_TreeModel.root);
        }

        protected override IList<TreeViewItem> BuildRows (TreeViewItem root)
        {
            if (this.m_TreeModel.root == null)
            {
                Debug.LogError ("tree model root is null. did you call SetData()?");
            }

            this.m_Rows.Clear ();
            if (!string.IsNullOrEmpty(this.searchString))
            {
                this.Search (this.m_TreeModel.root, this.searchString, this.m_Rows);
            }
            else
            {
                if (this.m_TreeModel.root.hasChildren)
                    this.AddChildrenRecursive(this.m_TreeModel.root, 0, this.m_Rows);
            }

            // We still need to setup the child parent information for the rows since this 
            // information is used by the TreeView internal logic (navigation, dragging etc)
            SetupParentsAndChildrenFromDepths (root, this.m_Rows);

            return this.m_Rows;
        }

        void AddChildrenRecursive (T parent, int depth, IList<TreeViewItem> newRows)
        {
            foreach (T child in parent.children)
            {
                var item = new TreeViewItem<T>(child.id, depth, child.name, child);
                newRows.Add(item);

                if (child.hasChildren)
                {
                    if (this.IsExpanded(child.id))
                    {
                        this.AddChildrenRecursive (child, depth + 1, newRows);
                    }
                    else
                    {
                        item.children = CreateChildListForCollapsedParent();
                    }
                }
            }
        }

        void Search(T searchFromThis, string search, List<TreeViewItem> result)
        {
            if (string.IsNullOrEmpty(search))
                throw new ArgumentException("Invalid search: cannot be null or empty", "search");

            const int kItemDepth = 0; // tree is flattened when searching

            Stack<T> stack = new Stack<T>();
            foreach (var element in searchFromThis.children)
                stack.Push((T)element);
            while (stack.Count > 0)
            {
                T current = stack.Pop();
                // Matches search?
                if (current.name.IndexOf(search, StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    result.Add(new TreeViewItem<T>(current.id, kItemDepth, current.name, current));
                }

                if (current.children != null && current.children.Count > 0)
                {
                    foreach (var element in current.children)
                    {
                        stack.Push((T)element);
                    }
                }
            }
            this.SortSearchResult(result);
        }

        protected virtual void SortSearchResult (List<TreeViewItem> rows)
        {
            rows.Sort ((x,y) => EditorUtility.NaturalCompare (x.displayName, y.displayName)); // sort by displayName by default, can be overriden for multicolumn solutions
        }
    
        protected override IList<int> GetAncestors (int id)
        {
            return this.m_TreeModel.GetAncestors(id);
        }

        protected override IList<int> GetDescendantsThatHaveChildren (int id)
        {
            return this.m_TreeModel.GetDescendantsThatHaveChildren(id);
        }


        // Dragging
        //-----------
    
        const string k_GenericDragID = "GenericDragColumnDragging";

        protected override bool CanStartDrag (CanStartDragArgs args)
        {
            return true;
        }

        protected override void SetupDragAndDrop(SetupDragAndDropArgs args)
        {
            if (this.hasSearch)
                return;

            DragAndDrop.PrepareStartDrag();
            var draggedRows = this.GetRows().Where(item => args.draggedItemIDs.Contains(item.id)).ToList();
            DragAndDrop.SetGenericData(k_GenericDragID, draggedRows);
            DragAndDrop.objectReferences = new UnityEngine.Object[] { }; // this IS required for dragging to work
            string title = draggedRows.Count == 1 ? draggedRows[0].displayName : "< Multiple >";
            DragAndDrop.StartDrag (title);
        }

        protected override DragAndDropVisualMode HandleDragAndDrop (DragAndDropArgs args)
        {
            // Check if we can handle the current drag data (could be dragged in from other areas/windows in the editor)
            var draggedRows = DragAndDrop.GetGenericData(k_GenericDragID) as List<TreeViewItem>;
            if (draggedRows == null)
                return DragAndDropVisualMode.None;

            // Parent item is null when dragging outside any tree view items.
            switch (args.dragAndDropPosition)
            {
                case DragAndDropPosition.UponItem:
                case DragAndDropPosition.BetweenItems:
                    {
                        bool validDrag = this.ValidDrag(args.parentItem, draggedRows);
                        if (args.performDrop && validDrag)
                        {
                            T parentData = ((TreeViewItem<T>)args.parentItem).data;
                            this.OnDropDraggedElementsAtIndex(draggedRows, parentData, args.insertAtIndex == -1 ? 0 : args.insertAtIndex);
                        }
                        return validDrag ? DragAndDropVisualMode.Move : DragAndDropVisualMode.None;
                    }

                case DragAndDropPosition.OutsideItems:
                    {
                        if (args.performDrop)
                            this.OnDropDraggedElementsAtIndex(draggedRows, this.m_TreeModel.root, this.m_TreeModel.root.children.Count);

                        return DragAndDropVisualMode.Move;
                    }
                default:
                    Debug.LogError("Unhandled enum " + args.dragAndDropPosition);
                    return DragAndDropVisualMode.None;
            }
        }

        public virtual void OnDropDraggedElementsAtIndex (List<TreeViewItem> draggedRows, T parent, int insertIndex)
        {
            if (this.beforeDroppingDraggedItems != null)
                this.beforeDroppingDraggedItems (draggedRows);

            var draggedElements = new List<TreeElement> ();
            foreach (var x in draggedRows)
                draggedElements.Add (((TreeViewItem<T>) x).data);
        
            var selectedIDs = draggedElements.Select (x => x.id).ToArray();
            this.m_TreeModel.MoveElements (parent, insertIndex, draggedElements);
            this.SetSelection(selectedIDs, TreeViewSelectionOptions.RevealAndFrame);
        }


        bool ValidDrag(TreeViewItem parent, List<TreeViewItem> draggedItems)
        {
            TreeViewItem currentParent = parent;
            while (currentParent != null)
            {
                if (draggedItems.Contains(currentParent))
                    return false;
                currentParent = currentParent.parent;
            }
            return true;
        }
    
    }

}

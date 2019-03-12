namespace Craiel.UnityGameData.Editor.Window
{
    using System.Collections.Generic;
    using System.Linq;
    using Common;
    using GameData.Editor.Events;
    using UnityEditor;
    using UnityEditor.IMGUI.Controls;
    using UnityEngine;
    using UnityEssentials.Editor.UserInterface;
    using UnityEssentials.Runtime.Collections;
    using UnityEssentials.Runtime.Event.Editor;
    using UnityEssentials.Runtime.Utils;

    public class GameDataTreeContentPresenter : TreeView, IGameDataContentPresenter
    {
        private const float MinWidth = 200;
        private const float MaxWidth = 600;
        
        private readonly SearchField searchField;
        private readonly ExtendedDictionary<int, GameDataObject> idObjectMap;
        private readonly IList<int> selection;
        private readonly IList<TreeViewItem> treeViewEntryTempList;
        private readonly IList<GameDataObject> entryTempList;
        
        private TreeViewItem root;
        private int nextEntryId;

        private Vector2 scrollPos;
        private float treeViewWidth = MinWidth + 100;

        private SerializedObject copyObject;

        private GameDataEditorContent activeContent;

        private Editor currentEditor;

        private bool createOnNextRepaint;

        private bool isSplitterDragging;
        
        private GUIStyle toolBarStyle;
        
        // -------------------------------------------------------------------
        // Constructor
        // -------------------------------------------------------------------
        public GameDataTreeContentPresenter() 
            : base(new TreeViewState())
        {
            this.searchField = new SearchField();
            this.selection = new List<int>();
            this.idObjectMap = new ExtendedDictionary<int, GameDataObject> {EnableReverseLookup = true};
            this.treeViewEntryTempList = new List<TreeViewItem>();
            this.entryTempList = new List<GameDataObject>();

            this.showAlternatingRowBackgrounds = true;
        }
        
        // -------------------------------------------------------------------
        // Public
        // -------------------------------------------------------------------
        public void Draw(Rect drawArea, GameDataEditorContent content)
        {
            if (this.toolBarStyle == null)
            {
                this.toolBarStyle = new GUIStyle(GameDataEditorWindow.Instance.ToolBarStyle)
                {
                    fixedWidth = 32, 
                    fixedHeight = 32
                };
            }
            
            Rect splitterRect;
            
            if (this.activeContent != content)
            {
                this.activeContent = content;
                this.Reload();
                this.RebuildEditor();
            }
            
            if (this.createOnNextRepaint)
            {
                this.createOnNextRepaint = false;
                this.OpenCreateItemDialog(content);
            }

            EditorGUILayout.BeginHorizontal();
            {
                // Left
                EditorGUILayout.BeginVertical("box", GUILayout.Width(this.treeViewWidth), GUILayout.ExpandHeight(true));
                {
                    GUILayout.Space(5);
                    this.DrawToolBar(content);

                    GUILayout.Space(5);
                    this.searchString = this.searchField.OnGUI(this.searchString);
                    GUILayout.Space(5);
                    var rect = GUILayoutUtility.GetRect(this.treeViewWidth, this.treeViewWidth, 0, 100000);

                    this.OnGUI(rect);
                }

                EditorGUILayout.EndVertical();
                
                GUILayout.Box ("", 
                    GUILayout.Width(2), 
                    GUILayout.MaxWidth (2), 
                    GUILayout.MinWidth(2),
                    GUILayout.ExpandHeight(true));
                splitterRect = GUILayoutUtility.GetLastRect ();
                EditorGUIUtility.AddCursorRect(splitterRect, MouseCursor.ResizeHorizontal);

                // Right
                EditorGUILayout.BeginVertical("box");
                {
                    this.scrollPos = EditorGUILayout.BeginScrollView(this.scrollPos);

                    if (this.currentEditor != null)
                    {
                        Layout.SetExtendedLabelSize();
                        EditorGUI.BeginChangeCheck();
                        this.currentEditor.OnInspectorGUI();

                        Layout.SetDefaultLabelSize();
                    }

                    EditorGUILayout.EndScrollView();
                }

                EditorGUILayout.EndVertical();
            }

            EditorGUILayout.EndHorizontal();

            this.HandleSplitter(splitterRect);
        }
        
        public bool ProcessEvent(Event eventData)
        {
            switch (eventData.type)
            {
                case EventType.KeyUp:
                {
                    return this.ProcessEventKeyUp(eventData.keyCode);
                }

                default:
                {
                    return false;
                }
            }
        }

        // -------------------------------------------------------------------
        // Protected
        // -------------------------------------------------------------------
        protected override TreeViewItem BuildRoot()
        {
            this.nextEntryId = 0;
            this.root = new TreeViewItem { id = 0, depth = -1, displayName = "Root" };

            this.idObjectMap.Clear();

            if (this.activeContent != null)
            {
                this.treeViewEntryTempList.Clear();

                var orderedEntries = this.activeContent.Entries.OrderBy(x => x.Deprecated ? "XX - " + x.Name : x.Name);

                foreach (GameDataObject entry in orderedEntries)
                {
                    var item = new TreeViewItem(this.nextEntryId++, -1, entry.Name);

                    if (entry.Deprecated)
                    {
                        item.displayName = "XX - " + item.displayName;
                        var icon = GameDataHelpers.GetIcon("Deprecated");
                        if (icon != null)
                        {
                            item.icon = icon;
                        }
                    }
                    else
                    {
                        if (entry.IconSmall != null && entry.IconSmall.IsValid())
                        {
                            item.icon = ((Sprite) entry.IconSmall.Resource).texture;
                        }
                        else
                        {
                            item.icon = GameDataHelpers.GetIconForBaseType(entry.GetType());
                        }
                    }

                    this.idObjectMap.Add(item.id, entry);
                    this.treeViewEntryTempList.Add(item);
                }

                SetupParentsAndChildrenFromDepths(this.root, this.treeViewEntryTempList);
                this.treeViewEntryTempList.Clear();
            }

            this.ResetSelection();

            return this.root;
        }

        protected override void SelectionChanged(IList<int> selectedIds)
        {
            this.selection.Clear();
            foreach (var index in selectedIds)
            {
                this.selection.Add(index);
            }

            this.SendSelectedEvent();

            this.RebuildEditor();
        }

        // -------------------------------------------------------------------
        // Private
        // -------------------------------------------------------------------
        private void HandleSplitter(Rect splitterRect)
        {
            if (Event.current != null) {
                switch (Event.current.rawType) {
                    case EventType.MouseDown:
                    {
                        if (splitterRect.Contains(Event.current.mousePosition))
                        {
                            this.isSplitterDragging = true;
                        }

                        Event.current.Use();
                        break;
                    }

                    case EventType.MouseDrag:
                    {
                        if (this.isSplitterDragging)
                        {
                            this.treeViewWidth += Event.current.delta.x;
                            this.treeViewWidth = this.treeViewWidth.Clamp(MinWidth, MaxWidth);
                            
                            Repaint();
                        }

                        Event.current.Use();
                        break;
                    }
                    
                    case EventType.MouseUp:
                    {
                        if (this.isSplitterDragging)
                        {
                            this.isSplitterDragging = false;
                        }

                        Event.current.Use();
                        break;
                    }
                }
            }
        }
        
        private void SendSelectedEvent()
        {
            if (this.selection.Count == 0)
            {
                EditorEvents.Send(new EditorEventGameDataSelectionChanged());
                return;
            }
            
            IList<GameDataObject> selectedObjects = new List<GameDataObject>();
            foreach (int id in this.selection)
            {
                GameDataObject entry;
                if(this.idObjectMap.TryGetValue(id, out entry))
                {
                    selectedObjects.Add(entry);
                }
            }
            
            EditorEvents.Send(new EditorEventGameDataSelectionChanged(selectedObjects.ToArray()));
        }
        
        private void DrawToolBar(GameDataEditorContent content)
        {
            GUIContent guiContent = new GUIContent();
            
            EditorGUILayout.BeginHorizontal();
            {
                GUILayout.Space(4);
                
                guiContent.tooltip = "Add (F2)";
                guiContent.image = EditorGUIUtility.Load("icons/d_Collab.FileAdded.png") as Texture2D;
                if (GUILayout.Button(guiContent, this.toolBarStyle))
                {
                    this.OpenCreateItemDialog(content);
                }
                
                guiContent.tooltip = "Delete Selection";
                guiContent.image = EditorGUIUtility.Load("icons/d_TreeEditor.Trash.png") as Texture2D;
                if (GUILayout.Button(guiContent, this.toolBarStyle))
                {
                    if (EditorUtility.DisplayDialog("Confirm Delete", "This operation can not be undone, continue?", "yes", "no"))
                    {
                        this.currentEditor = null;
                        foreach (int id in this.selection)
                        {
                            GameDataObject entry;
                            if(this.idObjectMap.TryGetValue(id, out entry))
                            {
                                content.DeleteEntry(entry);
                            }
                        }

                        this.Reload();
                    }
                }
                
                guiContent.tooltip = "Clone Selected";
                guiContent.image = EditorGUIUtility.Load("icons/d_TreeEditor.Duplicate.png") as Texture2D;
                if (GUILayout.Button(guiContent, this.toolBarStyle))
                {
                    this.OpenCreateItemDialog(content, true);
                }
                
                GUILayout.Space(8);

                if (this.copyObject == null)
                {
                    guiContent.tooltip = "Copy";
                    guiContent.image = EditorGUIUtility.Load("icons/Clipboard.png") as Texture2D;
                    if (GUILayout.Button(guiContent, this.toolBarStyle))
                    {
                        this.CopySelectedObject();
                    }
                }
                else
                {
                    guiContent.tooltip = "Paste";
                    guiContent.image = EditorGUIUtility.Load("icons/d_Collab.FileUpdated.png") as Texture2D;
                    if (GUILayout.Button(guiContent, this.toolBarStyle))
                    {
                        this.PastCopyObjectToSelected();
                    }
                    
                    guiContent.tooltip = "Cancel Copy";
                    guiContent.image = EditorGUIUtility.Load("icons/d_LookDevClose.png") as Texture2D;
                    if (GUILayout.Button(guiContent, this.toolBarStyle))
                    {
                        this.copyObject = null;
                    }
                }
            }
            EditorGUILayout.EndHorizontal();
        }

        private void ResetSelection()
        {
            this.selection.Clear();

            if (this.activeContent != null)
            {
                GameDataObject firstEntry = this.activeContent.Entries.FirstOrDefault();
                if (firstEntry != null)
                {
                    this.Select(firstEntry);
                }
            }
        }

        private void Select(GameDataObject entry)
        {
            this.selection.Clear();

            int entryId;
            if (this.idObjectMap.TryGetKey(entry, out entryId))
            {
                var ids = new List<int> {entryId};
                this.SetSelection(ids);
                this.SelectionChanged(ids);
            }
        }

        private void OpenCreateItemDialog(GameDataEditorContent content, bool clone = false)
        {
            if (clone)
            {
                this.CopySelectedObject();
            }

            var prompt = ScriptableObject.CreateInstance<GameDataCreatePrompt>();
            prompt.Init(newName =>
            {
                GameDataObject entry = content.CreateEntry(newName);
                
                this.Reload();
                this.Select(entry);
                EditorWindow.GetWindow<GameDataEditorWindow>().Focus();

                if (clone)
                {
                    this.PastCopyObjectToSelected();
                }
            });
        }

        private void PastCopyObjectToSelected()
        {
            if (this.copyObject == null)
            {
                return;
            }

            this.copyObject.Update();

            foreach (int id in this.selection)
            {
                GameDataObject entry;
                if(!this.idObjectMap.TryGetValue(id, out entry))
                {
                    continue;
                }

                var target = new SerializedObject(entry);
                target.Update();

                SerializedProperty property = this.copyObject.GetIterator();
                if (property.NextVisible(true))
                {
                    do
                    {
                        if (property.name == "Guid" || property.name == "Name")
                        {
                            continue;
                        }

                        target.CopyFromSerializedProperty(this.copyObject.FindProperty(property.name));

                    }
                    while (property.NextVisible(false));
                }

                target.ApplyModifiedProperties();
            }
        }

        private void CopySelectedObject()
        {
            this.copyObject = null;
            if (this.selection.Count == 0)
            {
                return;
            }

            GameDataObject entry;
            if (this.idObjectMap.TryGetValue(this.selection.First(), out entry))
            {
                this.copyObject = new SerializedObject(entry);
            }
        }

        private void RebuildEditor()
        {
            this.entryTempList.Clear();
            foreach (int id in this.selection)
            {
                GameDataObject entry;
                if (this.idObjectMap.TryGetValue(id, out entry))
                {
                    this.entryTempList.Add(entry);
                }
            }

            this.currentEditor = Editor.CreateEditor(this.entryTempList.ToArray());
            this.entryTempList.Clear();
        }

        private bool ProcessEventKeyUp(KeyCode key)
        {
            switch (key)
            {
                case KeyCode.F2:
                {
                    this.createOnNextRepaint = true;
                    GUI.changed = true;
                    return true;
                }

                default:
                {
                    return false;
                }
            }
        }
    }
}

﻿using EditorEventGameDataSelectionChanged = Craiel.GameData.Editor.Events.EditorEventGameDataSelectionChanged;

namespace Assets.Scripts.Craiel.GameData.Editor.Window
{
    using System.Collections.Generic;
    using System.Linq;
    using Common;
    using Essentials.Collections;
    using Essentials.Editor.UserInterface;
    using Essentials.Event.Editor;
    using UnityEditor;
    using UnityEditor.IMGUI.Controls;
    using UnityEngine;

    public class GameDataTreeContentPresenter : TreeView, IGameDataContentPresenter
    {
        private readonly SearchField searchField;
        private readonly ExtendedDictionary<int, GameDataObject> idObjectMap;
        private readonly IList<int> selection;
        private readonly IList<TreeViewItem> treeViewEntryTempList;
        private readonly IList<GameDataObject> entryTempList;

        private TreeViewItem root;
        private int nextEntryId;

        private Vector2 scrollPos;
        private float treeViewWidth = 300;

        private SerializedObject copyObject;

        private GameDataEditorContent activeContent;

        private Editor currentEditor;

        private bool createOnNextRepaint;

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

                foreach (GameDataObject entry in this.activeContent.Entries)
                {
                    var item = new TreeViewItem(this.nextEntryId++, -1, entry.Name);
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
            EditorGUILayout.BeginHorizontal();
            {
                if (GUILayout.Button("Add (F2)", "button", GUILayout.Height(30)))
                {
                    this.OpenCreateItemDialog(content);
                }

                if (GUILayout.Button("Delete Selected", "button", GUILayout.Height(30)))
                {
                    if (EditorUtility.DisplayDialog("Delete Selection", "This operation can not be undone, continue?", "yes", "no"))
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

            }

            EditorGUILayout.EndHorizontal();

            if (GUILayout.Button("Clone Selected"))
            {
                this.OpenCreateItemDialog(content, true);
            }

            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Copy"))
            {
                this.CopySelectedObject();
            }

            if (this.copyObject != null)
            {
                if (GUILayout.Button("Cancel"))
                {
                    this.copyObject = null;
                }
            }

            GUILayout.EndHorizontal();

            if (this.copyObject != null)
            {
                if (GUILayout.Button(string.Format("Paste ({0})", this.copyObject.FindProperty("Name").stringValue)))
                {
                    this.PastCopyObjectToSelected();
                }
            }
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

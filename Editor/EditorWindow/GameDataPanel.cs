namespace Assets.Scripts.Craiel.GameData.Editor.EditorWindow
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Common;
    using Enums;
    using Essentials.Editor.UserInterface;
    using Essentials.IO;
    using TreeViewHelpers;
    using UnityEditor;
    using UnityEditor.IMGUI.Controls;
    using UnityEngine;

    public class GameDataPanel : GameDataPanelBase
    {
        private SearchField searchField;
        private Editor currentEditor;
        private Vector2 scrollPos;
        private float treeViewWidth = 300;
        
        private SerializedObject copyObject;

        private Texture icon;

        // -------------------------------------------------------------------
        // Constructor
        // -------------------------------------------------------------------
        public GameDataPanel(Type dataObjectType, string title, CarbonDirectory subFolder, params int[] workSpaces) 
            : base(dataObjectType, title, subFolder, workSpaces)
        {
        }

        // -------------------------------------------------------------------
        // Public
        // -------------------------------------------------------------------
        public override Texture Icon
        {
            get
            {
                if (this.icon == null)
                {
                    this.icon = GameDataHelpers.GetIconForBaseType(this.DataObjectType);
                }

                return this.icon;
            }
        }

        public GameDataObjectTreeView TreeView { get; private set; }

        public TreeViewState TreeViewState { get; private set; }

        public override void Init()
        {
            if (!this.IsInit)
            {
                this.TreeViewState = new TreeViewState();
                this.TreeView = new GameDataObjectTreeView(this.TreeViewState, this.DataObjectType);
                this.TreeView.OnSelectionChanged.RemoveAllListeners();
                this.TreeView.OnSelectionChanged.AddListener(this.BuildSelection);
                this.searchField = new SearchField();
                this.searchField.downOrUpArrowKeyPressed += this.TreeView.SetFocusAndEnsureSelectedItem;
            }

            this.IsInit = true;
        }

        public override void OnFocus()
        {
            base.OnFocus();

            this.TreeView.Reload();

            if (this.TreeView.SelectedData != null && this.TreeView.SelectedData.Count == 1)
            {
                this.TreeView.SelectItem(this.TreeView.SelectedData[0]);
            }
            else
            {
                this.TreeView.SelectFirstItem();
            }
            
        }

        public override void SelectItemByGuid(string itemGuid)
        {
            this.TreeView.SelectItemByGuid(itemGuid);
        }
        
        public override void SelectItemByObject(object target)
        {
            this.TreeView.SelectItemByObject(target);
        }

        public override void OnInspectorGUI()
        {
            this.HandleKeyboardShortcuts();
            
            switch (GameDataEditorCore.Config.GetViewMode())
            {
                case GameDataEditorViewMode.Compact:
                {
                    this.DrawCompact();
                    break;
                }

                case GameDataEditorViewMode.Full:
                {
                    this.DrawFull();
                    break;
                }
            }
        }

        // -------------------------------------------------------------------
        // Private
        // -------------------------------------------------------------------
        private void DrawCompact()
        {
            EditorGUILayout.BeginHorizontal();

            foreach (GameDataObject dataObject in this.TreeView.Data)
            {
                // TODO
            }
            
            EditorGUILayout.EndHorizontal();
        }

        private void DrawFull()
        {
            EditorGUILayout.BeginHorizontal();
            {
                // Left
                EditorGUILayout.BeginVertical("box", GUILayout.Width(this.treeViewWidth), GUILayout.ExpandHeight(true));
                {
                    GUILayout.Space(5);
                    this.ToolBar();
                    
                    GUILayout.Space(5);
                    this.TreeView.searchString = this.searchField.OnGUI(this.TreeView.searchString);
                    GUILayout.Space(5);
                    var treeViewRect = GUILayoutUtility.GetRect(this.treeViewWidth, this.treeViewWidth, 0, 100000);

                    this.TreeView.OnGUI(treeViewRect);
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

        private void HandleKeyboardShortcuts()
        {
            if (Event.current.isKey && Event.current.type == EventType.KeyUp)
            {
                switch (Event.current.keyCode)
                {
                    case KeyCode.F2:
                        this.OpenCreateItemDialog();
                        Event.current.Use();
                        break;
                }
            }
        }
        
        private void BuildSelection()
        {
            if (this.TreeView.SelectedData == null || this.TreeView.SelectedData.Count == 0)
            {
                this.currentEditor = null;
                return;
            }

            this.currentEditor = Editor.CreateEditor(this.TreeView.SelectedData.ToArray());
        }

        private void ToolBar()
        { 
            EditorGUILayout.BeginHorizontal();
            {
                if (GUILayout.Button("Add (F2)", "button", GUILayout.Height(30)))
                {
                    this.OpenCreateItemDialog(); 
                }

                if (GUILayout.Button("Delete Selected", "button", GUILayout.Height(30)))
                {
                    if (EditorUtility.DisplayDialog("Delete Selection", "This operation can not be undone, continue?", "yes", "no"))
                    {
                        this.currentEditor = null;
                        var selectedData = this.TreeView.SelectedData;
                        for (var i = selectedData.Count - 1; i >= 0; i--)
                        {
                            var item = this.TreeView.SelectedData[i];

                            GameDataHelpers.DeleteAsset(item);
                        }

                        this.TreeView.Reload();
                        this.TreeView.SelectFirstItem();

                        this.BuildSelection();
                    }
                }

            }

            EditorGUILayout.EndHorizontal();

            if (GUILayout.Button("Clone Selected"))
            {
                this.OpenCreateItemDialog(true);
            }

            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Copy"))
            {
                this.CopySelectedObject();
            }
            
            GUI.enabled = this.copyObject != null;
            if (GUILayout.Button("Paste " + (GUI.enabled ? string.Format("({0})", this.copyObject.FindProperty("Name").stringValue) : "")))
            {
                this.PastCopyObjectToSelected();
            }
            
            GUI.enabled = true;

            GUILayout.EndHorizontal();
        }

        private void PastCopyObjectToSelected()
        {
            this.copyObject.Update();
            var copy = new SerializedObject(this.TreeView.SelectedData[0]);
            copy.Update();

            SerializedProperty property = this.copyObject.GetIterator();
            if (property.NextVisible(true))
            {
                do
                {
                    // Draw movePoints property manually.
                    if (property.name == "Guid" || property.name == "Name")
                    {
                        continue;
                    }

                    copy.CopyFromSerializedProperty(this.copyObject.FindProperty(property.name));

                }
                while (property.NextVisible(false));
            }
            
            copy.ApplyModifiedProperties();
        }

        private void CopySelectedObject()
        {
            this.copyObject = new SerializedObject(this.TreeView.SelectedData[0]);
        }

        private void OpenCreateItemDialog(bool clone = false)
        {
            if (clone)
            {
                this.CopySelectedObject();
            }
            
            var prompt = ScriptableObject.CreateInstance<GameDataCreatePrompt>();
            prompt.Init(newName =>
            {
                CarbonDirectory folder = this.SubFolder == null ? new CarbonDirectory(this.Title) : this.SubFolder.ToDirectory(this.Title);
                var newObject = GameDataHelpers.CreateAsset(this.DataObjectType, folder, newName.Trim());
                this.TreeView.Reload();
                this.TreeView.SelectItem(newObject);
                EditorWindow.GetWindow<GameDataEditorWindow>().Focus();

                if (clone)
                {
                    this.PastCopyObjectToSelected();
                }
            });
        }
    }
}
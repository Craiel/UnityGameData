namespace Assets.Scripts.Craiel.GameData.Editor.EditorWindow
{
    using System;
    using System.Collections.Generic;
    using Common;
    using Essentials.Editor.UserInterface;
    using TreeViewHelpers;
    using UnityEditor;
    using UnityEditor.IMGUI.Controls;
    using UnityEngine;

    public class PanelTreeView<T> : PanelBase where T : GameDataObject
    {
        private readonly bool canEditHierarchy;
        
        private SearchField searchField;
        private Editor currentEditor;
        private Vector2 scrollPos;
        private float treeViewWidth = 300;

        // -------------------------------------------------------------------
        // Constructor
        // -------------------------------------------------------------------
        public PanelTreeView(string title, string subFolder, bool canEditHierarchy, params int[] workSpaces) 
            : base(title, subFolder, workSpaces)
        {
            this.TreeElements = new List<TreeElement>();
            this.canEditHierarchy = canEditHierarchy;
        }

        // -------------------------------------------------------------------
        // Public
        // -------------------------------------------------------------------
        [SerializeField] public readonly List<TreeElement> TreeElements;
        private SerializedObject copyObject;

        public override Type GameDataObjectType
        {
            get { return typeof(T); }
        }

        public override Texture Icon
        {
            get { return GameDataHelpers.GetIconForBaseType(typeof(T)); }
        }

        public GameDataObjectTreeView<T> TreeView { get; set; }

        public TreeViewState TreeViewState { get; private set; }

        public override void Init()
        {
            if (!this.IsInit)
            {
                this.TreeViewState = new TreeViewState();
                this.TreeView = new GameDataObjectTreeView<T>(this.TreeViewState);
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
            EditorGUILayout.BeginHorizontal();
            {
                // Left
                EditorGUILayout.BeginVertical("box", GUILayout.Width(this.treeViewWidth), GUILayout.ExpandHeight(true));
                {
                    if (this.canEditHierarchy)
                    {
                        GUILayout.Space(5);
                        this.ToolBar();
                    }
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

                    this.DrawEditor();
                    EditorGUILayout.EndScrollView();
                }

                EditorGUILayout.EndVertical();
            }

            EditorGUILayout.EndHorizontal();

            // Events
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

        public void DrawEditor()
        {
            if (this.currentEditor != null)
            {
                Layout.SetExtendedLabelSize();
                EditorGUI.BeginChangeCheck();
                this.currentEditor.OnInspectorGUI();

                Layout.SetDefaultLabelSize();
            }
        }

        // -------------------------------------------------------------------
        // Private
        // -------------------------------------------------------------------
        private void BuildSelection()
        {
            if (this.TreeView.SelectedData == null || this.TreeView.SelectedData.Count == 0)
            {
                this.currentEditor = null;
                return;
            }

            this.currentEditor = Editor.CreateEditor(this.TreeView.SelectedData.ToArray());

            if (GameDataEditorWindow.Instance != null)
            {
                GameDataEditorWindow.Instance.AddToHistory(this.TreeView.SelectedData[0]);
            }
        }

        private void ToolBar()
        { 
            EditorGUILayout.BeginHorizontal();
            {
                var style = "button";

                if (GUILayout.Button("Add (F2)", style, GUILayout.Height(30)))
                {
                    this.OpenCreateItemDialog(); 
                }

                if (GUILayout.Button("Delete Selected", style, GUILayout.Height(30)))
                {
                    if (EditorUtility.DisplayDialog("Delete Selection", "Are you sure ? (NOT Undoable)", "yes", "no"))
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
            var s = new SerializedObject(this.TreeView.SelectedData[0]);
            s.Update();

            SerializedProperty prop = this.copyObject.GetIterator();
            if (prop.NextVisible(true))
            {
                do
                {
                    // Draw movePoints property manually.
                    if (prop.name == "Guid" || prop.name == "Name")
                    {
                        continue;
                    }

                    s.CopyFromSerializedProperty(this.copyObject.FindProperty(prop.name));

                }
                while (prop.NextVisible(false));
            }
            s.ApplyModifiedProperties();
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
            var prompt = EditorWindow.CreateInstance<PromptDialog>();
            prompt.Init(newName =>
            {
                string folder = string.IsNullOrEmpty(this.SubFolder) ? this.Title : this.SubFolder + "/" + this.Title;
                var newObject = GameDataHelpers.CreateAsset<T>(folder, newName.Trim());
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

    public class PromptDialog : EditorWindow
    {
        private string newName = string.Empty;

        private bool focused;

        private Action<string> callback;

        public void Init(Action<string> newCallback)
        {
            this.callback = newCallback;

            this.minSize = this.maxSize = new Vector2(450, 100);

            this.titleContent = new GUIContent("New Item");
            this.ShowAuxWindow();
            this.Focus();
        }

        public void OnGUI()
        {
            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();

            GUILayout.BeginVertical(GUILayout.Width(400));
            GUILayout.FlexibleSpace();
            GUI.SetNextControlName("newNameTextField");
            this.newName = EditorGUILayout.TextField("Name", this.newName);
            GUILayout.Space(10);
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("OK (Return)", GUILayout.Height(30)))
            {
                this.CreateItem();
            }

            if (GUILayout.Button("CANCEL (ESC)", GUILayout.Height(30)))
            {
                this.Close();
            }
            GUILayout.EndHorizontal();
            GUILayout.FlexibleSpace();
            GUILayout.EndVertical();


            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
            if (!this.focused)
            {
                this.FocusTextField();
                this.focused = true;
            }

            if (Event.current.isKey)
            {
                switch (Event.current.keyCode)
                {
                    case KeyCode.Return:
                    case KeyCode.KeypadEnter:
                        this.CreateItem();
                        Event.current.Use();
                        break;

                    case KeyCode.Escape:
                        Event.current.Use();
                        this.Close();
                        break;
                }
            }
        }

        private void CreateItem()
        {
            var s = this.newName.Trim();
            if (!string.IsNullOrEmpty(s))
            {
                this.callback(s);
            }

            this.Close();
        }

        private void FocusTextField()
        {
            EditorGUI.FocusTextInControl("newNameTextField");
        }
    }
}
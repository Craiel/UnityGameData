namespace Craiel.UnityGameData.Editor.Window
{
    using System.Collections.Generic;
    using System.Linq;
    using Builder;
    using Common;
    using Enums;
    using UnityEditor;
    using UnityEngine;
    using UnityEssentials.Editor;
    using UnityEssentials.Editor.UserInterface;
    using UnityEssentials.Runtime;
    using UnityEssentials.Runtime.IO;

    public class GameDataEditorWindow : EssentialEditorWindowIM<GameDataEditorWindow>
    {
        private const int DefaultWorkSpaceId = 0;
        private const string DefaultWorkSpaceName = "None";

        private static readonly IList<GameDataEditorContent> Content = new List<GameDataEditorContent>();

        private static readonly IDictionary<int, string> WorkSpaces = new Dictionary<int, string>
        {
            { DefaultWorkSpaceId, DefaultWorkSpaceName }
        };

        private GameDataEditorContent[] contentSorted;

        private int selectedWorkSpace;

        private GameDataEditorViewMode selectedViewMode;

        private IGameDataContentPresenter activePresenter;

        // -------------------------------------------------------------------
        // Public
        // -------------------------------------------------------------------
        [SerializeField]
        public GUIStyle CategoryStyle;

        [SerializeField]
        public int CurrentPanelIndex;

        public static void OpenWindow()
        {
            OpenWindow("GameData Editor");
        }

        public override void OnEnable()
        {
            base.OnEnable();

            GameDataEditorCore.Configure();

            this.SetActiveContent(this.CurrentPanelIndex);

            this.autoRepaintOnSceneChange = true;

            if (this.CategoryStyle == null)
            {
                this.CategoryStyle = new GUIStyle(EditorStyles.boldLabel)
                {
                    fixedHeight = 50,
                    stretchWidth = false,
                    alignment = TextAnchor.MiddleCenter,
                    wordWrap = true
                };
            }

            this.selectedWorkSpace = GameDataEditorCore.Config.GetWorkspace(DefaultWorkSpaceId);
            this.selectedViewMode = GameDataEditorCore.Config.GetViewMode();
            this.SortContent();
            this.UpdateContentPresenter();
        }

        public override void OnDisable()
        {
            GameDataEditorCore.Config.SetWorkspace(this.selectedWorkSpace);
            GameDataEditorCore.Config.SetViewMode(this.selectedViewMode);

            base.OnDisable();
        }

        public void OnGUI()
        {
            this.DrawToolbar();
            this.DrawToolbarButtons();
            this.DrawPanels();

            ProcessEvents(Event.current);

            if (GUI.changed)
            {
                Repaint();
            }
        }

        public bool IsCurrentPane(string pane)
        {
            if (Content == null || Content.Count <= this.CurrentPanelIndex || Content[this.CurrentPanelIndex] == null || string.IsNullOrEmpty(Content[this.CurrentPanelIndex].Title))
            {
                return false;
            }

            return Content[this.CurrentPanelIndex].Title.Equals(pane);
        }

        public bool SelectRef(GameDataObject refObject)
        {
            string typeName = refObject.GetType().Name;
            GameDataEditorContent editorContent = Content.FirstOrDefault(p => p.IsOfType(typeName));
            if (editorContent == null)
            {
                Debug.LogErrorFormat("Can't find panel for type: {0}", refObject.GetType().Name);
                return false;
            }

            this.SetActiveContent(editorContent);
            return editorContent.SelectEntry(refObject);
        }

        public void SelectRef(GameDataRefBase refData)
        {
            SelectRef(refData.RefGuid);
        }

        public void SelectRef(SerializedProperty property)
        {
            var guid = property.FindPropertyRelative("RefGuid");
            if (guid == null)
            {
                Debug.LogErrorFormat("Can't find guid property");
                return;
            }

            SelectRef(guid.stringValue);
        }

        public void SelectRef(string guid)
        {
            if (string.IsNullOrEmpty(guid))
            {
                Debug.LogErrorFormat("Can't find guid property");
                return;
            }

            var dataObject = GameDataHelpers.FindGameDataByGuid(guid);
            if (dataObject == null)
            {
                Debug.LogErrorFormat("Can't find GameDataRef for: {0}", guid);
                return;
            }

            this.SelectRef(dataObject);
        }

        public static void AddContent<T>(string contentTitle, params int[] workSpaces)
            where T : GameDataObject
        {
            AddContent<T>(contentTitle, null, workSpaces);
        }

        public static void AddContent<T>(string contentTitle, ManagedDirectory subFolder, params int[] workSpaces)
            where T : GameDataObject
        {
            IList<int> workSpaceList = workSpaces.ToList();
            if (!workSpaceList.Contains(DefaultWorkSpaceId))
            {
                workSpaceList.Add(DefaultWorkSpaceId);
            }

            var content = new GameDataEditorContent(TypeCache<T>.Value);
            content.Initialize(contentTitle, subFolder, workSpaceList.ToArray());
            Content.Add(content);
        }

        public static void ClearContent()
        {
            Content.Clear();
        }

        public static void AddWorkSpace(int id, string title)
        {
            if (WorkSpaces.ContainsKey(id))
            {
                GameDataEditorCore.Logger.Error("Duplicate WorkSpace Registered: {0} {1} -> {2}", id, WorkSpaces[id], title);
                return;
            }

            WorkSpaces.Add(id, title);
        }

        public static void ClearWorkspaces()
        {
            WorkSpaces.Clear();
            WorkSpaces.Add(DefaultWorkSpaceId, DefaultWorkSpaceName);
        }

        public static void ReloadContent()
        {
            foreach (GameDataEditorContent content in Content)
            {
                content.Reload();
            }
        }

        // -------------------------------------------------------------------
        // Private
        // -------------------------------------------------------------------
        private void DrawToolbar()
        {
            EditorGUILayout.BeginHorizontal("Toolbar");
            {
                if (EditorGUILayout.DropdownButton(new GUIContent("Data"), FocusType.Passive, "ToolbarDropDown"))
                {
                    var menu = new GenericMenu();
                    menu.AddItem(new GUIContent("Validate"), false, ValidateGameData);
                    menu.AddItem(new GUIContent("Export"), false, ExportGameData);
                    menu.ShowAsContext();
                    Event.current.Use();
                }

                if (EditorGUILayout.DropdownButton(new GUIContent("Tools"), FocusType.Passive, "ToolbarDropDown"))
                {
                    var menu = new GenericMenu();
                    menu.AddItem(new GUIContent("Upgrade Data"), false, UpgradeGameData);
                    menu.AddItem(new GUIContent("Normalize Data Filenames"), false, this.NormalizeNames);
                    menu.ShowAsContext();
                    Event.current.Use();
                }

                if (EditorGUILayout.DropdownButton(new GUIContent("View"), FocusType.Passive, "ToolbarDropDown"))
                {
                    var menu = new GenericMenu();
                    menu.AddItem(new GUIContent("Reload"), false, ReloadContent);
                    menu.ShowAsContext();
                    Event.current.Use();
                }

                if (EditorGUILayout.DropdownButton(new GUIContent(string.Format("Workspace: {0}", WorkSpaces[this.selectedWorkSpace])), FocusType.Passive, "ToolbarDropDown"))
                {
                    var menu = new GenericMenu();

                    foreach (int id in WorkSpaces.Keys)
                    {
                        int closure = id;
                        menu.AddItem(new GUIContent(WorkSpaces[id]), this.selectedWorkSpace == id, () => this.SelectWorkSpace(closure));
                    }

                    menu.ShowAsContext();
                    Event.current.Use();
                }

                if (EditorGUILayout.DropdownButton(new GUIContent(string.Format("ViewMode: {0}", this.selectedViewMode)), FocusType.Passive, "ToolbarDropDown"))
                {
                    var menu = new GenericMenu();

                    foreach (GameDataEditorViewMode viewMode in GameDataEditorEnumValues.GameDataEditorViewModeValues)
                    {
                        var closure = viewMode;
                        menu.AddItem(new GUIContent(viewMode.ToString()), this.selectedViewMode == viewMode, () => this.SelectViewMode(closure));
                    }

                    menu.ShowAsContext();
                    Event.current.Use();
                }

                GUILayout.FlexibleSpace();
            }

            EditorGUILayout.EndHorizontal();
        }

        private void DrawToolbarButtons()
        {
            EditorGUILayout.BeginHorizontal();
            {
                // GameData Buttons
                foreach (var content in this.contentSorted)
                {
                    Color currentContentColor = GUI.contentColor;
                    GUI.contentColor = Styles.DefaulEditortTextColor;

                    var active = GUILayout.Toggle(
                        content.IsActive,
                        new GUIContent(content.Title, content.Icon, content.Title),
                        this.ToolBarStyle);

                    GUI.contentColor = currentContentColor;
                    if (active != content.IsActive)
                    {
                        this.SetActiveContent(content);
                    }
                }
            }

            EditorGUILayout.EndHorizontal();
        }

        private void DrawPanels()
        {
            if (this.activePresenter != null && Content.Count > 0 && this.CurrentPanelIndex != -1)
            {
                Rect contentRect = new Rect(10, 80, position.width - 20, position.height - 90);
                this.activePresenter.Draw(contentRect, Content[this.CurrentPanelIndex]);
            }
        }

        private void SelectWorkSpace(int id)
        {
            this.selectedWorkSpace = id;
            this.SortContent();
        }

        private void SelectViewMode(GameDataEditorViewMode mode)
        {
            if (this.selectedViewMode == mode)
            {
                return;
            }

            this.selectedViewMode = mode;

            GameDataEditorCore.Config.SetViewMode(mode);

            this.UpdateContentPresenter();
        }

        private void UpdateContentPresenter()
        {
            switch (this.selectedViewMode)
            {
                case GameDataEditorViewMode.Full:
                {
                    this.activePresenter = new GameDataTreeContentPresenter();
                    break;
                }

                case GameDataEditorViewMode.Compact:
                {
                    this.activePresenter = new GameDataNodeContentPresenter();
                    break;
                }
            }
        }

        private void SortContent()
        {
            this.contentSorted = Content.Where(x => x.WorkSpaces.Contains(this.selectedWorkSpace)).ToArray();
            if (this.contentSorted.Length == 0)
            {
                this.ClearActiveContent();
            }
            else
            {
                this.SetActiveContent(this.contentSorted[0]);
            }

            this.Repaint();
        }

        private void ClearActiveContent()
        {
            if (this.CurrentPanelIndex < 0 || Content.Count == 0)
            {
                return;
            }

            this.DisposeContent(Content[this.CurrentPanelIndex]);
            this.CurrentPanelIndex = -1;
        }

        private void SetActiveContent(GameDataEditorContent panel)
        {
            for (var i = 0; i < Content.Count; i++)
            {
                if (Content[i] == panel)
                {
                    this.SetActiveContent(i);
                }
            }
        }

        private void SetActiveContent(int index)
        {
            if (Content.Count <= index)
            {
                GameDataEditorCore.Logger.Warn("SetCurrentPane called with index out of bounds, did you forget to add panel definitions?");
                return;
            }

            this.ClearActiveContent();

            Content[index].SetActive();

            this.CurrentPanelIndex = index;

            Content[this.CurrentPanelIndex].Focus();
        }

        private void DisposeContent(GameDataEditorContent panel)
        {
            if (panel == null)
            {
                return;
            }

            panel.SetActive(false);
        }

        public static void ExportGameData()
        {
            GameDataBuilder.Build();
        }

        public static void ValidateGameData()
        {
            GameDataValidationWindow.OpenWindow();
        }

        public static void UpgradeGameData()
        {
            GameDataBuilder.Upgrade();
        }

        private void NormalizeNames()
        {
            if (!EditorUtility.DisplayDialog(
                "Normalize Game Data Filenames",
                "This will rename all GameData files to match their respective data Names.",
                "Confirm",
                "Cancel"))
            {
                return;
            }

            var guids = AssetDatabase.FindAssets("t:GameDataObject");

            foreach (var guid in guids)
            {
                var oldPath = new ManagedFile(AssetDatabase.GUIDToAssetPath(guid));
                var obj = AssetDatabase.LoadAssetAtPath<GameDataObject>(oldPath.GetUnityPath());

                var newPath = oldPath.GetDirectory().ToFile(obj.Name + ".asset");

                if (newPath.GetUnityPath() == oldPath.GetUnityPath())
                {
                    // Same path
                    continue;
                }

                if (newPath.Exists)
                {
                    Debug.LogErrorFormat("Can't rename file {0} to {1}, path already exists", oldPath, newPath);
                    continue;
                }

                var rename = AssetDatabase.RenameAsset(oldPath.GetUnityPath(), newPath.FileNameWithoutExtension);

                if (!string.IsNullOrEmpty(rename))
                {
                    Debug.LogErrorFormat("Error Renaming: {0}", rename);
                }
            }
        }

        private void ProcessEvents(Event eventData)
        {
            if (this.activePresenter != null)
            {
                this.activePresenter.ProcessEvent(eventData);

                if (GUI.changed)
                {
                    this.Repaint();
                }
            }
        }
    }
}
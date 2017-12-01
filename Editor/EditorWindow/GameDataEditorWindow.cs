namespace Assets.Scripts.Craiel.GameData.Editor.EditorWindow
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Builder;
    using Common;
    using Craiel.Editor.GameData;
    using Enums;
    using Essentials.Editor.UserInterface;
    using Essentials.IO;
    using NLog;
    using UnityEditor;
    using UnityEngine;

    public class GameDataEditorWindow : EditorWindow
    {
        private const int DefaultWorkSpaceId = 0;
        private const string DefaultWorkSpaceName = "None";

        private static readonly NLog.Logger Logger = LogManager.GetCurrentClassLogger();

        private static readonly IList<PanelBase> Panels = new List<PanelBase>();

        private static readonly IDictionary<int, string> WorkSpaces = new Dictionary<int, string>
        {
            { DefaultWorkSpaceId, DefaultWorkSpaceName }
        };

        public static GameDataEditorWindow Instance { get; private set; }

        private readonly IList<GameDataObject> history;

        private float buttonsTotalWidth;

        private PanelBase[] panelsSorted;

        private int selectedWorkSpace;

        private GameDataEditorViewMode selectedViewMode;

        // -------------------------------------------------------------------
        // Constructor
        // -------------------------------------------------------------------
        public GameDataEditorWindow()
        {
            this.history = new List<GameDataObject>();
        }

        // -------------------------------------------------------------------
        // Public
        // -------------------------------------------------------------------
        [SerializeField]
        public GUIStyle ToolBarStyle;

        [SerializeField]
        public GUIStyle ToolBarStyleSmall;

        [SerializeField]
        public GUIStyle CategoryStyle;

        [SerializeField]
        public int CurrentPanelIndex;
        
        public static void OpenWindow()
        {
            var window = (GameDataEditorWindow)GetWindow(typeof(GameDataEditorWindow));
            window.titleContent = new GUIContent("GameData Editor");
            window.Show();
        }

        public void OnEnable()
        {
            GameDataEditorCore.Initialize();

            Instance = this;
            this.SetCurrentPane(this.CurrentPanelIndex);

            this.autoRepaintOnSceneChange = true;

            if (this.ToolBarStyle == null)
            {
                this.ToolBarStyle = new GUIStyle(EditorStyles.toolbarButton)
                {
                    imagePosition = ImagePosition.ImageAbove,
                    fixedHeight = 50,
                    fixedWidth = 80,
                    wordWrap = true
                };
            }

            if (this.ToolBarStyleSmall == null)
            {
                this.ToolBarStyleSmall = new GUIStyle(EditorStyles.toolbarButton)
                {
                    imagePosition = ImagePosition.ImageOnly,
                    fixedHeight = 50,
                    fixedWidth = 50,
                    wordWrap = false
                };
            }

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
            this.SortPanels();
        }

        public void OnDestroy()
        {
            Instance = null;
        }

        public void OnDisable()
        {
            GameDataEditorCore.Config.SetWorkspace(this.selectedWorkSpace);
            GameDataEditorCore.Config.SetViewMode(this.selectedViewMode);

            Instance = null;
        }
        
        public void OnGUI()
        {
            // Menu Tool Bar
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
                    menu.AddItem(new GUIContent("Upgrade"), false, UpgradeGameData);
                    menu.AddItem(new GUIContent("Normalize ScriptableObjects Name"), false, this.NormalizeNames);
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

            var style = this.buttonsTotalWidth > this.position.width ? this.ToolBarStyleSmall : this.ToolBarStyle;
            this.buttonsTotalWidth = 0;

            EditorGUILayout.BeginHorizontal();
            {
                // GameData Buttons
                foreach (var panel in panelsSorted)
                {
                    var old = GUI.contentColor;
                    GUI.contentColor = Styles.DefaulEditortTextColor;

                    var active = GUILayout.Toggle(
                        panel.Active,
                        new GUIContent(panel.Title, panel.Icon, panel.Title),
                        style);

                    this.buttonsTotalWidth += this.ToolBarStyle.fixedWidth;

                    GUI.contentColor = old;
                    if (active != panel.Active)
                    {
                        this.SetCurrentPane(panel);
                    }
                }
            }

            EditorGUILayout.EndHorizontal();
            
            // Panel
            if (Panels.Count > 0 && this.CurrentPanelIndex != -1 && Panels[this.CurrentPanelIndex].IsInit)
            {
                Panels[this.CurrentPanelIndex].OnInspectorGUI();
            }
        }

        public bool IsCurrentPane(string pane)
        {
            if (Panels == null || Panels.Count <= this.CurrentPanelIndex || Panels[this.CurrentPanelIndex] == null || string.IsNullOrEmpty(Panels[this.CurrentPanelIndex].Title))
            {
                return false;
            }

            return Panels[this.CurrentPanelIndex].Title.Equals(pane);
        }
        
        public void SelectRef(GameDataObject refObject)
        {
            PanelBase panel = Panels.FirstOrDefault(p => p.GameDataObjectType.Name == refObject.GetType().Name);
            if (panel == null)
            {
                Debug.LogErrorFormat("Can't find panel for type: {0}", refObject.GetType().Name);
                return;
            }

            this.SetCurrentPane(panel);
            panel.SelectItemByObject(refObject);
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

        public static void AddPanel<T>(string panelTitle, string subFolder, params int[] workSpaces)
            where T : GameDataObject
        {
            AddPanel<T>(panelTitle, subFolder, true, workSpaces);
        }
        
        public static void AddPanel<T>(string panelTitle, params int[] workSpaces)
            where T : GameDataObject
        {
            AddPanel<T>(panelTitle, null, true, workSpaces);
        }
        
        public static void AddPanel<T>(string panelTitle, string subFolder, bool canEditHierarchy, params int[] workSpaces)
            where T : GameDataObject
        {
            IList<int> workSpaceList = workSpaces.ToList();
            if (!workSpaceList.Contains(DefaultWorkSpaceId))
            {
                workSpaceList.Add(DefaultWorkSpaceId);
            }
            
            Type panelType = typeof(PanelTreeView<>).MakeGenericType(typeof(T));
            var panel = Activator.CreateInstance(panelType, panelTitle, subFolder ?? string.Empty, canEditHierarchy, workSpaceList.ToArray());
            Panels.Add((PanelBase)panel);
        }

        public static void AddWorkSpace(int id, string title)
        {
            if (WorkSpaces.ContainsKey(id))
            {
                Logger.Error("Duplicate WorkSpace Registered: {0} {1} -> {2}", id, WorkSpaces[id], title);
                return;
            }
            
            WorkSpaces.Add(id, title);
        }

        // -------------------------------------------------------------------
        // Private
        // -------------------------------------------------------------------
        private void SelectWorkSpace(int id)
        {
            this.selectedWorkSpace = id;
            this.SortPanels();
        }

        private void SelectViewMode(GameDataEditorViewMode mode)
        {
            this.selectedViewMode = mode;
        }

        private void SortPanels()
        {
            panelsSorted = Panels.Where(x => x.WorkSpaces.Contains(this.selectedWorkSpace)).ToArray();
            if (this.panelsSorted.Length == 0)
            {
                this.ClearCurrentPane();
            }
            else
            {
                this.SetCurrentPane(this.panelsSorted[0]);
            }
            
            this.Repaint();
        }

        private void ClearCurrentPane()
        {
            if (this.CurrentPanelIndex < 0 || Panels.Count == 0)
            {
                return;
            }
            
            this.DisposePanel(Panels[this.CurrentPanelIndex]);
            this.CurrentPanelIndex = -1;
        }
        
        private void SetCurrentPane(PanelBase panel)
        {
            for (var i = 0; i < Panels.Count; i++)
            {
                if (Panels[i] == panel)
                {
                    this.SetCurrentPane(i);
                }
            }
        }

        private void SetCurrentPane(int index)
        {
            if (Panels.Count <= index)
            {
                Logger.Warn("SetCurrentPane called with index out of bounds, did you forget to add panel definitions?");
                return;
            }

            this.ClearCurrentPane();

            if (Panels[index].IsInit == false)
            {
                Panels[index].Init();
            }

            Panels[index].Active = true;

            this.CurrentPanelIndex = index;

            Panels[this.CurrentPanelIndex].OnFocus();
        }

        private void DisposePanel(PanelBase panel)
        {
            if (panel == null)
            {
                return;
            }

            panel.Active = false;
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
                "Rename ScriptablesObjects",
                "This will rename every GameData ScriptableObjects to be in sync with the name.\nYou should only doing that if you have SourceControl activated.\nAsk before doing that :).",
                "Do it !",
                "Cancel"))
            {
                return;
            }

            var guids = AssetDatabase.FindAssets("t:GameDataObject");

            foreach (var guid in guids)
            {
                var oldPath = new CarbonFile(AssetDatabase.GUIDToAssetPath(guid));
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
        
        public void AddToHistory(GameDataObject gameDataObject)
        {
            if (this.history.Contains(gameDataObject))
            {
                this.history.Remove(gameDataObject);
            }

            this.history.Add(gameDataObject);
            if (this.history.Count > 10)
            {
                this.history.RemoveAt(0);
            }
        }
    }
}
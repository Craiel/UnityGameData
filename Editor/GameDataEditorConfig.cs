namespace Craiel.UnityGameData.Editor
{
    using Enums;
    using UnityEssentials.Editor;

    public class GameDataEditorConfig : EditorConfig<GameDataEditorConfigKeys>
    {
        private const string SaveKey = "gameDataEditor";
        private const int CurrentVersion = 1;

        // -------------------------------------------------------------------
        // Constructor
        // -------------------------------------------------------------------
        public GameDataEditorConfig() 
            : base(SaveKey, CurrentVersion)
        {
        }

        // -------------------------------------------------------------------
        // Public
        // -------------------------------------------------------------------
        public void SetWorkspace(int id)
        {
            this.Set(GameDataEditorConfigKeys.Workspace, id);
        }

        public int GetWorkspace(int defaultValue)
        {
            return this.GetInt(GameDataEditorConfigKeys.Workspace, defaultValue);
        }

        public void SetViewMode(GameDataEditorViewMode mode)
        {
            this.Set(GameDataEditorConfigKeys.ViewMode, (int) mode);
        }

        public GameDataEditorViewMode GetViewMode()
        {
            return (GameDataEditorViewMode)this.GetInt(GameDataEditorConfigKeys.ViewMode, (int) GameDataEditorViewMode.Compact);
        }
    }
}

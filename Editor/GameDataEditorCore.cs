namespace Craiel.UnityGameData.Editor
{
    using GameData.Editor.Contracts;
    using NLog;
    using UnityEssentials.Runtime.Component;

    public static class GameDataEditorCore
    {
        // -------------------------------------------------------------------
        // Public
        // -------------------------------------------------------------------
        public static readonly NLog.Logger Logger = LogManager.GetLogger("CRAIEL_GAMEDATA_EDITOR");

        public static bool IsInitialized { get; private set; }

        public static GameDataEditorConfig Config { get; private set; }
        
        public static bool IsPopupActive { get; set; }

        public static void Configure(bool reconfigure = false)
        {
            if (IsInitialized && !reconfigure)
            {
                return;
            }

            Config = new GameDataEditorConfig();
            Config.Load();

            new CraielComponentConfigurator<IGameDataEditorConfig>().Configure();
            
            IsInitialized = true;
        }
    }
}

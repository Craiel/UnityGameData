namespace Craiel.UnityGameData.Editor
{
    using GameData.Editor.Contracts;
    using UnityEssentials.Component;

    public static class GameDataEditorCore
    {
        // -------------------------------------------------------------------
        // Public
        // -------------------------------------------------------------------
        public static bool IsInitialized { get; private set; }

        public static GameDataEditorConfig Config { get; private set; }

        public static void Initialize()
        {
            if (IsInitialized)
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

using IGameDataEditorConfig = Craiel.GameData.Editor.Contracts.IGameDataEditorConfig;

namespace Assets.Scripts.Craiel.GameData.Editor
{
    using Essentials.Component;

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

namespace Assets.Scripts.Craiel.GameData.Editor
{
    using Contracts;
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

            new CraielComponentConfigurator<IGameDataConfig>().Configure();
            
            IsInitialized = true;
        }
    }
}

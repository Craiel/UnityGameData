namespace Assets.Scripts.Craiel.GameData.Editor
{
    using Contracts;
    using Essentials.UnityComponent;

    public static class GameDataEditorCore
    {
        // -------------------------------------------------------------------
        // Public
        // -------------------------------------------------------------------
        public static bool IsInitialized { get; private set; }

        public static void Initialize()
        {
            if (IsInitialized)
            {
                return;
            }
            
            new UnityComponentConfigurator<IGameDataConfig>().Configure();

            IsInitialized = true;
        }
    }
}

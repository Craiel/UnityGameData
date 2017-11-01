namespace Assets.Scripts.Craiel.GameData.Editor
{
    using System;
    using System.Linq;
    using Contracts;
    using NLog;

    public static class GameDataEditorCore
    {
        private static readonly NLog.Logger Logger = LogManager.GetCurrentClassLogger();

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

            Type configType = typeof(IGameDataConfig);
            var implementations = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(x => x.GetTypes())
                .Where(x => !x.IsInterface && configType.IsAssignableFrom(x))
                .ToList();

            if (implementations.Count != 1)
            {
                Logger.Error("No implementation of IGameDataConfig found, configure your game data first");
                return;
            }

            var config = Activator.CreateInstance(implementations.First()) as IGameDataConfig;
            if (config == null)
            {
                Logger.Error("Failed to instantiate config class");
                return;
            }

            config.Configure();

            IsInitialized = true;
        }
    }
}

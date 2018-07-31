namespace Craiel.UnityGameData.Runtime
{
    using NLog;
    using UnityEssentials.Runtime;
    using UnityEssentials.Runtime.IO;

    public static class GameDataCore
    {
        public static readonly NLog.Logger Logger = LogManager.GetLogger("CRAIEL_GAMEDATA");

        public static readonly ManagedDirectory GameDataListPath = new ManagedDirectory("ScriptableObjects");

        public const string GameDataListExtension = ".blst";

        public static readonly ManagedDirectory GameDataPath = EssentialsCore.AssetsPath.ToDirectory("Data");
    }
}

using EssentialsCore = Craiel.UnityEssentials.Runtime.EssentialsCore;
using ManagedDirectory = Craiel.UnityEssentials.Runtime.IO.ManagedDirectory;

namespace Craiel.UnityGameData.Runtime
{
    public static class GameDataCore
    {
        public static readonly ManagedDirectory GameDataListPath = new ManagedDirectory("ScriptableObjects");

        public const string GameDataListExtension = ".blst";

        public static readonly ManagedDirectory GameDataPath = EssentialsCore.AssetsPath.ToDirectory("Data");
    }
}

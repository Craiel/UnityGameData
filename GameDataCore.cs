using EssentialsCore = Craiel.UnityEssentials.EssentialsCore;
using ManagedDirectory = Craiel.UnityEssentials.IO.ManagedDirectory;

namespace Craiel.UnityGameData
{
    public static class GameDataCore
    {
        public static readonly ManagedDirectory GameDataListPath = new ManagedDirectory("ScriptableObjects");

        public const string GameDataListExtension = ".blst";

        public static readonly ManagedDirectory GameDataPath = EssentialsCore.AssetsPath.ToDirectory("Data");
    }
}

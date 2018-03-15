using EssentialsCore = Craiel.UnityEssentials.EssentialsCore;
using ManagedDirectory = Craiel.UnityEssentials.IO.ManagedDirectory;

namespace Assets.Scripts.Craiel.GameData
{
    using Essentials;

    public static class GameDataCore
    {
        public static readonly ManagedDirectory GameDataListPath = new ManagedDirectory("ScriptableObjects");

        public const string GameDataListExtension = ".blst";

        public static readonly ManagedDirectory GameDataPath = EssentialsCore.AssetsPath.ToDirectory("Data");
    }
}

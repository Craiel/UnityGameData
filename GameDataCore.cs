namespace Assets.Scripts.Craiel.GameData
{
    using Essentials;
    using Essentials.IO;

    public static class GameDataCore
    {
        public static readonly CarbonDirectory GameDataListPath = new CarbonDirectory("ScriptableObjects");

        public const string GameDataListExtension = ".blst";

        public static readonly CarbonDirectory GameDataPath = EssentialsCore.AssetsPath.ToDirectory("Data");
    }
}

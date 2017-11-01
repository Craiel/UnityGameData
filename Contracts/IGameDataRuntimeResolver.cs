namespace Assets.Scripts.Craiel.GameData.Contracts
{
    public interface IGameDataRuntimeResolver
    {
        GameDataId GetRuntimeId(GameDataRuntimeRefBase runtimeRef);
    }
}
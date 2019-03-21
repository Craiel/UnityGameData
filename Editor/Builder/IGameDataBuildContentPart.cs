namespace Craiel.UnityGameData.Editor.Builder
{
    using Common;
    using Runtime;

    public interface IGameDataBuildContentPart<out T>
        where T : RuntimeGameDataPart
    {
        void Validate(GameDataObject owner, GameDataBuildValidationContext context);

        T Build(GameDataObject owner, GameDataBuildContext context);
    }
}

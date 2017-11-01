namespace Assets.Scripts.Craiel.GameData.Editor.Builder
{
    public interface IGameDataBuildContentPart
    {
        void Validate(object owner, GameDataBuildValidationContext context);

        RuntimeGameDataPart Build(object owner, GameDataBuildContext context);
    }
}

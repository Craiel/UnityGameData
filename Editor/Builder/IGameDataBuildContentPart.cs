using RuntimeGameDataPart = Craiel.UnityGameData.RuntimeGameDataPart;

namespace Craiel.UnityGameData.Editor.Builder
{
    public interface IGameDataBuildContentPart<out T>
        where T : RuntimeGameDataPart
    {
        void Validate(object owner, GameDataBuildValidationContext context);

        T Build(object owner, GameDataBuildContext context);
    }
}

namespace Craiel.GameData.Editor.Contracts.VFXShared
{
    using UnityGameData.Editor.Contracts.VFXShared;

    public interface IVFXEditorComponent : IGameDataVFXNode
    {
        string Name { get; set; }
    }
}
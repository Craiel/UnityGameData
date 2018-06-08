namespace Craiel.UnityGameData.Editor.Contracts.VFXShared
{
    using Builder;
    using Runtime.VFXShared;
    using UnityEngine;
    using UnityGameData.VFXShared;

    public interface IGameDataVFXNode : IGameDataBuildContentPart<RuntimeVFXNodeData>
    {
        Vector2 Position { get; set; }
    }
}
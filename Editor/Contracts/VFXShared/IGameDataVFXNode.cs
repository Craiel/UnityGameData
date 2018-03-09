namespace Craiel.GameData.Editor.Contracts.VFXShared
{
    using Assets.Scripts.Craiel.GameData.Editor.Builder;
    using Assets.Scripts.Craiel.GameData.VFXShared;
    using UnityEngine;

    public interface IGameDataVFXNode : IGameDataBuildContentPart<RuntimeVFXNodeData>
    {
        Vector2 Position { get; set; }
    }
}
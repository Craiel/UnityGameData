namespace Craiel.UnityGameData.Editor.VFXShared
{
    using Builder;
    using Common;
    using GameData.Editor.Contracts.VFXShared;
    using Runtime.VFXShared;
    using UnityEngine;

    public abstract class VFXEditorComponentBase : IVFXEditorComponent
    {
        // -------------------------------------------------------------------
        // Public
        // -------------------------------------------------------------------
        public string Name { get; set; }
        
        public Vector2 Position { get; set; }
        
        public void Validate(GameDataObject owner, GameDataBuildValidationContext context)
        {
        }

        public RuntimeVFXNodeData Build(GameDataObject owner, GameDataBuildContext context)
        {
            return null;
        }
    }
}
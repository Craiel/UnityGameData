﻿namespace Craiel.UnityGameData.VFXShared
{
    using Editor.Builder;
    using GameData.Editor.Contracts.VFXShared;
    using UnityEngine;

    public abstract class VFXEdiorComponentBase : IVFXEditorComponent
    {
        // -------------------------------------------------------------------
        // Public
        // -------------------------------------------------------------------
        public string Name { get; set; }
        
        public Vector2 Position { get; set; }
        
        public void Validate(object owner, GameDataBuildValidationContext context)
        {
        }

        public RuntimeVFXNodeData Build(object owner, GameDataBuildContext context)
        {
            return null;
        }
    }
}
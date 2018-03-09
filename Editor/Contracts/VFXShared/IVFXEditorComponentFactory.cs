namespace Craiel.GameData.Editor.Contracts.VFXShared
{
    using System.Collections.Generic;
    using Assets.Scripts.Craiel.VFX.Editor.Components;
    using UnityEngine;

    public interface IVFXEditorComponentFactory
    {
        IList<VFXEditorComponentDescriptor> AvailableComponents { get; }

        IVFXEditorComponent CreateNew(VFXEditorComponentDescriptor descriptor, Vector2 position);
    }
}
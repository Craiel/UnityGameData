namespace Craiel.UnityGameData.Editor.Contracts.VFXShared
{
    using System.Collections.Generic;
    using GameData.Editor.Contracts.VFXShared;
    using UnityEngine;
    using UnityGameData.VFXShared;

    public interface IVFXEditorComponentFactory
    {
        IList<VFXEditorComponentDescriptor> AvailableComponents { get; }

        IVFXEditorComponent CreateNew(VFXEditorComponentDescriptor descriptor, Vector2 position);
    }
}
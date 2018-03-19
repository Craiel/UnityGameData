namespace Craiel.UnityGameData.VFXShared
{
    using System;
    using Editor.Contracts.VFXShared;

    public struct VFXEditorComponentDescriptor
    {
        // -------------------------------------------------------------------
        // Constructor
        // -------------------------------------------------------------------
        public VFXEditorComponentDescriptor(IVFXEditorComponentFactory factory)
            : this()
        {
            this.Factory = factory;
        }
        
        // -------------------------------------------------------------------
        // Public
        // -------------------------------------------------------------------
        public readonly IVFXEditorComponentFactory Factory;
        
        public Type Type;

        public string Name;

        public string Category;
    }
}
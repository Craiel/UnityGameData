namespace Craiel.GameData.Editor.Events
{
    using UnityEssentials.Runtime.Contracts.Editor;
    using UnityGameData.Editor.Common;

    public class EditorEventGameDataSelectionChanged : IEditorEvent
    {
        // -------------------------------------------------------------------
        // Constructor
        // -------------------------------------------------------------------
        public EditorEventGameDataSelectionChanged(params GameDataObject[] selected)
        {
            this.SelectedObjects = selected;
        }
        
        // -------------------------------------------------------------------
        // Public
        // -------------------------------------------------------------------
        public GameDataObject[] SelectedObjects { get; private set; }
    }
}
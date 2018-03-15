﻿namespace Craiel.GameData.Editor.Events
{
    using Assets.Scripts.Craiel.GameData.Editor.Common;
    using UnityEssentials.Contracts.Editor;

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
using EditorEventGameDataSelectionChanged = Craiel.GameData.Editor.Events.EditorEventGameDataSelectionChanged;
using EditorEvents = Craiel.UnityEssentials.Runtime.Event.Editor.EditorEvents;
using ScriptableNodeBase = Craiel.UnityEssentials.Editor.NodeEditor.ScriptableNodeBase;

namespace Craiel.UnityGameData.Editor.Window
{
    using Common;
    using UnityEditor;
    using UnityEngine;

    public class GameDataCompactNode : ScriptableNodeBase
    {
        private readonly GameDataObject entry;

        private readonly IGameDataCompactEditor editor;

        // -------------------------------------------------------------------
        // Constructor
        // -------------------------------------------------------------------
        public GameDataCompactNode(GameDataObject entry)
        {
            this.entry = entry;

            this.editor = UnityEditor.Editor.CreateEditor(this.entry) as IGameDataCompactEditor;

            this.EnableDrag = false;
        }

        // -------------------------------------------------------------------
        // Public
        // -------------------------------------------------------------------
        public override void Draw(Rect drawArea)
        {
            base.Draw(drawArea);

            if (this.editor == null)
            {
                return;
            }

            this.SetSize(this.editor.GetCompactWidth(), this.editor.GetCompactHeight());

            GUILayout.BeginArea(this.NodeRect, EditorStyles.helpBox);
            this.editor.DrawCompact();
            GUILayout.EndArea();
        }

        public override bool ProcessEvent(Event eventData)
        {
            if (base.ProcessEvent(eventData))
            {
                return true;
            }

            if (!this.NodeRect.Contains(eventData.mousePosition))
            {
                return false;
            }

            switch (eventData.type)
            {
                case EventType.MouseUp:
                {
                    if (eventData.button == 0)
                    {
                        // Select this data object in inspector
                        Selection.activeObject = this.entry;
                        
                        EditorEvents.Send(new EditorEventGameDataSelectionChanged(this.entry));
                        
                        return true;
                    }

                    return false;
                }

                default:
                {
                    return false;
                }
            }
        }
    }
}

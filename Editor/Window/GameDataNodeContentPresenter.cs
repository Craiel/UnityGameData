using ScriptableNodeEditor = Craiel.UnityEssentials.Editor.NodeEditor.ScriptableNodeEditor;
using ScriptableNodeGridLayouter = Craiel.UnityEssentials.Editor.NodeEditor.ScriptableNodeGridLayouter;

namespace Craiel.UnityGameData.Editor.Window
{
    using Common;
    using UnityEngine;

    // TODO: http://gram.gs/gramlog/creating-node-based-editor-unity/
    // https://forum.unity.com/threads/simple-node-editor.189230/
    public class GameDataNodeContentPresenter : ScriptableNodeEditor, IGameDataContentPresenter
    {
        private GameDataEditorContent activeContent;

        // -------------------------------------------------------------------
        // Constructor
        // -------------------------------------------------------------------
        public GameDataNodeContentPresenter()
        {
            // Might want to disable this for this case:
            //this.GridEnableMeasureSections = false;

            this.LayouterEnabled = true;
            this.Layouter = new ScriptableNodeGridLayouter
            {
                ColumnMargin = 8,
                RowMargin = 8
            };
        }
        
        // -------------------------------------------------------------------
        // Public
        // -------------------------------------------------------------------
        public void Draw(Rect drawArea, GameDataEditorContent content)
        {
            if (this.activeContent != content)
            {
                this.activeContent = content;
                this.Reload();
            }

            this.Draw(drawArea);
        }

        // -------------------------------------------------------------------
        // Private
        // -------------------------------------------------------------------
        private void Reload()
        {
            this.Clear();
            foreach (GameDataObject entry in this.activeContent.Entries)
            {
                this.AddNode(new GameDataCompactNode(entry));
            }
        }
    }
}

namespace Assets.Scripts.Craiel.GameData.Editor.Window
{
    using Common;
    using Essentials.Editor.NodeEditor;
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
    }
}

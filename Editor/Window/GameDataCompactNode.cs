namespace Assets.Scripts.Craiel.GameData.Editor.Window
{
    using Common;
    using Essentials.Editor.NodeEditor;
    using UnityEditor;
    using UnityEngine;

    public class GameDataCompactNode : ScriptableNodeBase
    {
        private readonly GameDataObject entry;

        private readonly Editor editor;

        public GameDataCompactNode(GameDataObject entry)
        {
            this.entry = entry;

            this.editor = UnityEditor.Editor.CreateEditor(this.entry);

            this.SetSize(400, 300);
        }

        public override void Draw(Rect drawArea)
        {
            base.Draw(drawArea);

            //GUI.Box(this.NodeRect, this.entry.Name, ScriptableNodeStyleDefault.Instance.Style);

            GUILayout.BeginArea(this.NodeRect);
            this.editor.OnInspectorGUI();
            GUILayout.EndArea();
        }
    }
}

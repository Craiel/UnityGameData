namespace Craiel.UnityGameData.Editor.Common
{
    using UnityEditor;
    using UnityEngine;
    using UnityEssentials.Editor;

    [CustomEditor(typeof(GameDataObject))]
    [CanEditMultipleObjects]
    public abstract class GameDataObjectEditor : EssentialCustomEditor, IGameDataCompactEditor
    {
        private static bool objectFoldout;

        // -------------------------------------------------------------------
        // Public
        // -------------------------------------------------------------------
        public virtual int GetCompactWidth()
        {
            return 200;
        }

        public virtual int GetCompactHeight()
        {
            return 100;
        }

        public virtual void DrawCompact()
        {
            this.serializedObject.Update();
            
            var typedTarget = (GameDataObject)this.target;
            GUILayout.Label(typedTarget.Name, EditorStyles.boldLabel);
        }

        // -------------------------------------------------------------------
        // Protected
        // -------------------------------------------------------------------
        protected override void DrawFull()
        {
            if (this.UseDefaultInspector)
            {
                base.DrawFull();
                return;
            }
            
            if (this.DrawFoldout("Object Properties", ref objectFoldout))
            {
                this.DrawProperty<GameDataObject>(x => x.Guid);
                this.DrawProperty<GameDataObject>(x => x.Name);
                this.DrawProperty<GameDataObject>(x => x.DisplayName);
                this.DrawProperty<GameDataObject>(x => x.Notes);
                this.DrawProperty<GameDataObject>(x => x.Description);
                this.DrawProperty<GameDataObject>(x => x.IconSmall);
                this.DrawProperty<GameDataObject>(x => x.IconLarge);
            }
        }
    }
}
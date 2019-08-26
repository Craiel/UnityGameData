namespace Craiel.UnityGameData.Editor.Common
{
    using UnityEditor;
    using UnityEngine;
    using UnityEssentials.Editor;
    using UnityEssentials.Editor.UserInterface;
    using UnityEssentials.Runtime.Extensions;

    [CustomEditor(typeof(GameDataObject))]
    [CanEditMultipleObjects]
    public abstract class GameDataObjectEditor : EssentialEditorIM, IGameDataCompactEditor
    {
        private static bool objectFoldout;

        // -------------------------------------------------------------------
        // Constructor
        // -------------------------------------------------------------------
        protected GameDataObjectEditor()
        {
            this.DrawObjectProperties = true;
        }

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
        protected bool DrawObjectProperties { get; set; }
        
        protected override void DrawFull()
        {
            if (this.UseDefaultInspector)
            {
                base.DrawFull();
                return;
            }

            var typed = (GameDataObject) this.target;

            if (typed.Deprecated)
            {
                GUILayout.Space(8);
                Layout.BeginBackgroundColor(Colors.OrangeRed);
                if (GUILayout.Button("Undo Deprecated", GUILayout.Height(30)))
                {
                    ((GameDataObject) this.target).Deprecated = false;
                    EditorUtility.SetDirty(this.target);
                }
                Layout.EndBackgroundColor();
                
                return;
            }
            
            if (this.DrawObjectProperties 
                && this.DrawFoldout("Object Properties", ref objectFoldout))
            {
                this.DrawProperty<GameDataObject>(x => x.Guid);
                this.DrawProperty<GameDataObject>(x => x.Name);
                this.DrawProperty<GameDataObject>(x => x.DisplayName);
                this.DrawProperty<GameDataObject>(x => x.Notes);
                this.DrawProperty<GameDataObject>(x => x.Description);
                this.DrawProperty<GameDataObject>(x => x.IconSmall);
                this.DrawProperty<GameDataObject>(x => x.IconLarge);

                GUILayout.Space(8);
                Layout.BeginBackgroundColor(Colors.OrangeRed);
                if (GUILayout.Button("Mark as Deprecated", GUILayout.Height(30)))
                {
                    ((GameDataObject) this.target).Deprecated = true;
                    EditorUtility.SetDirty(this.target);
                }
                Layout.EndBackgroundColor();
            }
            
            this.DoDrawFull();

            this.serializedObject.ApplyModifiedProperties();
        }

        protected abstract void DoDrawFull();
    }
}
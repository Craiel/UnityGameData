namespace Assets.Scripts.Craiel.GameData.Editor.Common
{
    using Enums;
    using Essentials.Editor.NodeEditor;
    using Essentials.Editor.UserInterface;
    using UnityEditor;
    using UnityEngine;
    using Window;

    [CustomEditor(typeof(GameDataObject))]
    [CanEditMultipleObjects]
    public abstract class GameDataObjectEditor : Editor
    {
        private bool objectFoldout = true;

        // -------------------------------------------------------------------
        // Public
        // -------------------------------------------------------------------
        public virtual bool UseDefaultInspector
        {
            get { return false; }
        }

        public override void OnInspectorGUI()
        {
            this.serializedObject.Update();
            

            this.DrawGUI();
            this.serializedObject.ApplyModifiedProperties();
        }

        public void DrawGUI()
        {
            switch (GameDataEditorCore.Config.GetViewMode())
            {
                case GameDataEditorViewMode.Compact:
                {
                    this.DrawCompact();
                    break;
                }
                    
                case GameDataEditorViewMode.Full:
                {
                    this.DrawFull();
                    break;
                }
            }
        }

        public bool DrawFoldout(string title, ref bool toggle)
        {
            toggle = Layout.DrawSectionHeaderToggleWithSection(title, toggle);
            return toggle;
        }
        
        public void DrawProperty(SerializedProperty prop, bool includeChildren = true)
        {
            EditorGUILayout.PropertyField(prop, includeChildren);
        }

        public void DrawProperty(string propName, bool includeChildren = true)
        {
            EditorGUILayout.PropertyField(this.serializedObject.FindProperty(propName), includeChildren);
        }
        
        // -------------------------------------------------------------------
        // Protected
        // -------------------------------------------------------------------
        protected virtual void DrawCompact()
        {
            var typedTarget = (GameDataObject)this.target;
            GUILayout.BeginVertical();
            GUILayout.Box(typedTarget.Name, GameDataNodeStyle.Content);
            GUILayout.EndVertical();
        }

        protected virtual void DrawFull()
        {
            if (this.UseDefaultInspector)
            {
                base.OnInspectorGUI();
            }
            else
            {
                if (this.DrawFoldout("Object Properties", ref this.objectFoldout))
                {
                    this.DrawProperty(this.serializedObject.FindProperty<GameDataObject>(x => x.Guid));
                    this.DrawProperty(this.serializedObject.FindProperty<GameDataObject>(x => x.Name));
                    this.DrawProperty(this.serializedObject.FindProperty<GameDataObject>(x => x.DisplayName));
                    this.DrawProperty(this.serializedObject.FindProperty<GameDataObject>(x => x.Notes));
                    this.DrawProperty(this.serializedObject.FindProperty<GameDataObject>(x => x.Description));
                }
            }
        }
    }
}
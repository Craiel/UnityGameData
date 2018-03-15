using GuiUtils = Craiel.UnityEssentials.Editor.UserInterface.GuiUtils;
using Layout = Craiel.UnityEssentials.Editor.UserInterface.Layout;

namespace Assets.Scripts.Craiel.GameData.Editor.Common
{
    using UnityEditor;
    using UnityEngine;
    using UserInterface;
    using UserInterface;
    using UserInterface;
    using UserInterface;

    [CustomEditor(typeof(GameDataObject))]
    [CanEditMultipleObjects]
    public abstract class GameDataObjectEditor : Editor, IGameDataCompactEditor
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
            this.DrawFull();
            this.serializedObject.ApplyModifiedProperties();
        }
        
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
                    this.DrawProperty(GuiUtils.FindProperty<GameDataObject>(this.serializedObject, x => x.Guid));
                    this.DrawProperty(GuiUtils.FindProperty<GameDataObject>(this.serializedObject, x => x.Name));
                    this.DrawProperty(GuiUtils.FindProperty<GameDataObject>(this.serializedObject, x => x.DisplayName));
                    this.DrawProperty(GuiUtils.FindProperty<GameDataObject>(this.serializedObject, x => x.Notes));
                    this.DrawProperty(GuiUtils.FindProperty<GameDataObject>(this.serializedObject, x => x.Description));
                }
            }
        }
    }
}
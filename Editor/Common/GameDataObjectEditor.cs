namespace Craiel.UnityGameData.Editor.Common
{
    using System;
    using System.Linq.Expressions;
    using UnityEditor;
    using UnityEngine;
    using UnityEssentials.Editor.ReorderableList;
    using UnityEssentials.Editor.UserInterface;

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

        // -------------------------------------------------------------------
        // Protected
        // -------------------------------------------------------------------
        protected bool DrawFoldout(string title, ref bool toggle)
        {
            toggle = Layout.DrawSectionHeaderToggleWithSection(title, toggle);
            return toggle;
        }

        protected virtual void DrawProperty<TSource>(Expression<Func<TSource, object>> expression, GUIContent content)
        {
            DrawProperty(this.serializedObject, expression, content);
        }

        protected virtual void DrawProperty<TSource>(Expression<Func<TSource, object>> expression, bool includeChildren = true)
        {
            DrawProperty(this.serializedObject, expression, includeChildren);
        }

        protected virtual void DrawProperty<TSource>(Expression<Func<TSource, object>> expression, params GUILayoutOption[] options)
        {
            DrawProperty(this.serializedObject, expression, options);
        }

        protected virtual void DrawProperty<TSource>(SerializedObject property, Expression<Func<TSource, object>> expression, GUIContent content = null)
        {
            DrawProperty(property.FindProperty(expression), null, true);
        }

        protected virtual void DrawProperty<TSource>(SerializedObject property, Expression<Func<TSource, object>> expression, bool includeChildren = true)
        {
            DrawProperty(property.FindProperty(expression), null, includeChildren);
        }

        protected virtual void DrawProperty<TSource>(SerializedObject property, Expression<Func<TSource, object>> expression, params GUILayoutOption[] options)
        {
            DrawProperty(property.FindProperty(expression), null, true, options);
        }

        protected virtual void DrawPropertyRelative<TSource>(SerializedProperty property, Expression<Func<TSource, object>> expression, bool includeChildren = true)
        {
            DrawProperty(property.FindPropertyRelative(expression), null, includeChildren);
        }

        protected virtual void DrawPropertyRelative<TSource>(SerializedProperty property, Expression<Func<TSource, object>> expression, params GUILayoutOption[] options)
        {
            DrawProperty(property.FindPropertyRelative(expression), null, true, options);
        }

        protected virtual void DrawProperty(SerializedProperty prop, GUIContent content, bool includeChildren, params GUILayoutOption[] options)
        {
            EditorGUILayout.PropertyField(prop, content, includeChildren, options);
        }

        protected virtual void DrawReorderableList<TSource>(string title, Expression<Func<TSource, object>> expression)
        {
            if (!this.serializedObject.isEditingMultipleObjects)
            {
                ReorderableListGUI.Title(title);
                ReorderableListGUI.ListField(this.serializedObject.FindProperty(expression));
            }
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
}
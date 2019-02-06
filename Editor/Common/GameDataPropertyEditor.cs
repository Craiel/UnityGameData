namespace Craiel.UnityGameData.Editor.Common
{
    using System;
    using System.Linq.Expressions;
    using UnityEditor;
    using UnityEngine;
    using UnityEssentials.Editor.ReorderableList;
    using UnityEssentials.Editor.UserInterface;

    [CustomPropertyDrawer(typeof(GameDataProperty))]
    public abstract class GameDataPropertyEditor : PropertyDrawer
    {
        // -------------------------------------------------------------------
        // Public
        // -------------------------------------------------------------------
        public virtual bool UseDefaultInspector
        {
            get { return false; }
        }

        public override void OnGUI (Rect position, SerializedProperty property, GUIContent label)
        {
            this.Position = position;
            this.TargetProperty = property;
            this.Label = label;
            
            this.Target = fieldInfo.GetValue(property.serializedObject.targetObject);
            
            this.DrawFull();
        }
        
        // -------------------------------------------------------------------
        // Protected
        // -------------------------------------------------------------------
        protected Rect Position;
        protected SerializedProperty TargetProperty;
        protected object Target;
        protected GUIContent Label;
        
        protected bool DrawFoldout(string title, ref bool toggle)
        {
            toggle = Layout.DrawSectionHeaderToggleWithSection(title, toggle);
            return toggle;
        }

        protected virtual void DrawProperty<TSource>(Expression<Func<TSource, object>> expression)
        {
            DrawPropertyRelative(this.TargetProperty, expression);
        }

        protected virtual void DrawProperty<TSource>(Expression<Func<TSource, object>> expression, bool includeChildren)
        {
            DrawPropertyRelative(this.TargetProperty, expression, includeChildren);
        }

        protected virtual void DrawProperty<TSource>(Expression<Func<TSource, object>> expression, params GUILayoutOption[] options)
        {
            DrawPropertyRelative(this.TargetProperty, expression, options);
        }
        
        protected virtual void DrawProperty<TSource>(Expression<Func<TSource, object>> expression, GUIContent content)
        {
            DrawPropertyRelative(this.TargetProperty, expression, content);
        }
        
        protected virtual void DrawPropertyRelative<TSource>(SerializedProperty property, Expression<Func<TSource, object>> expression, bool includeChildren = true)
        {
            DrawProperty(property.FindPropertyRelative(expression), null, includeChildren);
        }
        
        protected virtual void DrawPropertyRelative<TSource>(SerializedProperty property, Expression<Func<TSource, object>> expression, GUIContent content, bool includeChildren = true)
        {
            DrawProperty(property.FindPropertyRelative(expression), content, includeChildren);
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
            ReorderableListGUI.Title(title);
            ReorderableListGUI.ListField(this.TargetProperty.FindPropertyRelative(expression));
        }

        // -------------------------------------------------------------------
        // Protected
        // -------------------------------------------------------------------
        protected virtual void DrawFull()
        {
            if (this.UseDefaultInspector)
            {
                base.OnGUI(this.Position, this.TargetProperty, this.Label);
            }
        }
    }
}
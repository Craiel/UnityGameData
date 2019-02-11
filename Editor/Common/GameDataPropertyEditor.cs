namespace Craiel.UnityGameData.Editor.Common
{
    using System;
    using System.Linq.Expressions;
    using Runtime;
    using UnityEditor;
    using UnityEngine;
    using UnityEssentials.Editor.ReorderableList;
    using UnityEssentials.Editor.UserInterface;
    using UnityEssentials.Runtime.IO;

    [CustomPropertyDrawer(typeof(GameDataProperty))]
    public abstract class GameDataPropertyEditor : PropertyDrawer
    {
        private const float FoldoutPropertyHeight = 25;
        private const float FoldoutBoxMargin = 8;
        private const float LabeledSectionIndent = 6;
        
        private const float ManagedLabelHeight = 25;
        
        // -------------------------------------------------------------------
        // Public
        // -------------------------------------------------------------------
        public virtual bool UseDefaultInspector
        {
            get { return false; }
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            this.SetLocalData(Rect.zero, property, label);

            if (this.UseFoldoutInspector && !this.TargetProperty.isExpanded)
            {
                return FoldoutPropertyHeight;
            }
            
            float result = this.GetPropertyHeight();
            if (this.UseFoldoutInspector)
            {
                result += FoldoutPropertyHeight;
                result += FoldoutBoxMargin * 2;
            }

            return result;
        }

        public override void OnGUI (Rect position, SerializedProperty property, GUIContent label)
        {
            this.SetLocalData(position, property, label);
            
            if (this.UseDefaultInspector)
            {
                base.OnGUI(this.Position, this.TargetProperty, this.Label);
                return;
            }
            
            if(this.UseFoldoutInspector)
            {
                this.TargetProperty.isExpanded = EditorGUI.Foldout(new Rect(this.Position.position, new Vector2(this.Position.width, 25)), this.TargetProperty.isExpanded, this.Label);
                this.Position.y += 25;

                if (this.TargetProperty.isExpanded)
                {
                    this.Position.x += 20;
                    
                    EditorGUI.indentLevel++;
                    this.Position = EditorGUI.IndentedRect(this.Position);
                    this.Position.width -= 40;
                    
                    Rect bodyRect = new Rect(this.Position.position, new Vector2(this.Position.width, this.GetPropertyHeight() + (FoldoutBoxMargin * 2)));
                    EditorGUI.HelpBox (bodyRect, "", MessageType.None);

                    // Pad Position for some margins
                    this.Position.y += FoldoutBoxMargin;
                    this.Position.width -= FoldoutBoxMargin;

                    this.DrawFull(this.Position);

                    EditorGUI.indentLevel--;
                }
                
                return;
            }
            
            this.DrawFull(this.Position);
        }

        // -------------------------------------------------------------------
        // Protected
        // -------------------------------------------------------------------
        protected Rect Position;
        protected SerializedProperty TargetProperty;
        protected object Target;
        protected GUIContent Label;
        protected Type GameDataParentType;
        protected UnityEngine.Object GameDataParent;
        protected ManagedFile GameDataParentAssetPath;

        protected bool UseFoldoutInspector;

        protected virtual float GetPropertyHeight()
        {
            return base.GetPropertyHeight(this.TargetProperty, this.Label);
        }

        protected float GetPropertyHeight<TSource>(Expression<Func<TSource, object>> expression, GUIContent label = null)
        {
            return EditorGUI.GetPropertyHeight(this.TargetProperty.FindPropertyRelative<TSource>(expression), label);
        }
        
        protected virtual void DrawPropertyManaged<TSource>(Expression<Func<TSource, object>> expression)
        {
            this.DrawPropertyManaged<TSource>(expression, null, true);
        }
        
        protected virtual void DrawPropertyManaged<TSource>(Expression<Func<TSource, object>> expression, bool includeChildren)
        {
            this.DrawPropertyManaged<TSource>(expression, null, includeChildren);
        }
        
        protected virtual void DrawPropertyManaged<TSource>(Expression<Func<TSource, object>> expression, GUIContent content)
        {
            this.DrawPropertyManaged<TSource>(expression, content, true);
        }
        
        protected virtual void DrawPropertyManaged<TSource>(Expression<Func<TSource, object>> expression, GUIContent content, bool includeChildren)
        {
            float propertyHeight = this.GetPropertyHeight<TSource>(expression);
            DrawPropertyRelative(this.Position, this.TargetProperty, expression, content, includeChildren);
            this.Position.y += propertyHeight;
        }

        protected virtual void BeginManagedLabeledSection(string label)
        {
            this.DrawLabelManaged(label);
            this.Position.x += LabeledSectionIndent;
            this.Position.width -= LabeledSectionIndent;
        }

        protected virtual void EndManagedLabeledSection()
        {
            this.Position.x -= LabeledSectionIndent;
            this.Position.width += LabeledSectionIndent;
        }

        protected virtual void DrawLabelManaged(string label)
        {
            this.Position.y += 8;
            EditorGUI.LabelField(this.Position, label, EditorStyles.boldLabel);
            this.Position.y += ManagedLabelHeight - 8;
        }

        protected float GetManagedLabelHeight()
        {
            return ManagedLabelHeight;
        }

        protected virtual void DrawProperty<TSource>(Rect rect, Expression<Func<TSource, object>> expression)
        {
            DrawPropertyRelative(rect, this.TargetProperty, expression);
        }

        protected virtual void DrawProperty<TSource>(Rect rect, Expression<Func<TSource, object>> expression, bool includeChildren)
        {
            DrawPropertyRelative(rect, this.TargetProperty, expression, includeChildren);
        }
        
        protected virtual void DrawProperty<TSource>(Rect rect, Expression<Func<TSource, object>> expression, GUIContent content)
        {
            DrawPropertyRelative(rect, this.TargetProperty, expression, content);
        }
        
        protected virtual void DrawPropertyRelative<TSource>(Rect rect, SerializedProperty property, Expression<Func<TSource, object>> expression, bool includeChildren = true)
        {
            DrawProperty(rect, property.FindPropertyRelative(expression), null, includeChildren);
        }
        
        protected virtual void DrawPropertyRelative<TSource>(Rect rect, SerializedProperty property, Expression<Func<TSource, object>> expression, GUIContent content, bool includeChildren = true)
        {
            DrawProperty(rect, property.FindPropertyRelative(expression), content, includeChildren);
        }

        protected virtual void DrawProperty(Rect rect, SerializedProperty prop, GUIContent content, bool includeChildren)
        {
            EditorGUI.PropertyField(rect, prop, content, includeChildren);
        }
        
        protected virtual void DrawReorderableList<TSource>(string title, Expression<Func<TSource, object>> expression)
        {
            ReorderableListGUI.Title(title);
            ReorderableListGUI.ListField(this.TargetProperty.FindPropertyRelative(expression));
        }

        protected virtual void DrawFull(Rect position)
        {
        }
        
        // -------------------------------------------------------------------
        // Private
        // -------------------------------------------------------------------
        private void SetLocalData(Rect position, SerializedProperty property, GUIContent label)
        {
            this.Position = position;
            this.TargetProperty = property;
            this.Label = label;

            this.GameDataParentType = property.serializedObject.targetObject.GetType();
            this.GameDataParent = property.serializedObject.targetObject;
            this.GameDataParentAssetPath = new ManagedFile(AssetDatabase.GetAssetPath(property.serializedObject.targetObject));
            
            this.Target = fieldInfo.GetValue(property.serializedObject.targetObject);
        }
    }
}
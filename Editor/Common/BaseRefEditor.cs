using UnityObjectHelper = Craiel.UnityGameData.Editor.UnityObjectHelper;
using UnityObjectIconSelectorDelegate = Craiel.UnityGameData.Editor.UnityObjectIconSelectorDelegate;
using UnityObjectNameSelectorDelegate = Craiel.UnityGameData.Editor.UnityObjectNameSelectorDelegate;

namespace Craiel.UnityGameData.Editor.Common
{
    using UnityEditor;
    using UnityEngine;

    public abstract class BaseRefEditor : PropertyDrawer
    {
        private const string SelectionIconPath = "GameDataEditor/eyedropper.png";

        private static readonly Color SelectionColor = new Color(1, 0.5f, 0);
        private static readonly Color MissingColor = Color.red;
        private static readonly Color ValidColor = Color.green;

        // -------------------------------------------------------------------
        // Public
        // -------------------------------------------------------------------
        protected BaseRefEditor()
        {
            this.NameSelector = UnityObjectHelper.DefaultNameSelector;
            this.IconSelector = UnityObjectHelper.DefaultIconSelector;
        }

        // -------------------------------------------------------------------
        // Public
        // -------------------------------------------------------------------
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return 30;
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.LabelField(position, label.text);
            if (!string.IsNullOrEmpty(label.text))
            {
                position.x += EditorGUIUtility.labelWidth + 2;
                position.width -= EditorGUIUtility.labelWidth + 2;
            }

            position.y += 2;
            position.height -= 5;

            string activeObjectString = GetActiveObjectString(property);
            var activeObject = GetActiveObject(property);

            var cachedColor = GUI.backgroundColor;

            GUIContent displayContent = new GUIContent();
            if (property.hasMultipleDifferentValues)
            {
                displayContent.text = "-";
            }
            else if (string.IsNullOrEmpty(activeObjectString))
            {
                // No object is selected
                GUI.backgroundColor = SelectionColor;
                Texture iconTexture = EditorGUIUtility.Load(SelectionIconPath) as Texture2D;
                displayContent.text = "[Select Prefab]";
                displayContent.image = iconTexture;
            }
            else if (activeObject == null)
            {
                // Object is selected but no longer found
                GUI.backgroundColor = MissingColor;
                displayContent.text = string.Format("[Missing: {0}]", activeObjectString);
            }
            else
            {
                // Object is selected and valid
                GUI.backgroundColor = ValidColor;
                string assetPath = AssetDatabase.GetAssetPath(activeObject);
                displayContent.image = IconSelector(activeObject, activeObject.GetType());
                displayContent.text = NameSelector(activeObject, assetPath, null);
                displayContent.tooltip = assetPath;
            }

            this.PrepareDrawExtras(ref position, property);

            this.DrawSelection(position, property, displayContent);

            this.DrawExtras(position, property);

            GUI.backgroundColor = cachedColor;
        }

        // -------------------------------------------------------------------
        // Protected
        // -------------------------------------------------------------------
        protected UnityObjectNameSelectorDelegate NameSelector { get; set; }

        protected UnityObjectIconSelectorDelegate IconSelector { get; set; }

        protected abstract string GetActiveObjectString(SerializedProperty property);

        protected abstract Object GetActiveObject(SerializedProperty property);

        protected abstract void SelectObject(Object activeObject, SerializedProperty property);

        protected abstract void PickObject(SerializedProperty property, Rect displayRect);

        protected virtual void PrepareDrawExtras(ref Rect position, SerializedProperty property)
        {
        }

        protected virtual void DrawExtras(Rect position, SerializedProperty property)
        {
        }

        // -------------------------------------------------------------------
        // Private
        // -------------------------------------------------------------------
        private void DrawSelection(Rect position, SerializedProperty property, GUIContent content)
        {
            if (GUI.Button(position, content))
            {
                if (Event.current.button == 0)
                {
                    var mouse = GUIUtility.GUIToScreenPoint(Event.current.mousePosition);
                    var rect = new Rect(position) { x = mouse.x, y = mouse.y };

                    this.PickObject(property, rect);
                }
                else if (Event.current.button == 1)
                {
                    SelectObject(GetActiveObject(property), property);
                }
            }
        }
    }
}

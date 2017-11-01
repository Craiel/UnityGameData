namespace Assets.Scripts.Craiel.GameData.Editor.EditorWindow
{
    using System;
    using UnityEngine;

    public class PanelBase 
    {
        // -------------------------------------------------------------------
        // Public
        // -------------------------------------------------------------------
        [SerializeField]
        public string Title;

        [SerializeField]
        public string Category;

        [SerializeField]
        public bool Active;

        [SerializeField]
        public bool IsInit;

        // -------------------------------------------------------------------
        // Constructor
        // -------------------------------------------------------------------
        public PanelBase(string title, string category = null)
        {
            this.Title = title;

            if (!string.IsNullOrEmpty(category))
            {
                this.Category = category;
            }
        }

        // -------------------------------------------------------------------
        // Public
        // -------------------------------------------------------------------
        public virtual Texture Icon
        {
            get { return null; }
        }
        
        public virtual void Init()
        {
        }

        public virtual void OnInspectorGUI()
        {
        }
        
        public virtual void OnFocus()
        {
        }

        public virtual Type GameDataObjectType
        {
            get { return null; }
        }

        public virtual void SelectItemByGuid(string itemGuid)
        {
        }

        public virtual void SelectItemByObject(object target)
        {
        }
    }
}

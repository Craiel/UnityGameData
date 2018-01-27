namespace Assets.Scripts.Craiel.GameData.Editor.EditorWindow
{
    using System;
    using UnityEngine;

    public class GameDataPanelBase 
    {
        // -------------------------------------------------------------------
        // Constructor
        // -------------------------------------------------------------------
        public GameDataPanelBase(string title, params int[] workSpaces)
            : this(title, null, workSpaces)
        {
            
        }
        
        public GameDataPanelBase(string title, string subFolder, params int[] workSpaces)
        {
            this.Title = title;
            this.SubFolder = subFolder;
            this.WorkSpaces = workSpaces;
        }

        // -------------------------------------------------------------------
        // Public
        // -------------------------------------------------------------------
        [SerializeField]
        public string Title;

        [SerializeField]
        public string SubFolder;

        [SerializeField]
        public bool Active;

        [SerializeField]
        public bool IsInit;

        [SerializeField]
        public int[] WorkSpaces;
        
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

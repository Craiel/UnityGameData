namespace Assets.Scripts.Craiel.GameData.Editor.EditorWindow
{
    using System;
    using Essentials.IO;
    using UnityEngine;

    public class GameDataPanelBase 
    {
        // -------------------------------------------------------------------
        // Constructor
        // -------------------------------------------------------------------
        public GameDataPanelBase(Type dataObjectType, string title, CarbonDirectory subFolder, params int[] workSpaces)
        {
            this.Title = title;
            this.SubFolder = subFolder;
            this.WorkSpaces = workSpaces;
            this.DataObjectType = dataObjectType;
        }

        // -------------------------------------------------------------------
        // Public
        // -------------------------------------------------------------------
        public string Title { get; private set; }

        public CarbonDirectory SubFolder { get; private set; }

        public bool Active { get; protected set; }

        public bool IsInit { get; protected set; }

        public int[] WorkSpaces { get; private set; }
        
        public Type DataObjectType { get; private set; }
        
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

        public virtual void SelectItemByGuid(string itemGuid)
        {
        }

        public virtual void SelectItemByObject(object target)
        {
        }

        public void SetActive(bool active = true)
        {
            this.Active = active;
        }
    }
}

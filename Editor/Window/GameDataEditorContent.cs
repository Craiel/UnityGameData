namespace Craiel.UnityGameData.Editor.Window
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Common;
    using UnityEngine;
    using UnityEssentials.Runtime.Extensions;
    using UnityEssentials.Runtime.IO;

    public class GameDataEditorContent
    {
        private readonly Type dataObjectType;

        private int[] workSpaces;
        
        // -------------------------------------------------------------------
        // Constructor
        // -------------------------------------------------------------------
        public GameDataEditorContent(Type dataObjectType)
        {
            this.dataObjectType = dataObjectType;
            this.Entries = new List<GameDataObject>();
        }

        // -------------------------------------------------------------------
        // Public
        // -------------------------------------------------------------------
        public bool IsActive { get; private set; }
        
        public Texture Icon { get; private set; }

        public string Title { get; private set; }

        public ManagedDirectory SubFolder { get; private set; }

        public IList<GameDataObject> Entries { get; private set; }

        public int[] WorkSpaces
        {
            get { return this.workSpaces; }
        }

        public void Initialize(string title, ManagedDirectory subFolder, int[] newWorkSpaces)
        {
            this.Title = title;
            this.SubFolder = subFolder;

            this.workSpaces = new int[newWorkSpaces.Length];
            Array.Copy(newWorkSpaces, this.workSpaces, this.workSpaces.Length);

            this.Reload();
            this.Refresh();
        }

        public void SetActive(bool isActive = true)
        {
            this.IsActive = isActive;
        }

        public void Focus()
        {
            // TODO
        }
        
        public bool IsOfType(string typeName)
        {
            return this.dataObjectType.Name.Equals(typeName, StringComparison.OrdinalIgnoreCase);
        }

        public bool SelectEntry(GameDataObject entry)
        {
            // TODO
            return false;
        }

        public void Refresh()
        {
            this.Entries.Clear();

            IList<GameDataObject> data = GameDataHelpers.FindGameDataList(this.dataObjectType);
            if (data == null || data.Count == 0)
            {
                return;
            }

            this.Entries.AddRange(data.OrderBy(x => x.Name));
        }

        public void Reload()
        {
            this.Icon = GameDataHelpers.GetIconForBaseType(this.dataObjectType);
        }

        public GameDataObject CreateEntry(string name)
        {
            ManagedDirectory folder = this.SubFolder == null 
                ? new ManagedDirectory(this.Title) 
                : this.SubFolder.ToDirectory(this.Title);

            GameDataObject entry = GameDataHelpers.CreateAsset(this.dataObjectType, folder, name.Trim());
            this.Refresh();

            return entry;
        }

        public void DeleteEntry(GameDataObject entry)
        {
            GameDataHelpers.DeleteAsset(entry);
            this.Refresh();
        }
    }
}

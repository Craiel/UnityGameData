namespace Assets.Scripts.Craiel.GameData.Editor.TreeViewHelpers
{
    using System;
    using System.Collections.Generic;
    using UnityEngine;

    [Serializable]
    public class TreeElement
    {
        [SerializeField] int m_ID;
        [SerializeField] string m_Name;
        [SerializeField] int m_Depth;
        [NonSerialized] TreeElement m_Parent;
        [NonSerialized] List<TreeElement> m_Children;

        public int depth
        {
            get { return this.m_Depth; }
            set { this.m_Depth = value; }
        }

        public TreeElement parent
        {
            get { return this.m_Parent; }
            set { this.m_Parent = value; }
        }

        public List<TreeElement> children
        {
            get { return this.m_Children; }
            set { this.m_Children = value; }
        }

        public bool hasChildren
        {
            get { return this.children != null && this.children.Count > 0; }
        }

        public string name
        {
            get { return this.m_Name; } set { this.m_Name = value; }
        }

        public int id
        {
            get { return this.m_ID; } set { this.m_ID = value; }
        }

        public TreeElement ()
        {
        }

        public TreeElement (string name, int depth, int id)
        {
            this.m_Name = name;
            this.m_ID = id;
            this.m_Depth = depth;
        }
    }

}



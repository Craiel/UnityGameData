namespace Craiel.UnityGameData.Editor.Attributes
{
    using System;
    using UnityEngine;

    public delegate void PrefabLinkPostCreateCallbackDelegate(GameObject instance);
    
    [AttributeUsage(AttributeTargets.Field)]
    public class GameDataPrefabLinkAttribute : Attribute
    {
        // -------------------------------------------------------------------
        // Constructor
        // -------------------------------------------------------------------
        public GameDataPrefabLinkAttribute(string name, Type rootScript)
        {
            this.Name = name;
            this.RootScriptType = rootScript;
        }

        // -------------------------------------------------------------------
        // Public
        // -------------------------------------------------------------------
        public string Name { get; private set; }
        
        public string FileSuffix { get; set; }
        
        public Type RootScriptType { get; private set; }

        public PrefabLinkPostCreateCallbackDelegate PostCreateCallback { get; protected set; }
    }
}
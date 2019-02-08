namespace Craiel.UnityGameData.Editor.Attributes
{
    using System;
    
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
    }
}
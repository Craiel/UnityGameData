namespace Craiel.UnityGameData.Editor.Attributes
{
    using System;
    
    [AttributeUsage(AttributeTargets.Field)]
    public class GameDataPrefabLinkAttribute : Attribute
    {
        // -------------------------------------------------------------------
        // Public
        // -------------------------------------------------------------------
        public GameDataPrefabLinkAttribute(Type rootScript)
        {
            this.RootScriptType = rootScript;
        }
        
        public Type RootScriptType;
    }
}
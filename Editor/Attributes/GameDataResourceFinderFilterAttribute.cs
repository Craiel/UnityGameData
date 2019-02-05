namespace Craiel.UnityGameData.Editor.Attributes
{
    using System;
    
    [AttributeUsage(AttributeTargets.Field)]
    public class GameDataResourceFinderFilterAttribute : Attribute
    {
        // -------------------------------------------------------------------
        // Public
        // -------------------------------------------------------------------
        public Type AttachedScript { get; set; }
    }
}
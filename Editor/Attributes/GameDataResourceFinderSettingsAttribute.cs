namespace Craiel.UnityGameData.Editor.Attributes
{
    using System;
    using Enums;

    [AttributeUsage(AttributeTargets.Field)]
    public class GameDataResourceFinderSettingsAttribute : Attribute
    {
        // -------------------------------------------------------------------
        // Public
        // -------------------------------------------------------------------
        public FinderPopUpStyle Style { get; set; }
        
        public Type AttachedScript { get; set; }
    }
}
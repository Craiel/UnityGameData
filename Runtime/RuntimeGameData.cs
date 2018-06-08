namespace Craiel.UnityGameData.Runtime
{
    using System;
    using UnityEngine;
    using UnityEssentials.Runtime.Resource;

    [Serializable]
    public abstract class RuntimeGameData
    {
        // -------------------------------------------------------------------
        // Public
        // -------------------------------------------------------------------
        [SerializeField]
        public GameDataId Id;

        [SerializeField]
        public string Name;

        [SerializeField]
        public string DisplayName;

        [SerializeField]
        public string Description;
        
        [SerializeField]
        public string IconSmallResourcePath;

        [SerializeField]
        public string IconLargeResourcePath;
        
        public ResourceKey IconSmall { get; private set; }
        
        public ResourceKey IconLarge { get; private set; }
        
        public virtual void PostLoad()
        {
            if (!string.IsNullOrEmpty(this.IconSmallResourcePath))
            {
                this.IconSmall = ResourceKey.Create<Sprite>(this.IconSmallResourcePath);
            }
            
            if (!string.IsNullOrEmpty(this.IconLargeResourcePath))
            {
                this.IconLarge = ResourceKey.Create<Sprite>(this.IconLargeResourcePath);
            }
        }
    }
}

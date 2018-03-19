namespace Craiel.UnityGameData
{
    using System;
    using UnityEngine;

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
        
        public virtual void PostLoad()
        {
        }
    }
}

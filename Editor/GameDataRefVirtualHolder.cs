namespace Craiel.UnityGameData.Editor
{
    using System;
    using UnityEngine;

    [Serializable]
    public class GameDataRefVirtualHolder : ScriptableObject
    {
        // -------------------------------------------------------------------
        // Public
        // -------------------------------------------------------------------
        [SerializeField]
        public GameDataRefBase Ref;

        [SerializeField]
        public string TypeFilter;
    }
}
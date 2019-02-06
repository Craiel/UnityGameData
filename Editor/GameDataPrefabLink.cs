namespace Craiel.UnityGameData.Editor
{
    using System;
    using Common;
    using UnityEngine;

    [Serializable]
    public class GameDataPrefabLink
    {
        // -------------------------------------------------------------------
        // Public
        // -------------------------------------------------------------------
        [SerializeField]
        public GameResourcePrefabRef Ref;

        public bool IsValid()
        {
            return false;
        }

        public string GetPath()
        {
            return null;
        }
    }
}
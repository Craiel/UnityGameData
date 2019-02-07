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
            return this.Ref != null && this.Ref.IsValid();
        }

        public string GetPath()
        {
            return Ref?.GetPath();
        }
    }
}
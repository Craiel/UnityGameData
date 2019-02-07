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
            return this.Ref.IsValid();
        }

        public string GetPath()
        {
            return this.Ref.GetPath();
        }
    }
}
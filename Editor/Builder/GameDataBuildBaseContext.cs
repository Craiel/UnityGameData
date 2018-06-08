namespace Craiel.UnityGameData.Editor.Builder
{
    using System.Collections.Generic;
    using Common;
    using Runtime;
    using UnityEngine;

    public class GameDataBuildBaseContext
    {
        private readonly IDictionary<MonoBehaviour, uint> instanceIdLookup;

        private readonly IDictionary<string, uint> guidIdLookup;

        private uint nextDataId = GameDataId.FirstValidId;

        // -------------------------------------------------------------------
        // Constructor
        // -------------------------------------------------------------------
        public GameDataBuildBaseContext()
        {
            this.instanceIdLookup = new Dictionary<MonoBehaviour, uint>();
            this.guidIdLookup = new Dictionary<string, uint>();
            this.Instances = new List<MonoBehaviour>();
            this.Content = new List<GameDataObject>();
        }

        // -------------------------------------------------------------------
        // Public
        // -------------------------------------------------------------------
        public IList<MonoBehaviour> Instances { get; private set; }

        public IList<GameDataObject> Content { get; private set; }

        public void AddWithoutGuid(GameDataObject obj)
        {
            if (this.Content.Contains(obj))
            {
                Debug.LogWarningFormat("Content was already added: {0} - {1}", obj, obj.name);
                return;
            }

            this.Content.Add(obj);
        }

        public void Add(GameDataObject obj)
        {
            if (string.IsNullOrEmpty(obj.Guid))
            {
                Debug.LogErrorFormat("Invalid Guid: {0} ({1})", obj.name, obj.GetType().Name);
                return;
            }

            if (this.Content.Contains(obj))
            {
                Debug.LogWarningFormat("Content was already added: {0} - {1}", obj, obj.name);
                return;
            }

            if (this.guidIdLookup.ContainsKey(obj.Guid))
            {
                Debug.LogWarningFormat("Guid conflict!  {0} - {1}", obj.Guid, obj.name);
                return;
            }

            this.Content.Add(obj);
            this.guidIdLookup.Add(obj.Guid, this.nextDataId++);
        }

        public void ReindexObjects()
        {
            this.nextDataId = GameDataId.FirstValidId;
            this.guidIdLookup.Clear();

            foreach (GameDataObject dataObject in this.Content)
            {
                this.guidIdLookup.Add(dataObject.Guid, this.nextDataId++);
            }
        }

        public void ReleaseInstances()
        {
            this.Instances.Clear();
            this.instanceIdLookup.Clear();
        }

        public uint GetIdForInstance(MonoBehaviour instance)
        {
            return this.instanceIdLookup[instance];
        }

        public bool EntryExists(string guid)
        {
            return this.guidIdLookup.ContainsKey(guid);
        }

        // -------------------------------------------------------------------
        // Protected
        // -------------------------------------------------------------------
        protected void RegisterInstance(MonoBehaviour instance, uint id)
        {
            this.instanceIdLookup.Add(instance, id);
        }

        protected uint GetIdForGuid(string guid)
        {
            uint result;
            if (!this.guidIdLookup.TryGetValue(guid, out result))
            {
                return GameDataId.InvalidId;
            }

            return this.guidIdLookup[guid];
        }
    }
}
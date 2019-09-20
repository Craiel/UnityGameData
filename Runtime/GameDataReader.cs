namespace Craiel.UnityGameData.Runtime
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Text;
    using Contracts;
    using UnityEngine;
    using UnityEssentials.Runtime;
    using UnityEssentials.Runtime.Collections;
    using UnityEssentials.Runtime.Data.SBT;
    using UnityEssentials.Runtime.Data.SBT.Nodes;
    using UnityEssentials.Runtime.Extensions;
    using UnityEssentials.Runtime.IO;

    public class GameDataReader : IGameDataRuntimeResolver
    {
        private readonly IDictionary<GameDataId, object> gameDataRegister;

        private readonly ExtendedDictionary<string, uint> gameDataIdLookup;

        private readonly IDictionary<Type, IList<object>> gameDataTypeLookup;

        private readonly IDictionary<Type, IList<RuntimeGameData>> data;

        // -------------------------------------------------------------------
        // Constructor
        // -------------------------------------------------------------------
        public GameDataReader()
        {
            this.gameDataRegister = new Dictionary<GameDataId, object>();
            this.gameDataIdLookup = new ExtendedDictionary<string, uint> { EnableReverseLookup = true };
            this.gameDataTypeLookup = new Dictionary<Type, IList<object>>();

            this.data = new Dictionary<Type, IList<RuntimeGameData>>();
        }

        // -------------------------------------------------------------------
        // Public
        // -------------------------------------------------------------------
        public bool IsLoaded { get; private set; }

        public byte[] RawData { get; private set; }

        public void RegisterData<T>()
            where T : RuntimeGameData
        {
            this.data.Add(TypeCache<T>.Value, new List<RuntimeGameData>());
        }

        public void RegisterData(Type dataType)
        {
            this.data.Add(dataType, new List<RuntimeGameData>());
        }

        public string GetGuid(uint id)
        {
            if (!this.IsLoaded)
            {
                GameDataCore.Logger.Warn("Game Data Not loaded");
                return GameDataId.Invalid.Guid;
            }

            string result;
            if (this.gameDataIdLookup.TryGetKey(id, out result))
            {
                return result;
            }

            return null;
        }

        public uint GetId(string guid)
        {
            if (!this.IsLoaded)
            {
                GameDataCore.Logger.Warn("Game Data Not loaded");
                return GameDataId.InvalidId;
            }

            uint result;
            if (this.gameDataIdLookup.TryGetValue(guid, out result))
            {
                return result;
            }

            return GameDataId.InvalidId;
        }

        public GameDataId GetRuntimeId(GameDataRuntimeRefBase runtimeRef)
        {
            if (!runtimeRef.IsValid())
            {
                return GameDataId.Invalid;
            }

            uint internalId = this.GetId(runtimeRef.RefGuid);
            if (internalId == GameDataId.InvalidId)
            {
                return GameDataId.Invalid;
            }

            return new GameDataId(runtimeRef.RefGuid, internalId);
        }

        public bool GetAll<T>(IList<T> target)
        {
            if (!this.IsLoaded)
            {
                GameDataCore.Logger.Warn("Game Data Not loaded");
            }

            IList<object> entries;
            if (this.gameDataTypeLookup.TryGetValue(TypeCache<T>.Value, out entries))
            {
                target.AddRange(entries.Cast<T>());
                return target.Count > 0;
            }

            return false;
        }

        public T Get<T>(GameDataId dataId)
        {
            if (!this.IsLoaded)
            {
                GameDataCore.Logger.Warn("Game Data Not loaded");
            }

            object result;
            if (this.gameDataRegister.TryGetValue(dataId, out result))
            {
                return (T)result;
            }

            return default(T);
        }

        public void Load(Stream stream)
        {
            GameDataCore.Logger.Info("Loading Game Data");

            this.Clear();

            // Read the raw data for later use
            this.RawData = new byte[stream.Length];
            stream.Seek(0, SeekOrigin.Begin);
            stream.Read(this.RawData, 0, this.RawData.Length);
            stream.Seek(0, SeekOrigin.Begin);

            GameDataCore.Logger.Info(" - {0} bytes", this.RawData.Length);

            var db = SBTDictionary.DeserializeCompressed(stream);
            // Now we process what we know
            foreach (Type type in this.data.Keys)
            {
                this.LoadBinaryList(type, db, this.data[type]);
            }

            this.IsLoaded = true;
        }

        public void AddManual<T>(T entry, Func<T, GameDataId> baseRetriever)
            where T : RuntimeGameData
        {
            this.IndexGameData(TypeCache<T>.Value, new List<RuntimeGameData> { entry });
        }

        // -------------------------------------------------------------------
        // Private
        // -------------------------------------------------------------------
        private void Clear()
        {
            foreach (Type type in this.data.Keys)
            {
                this.data[type].Clear();
            }
        }

        private void LoadBinaryList(Type type, SBTDictionary db, IList<RuntimeGameData> target)
        {
            target.Clear();
            ManagedFile listProtoFile = GameDataCore.GameDataListPath.ToFile(type.Name + GameDataCore.GameDataListExtension);
            string id = listProtoFile.GetPathUsingAlternativeSeparator();
            if (db.Contains(id))
            {
                SBTNodeStream stream = db.ReadStream(id);
                using (BinaryReader reader = stream.BeginRead())
                {
                    int count = reader.ReadInt32();
                    for (var i = 0; i < count; i++)
                    {
                        byte[] entryData = new byte[reader.ReadInt32()];
                        reader.Read(entryData, 0, entryData.Length);
                        string stringData = Encoding.UTF8.GetString(entryData);

                        RuntimeGameData entry = (RuntimeGameData)JsonUtility.FromJson(stringData, type);
                        entry.PostLoad();

                        target.Add(entry);
                    }
                }

                GameDataCore.Logger.Info(" -> {0}: {1} Entries", type.Name, target.Count);

                this.IndexGameData(type, target);
            }
        }

        private void IndexGameData(Type type, IEnumerable<RuntimeGameData> entries)
        {
            IList<object> typeList;
            if (!this.gameDataTypeLookup.TryGetValue(type, out typeList))
            {
                typeList = new List<object>();
                this.gameDataTypeLookup.Add(type, typeList);
            }

            foreach (RuntimeGameData entry in entries)
            {
                if (entry.Id.Id == GameDataId.InvalidId || string.IsNullOrEmpty(entry.Id.Guid))
                {
                    GameDataCore.Logger.Error("Game Data Entry has invalid id: {0}", entry.Id);
                    continue;
                }

                this.gameDataRegister.Add(entry.Id, entry);
                this.gameDataIdLookup.Add(entry.Id.Guid, entry.Id.Id);
                typeList.Add(entry);
            }
        }
    }
}

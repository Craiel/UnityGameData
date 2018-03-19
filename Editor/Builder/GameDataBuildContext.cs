namespace Craiel.UnityGameData.Editor.Builder
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Text;
    using Assets.Scripts.Craiel.Editor.GameData;
    using Common;
    using NLog;
    using UnityEngine;
    using UnityEssentials.IO;

    public class GameDataBuildContext : GameDataBuildBaseContext
    {
        private static readonly NLog.Logger Logger = LogManager.GetCurrentClassLogger();

        private readonly GameDataWriter writer;
        
        // -------------------------------------------------------------------
        // Constructor
        // -------------------------------------------------------------------
        public GameDataBuildContext(ManagedFile targetFile)
            : this()
        {
            this.TargetFile = targetFile;
        }

        public GameDataBuildContext()
        {
            this.writer = new GameDataWriter();
            this.BuildData = new Dictionary<Type, IList<byte[]>>();
        }

        // -------------------------------------------------------------------
        // Public
        // -------------------------------------------------------------------
        public ManagedFile TargetFile { get; private set; }
        
        public IDictionary<Type, IList<byte[]>> BuildData { get; private set; }

        public uint GetIdForRef(GameDataRefBase refData)
        {
            if (!refData.IsValid())
            {
                return GameDataId.InvalidId;
            }

            return GetIdForGuid(refData.RefGuid);
        }

        public void AddBuildResult(RuntimeGameData data)
        {
            IList<byte[]> entryList;
            if (!this.BuildData.TryGetValue(data.GetType(), out entryList))
            {
                entryList = new List<byte[]>();
                this.BuildData.Add(data.GetType(), entryList);
            }

            string stringData = JsonUtility.ToJson(data);
            if (string.IsNullOrEmpty(stringData))
            {
                Logger.Warn("Empty data object Ignored: {0} - {1}", data.GetType(), data.Id);
                return;
            }

            entryList.Add(Encoding.UTF8.GetBytes(stringData));
        }

        public void Save()
        {
            if (this.TargetFile == null)
            {
                throw new InvalidOperationException("Context was created without target file, can not save!");
            }

            foreach (Type type in this.BuildData.Keys)
            {
                var closure = type;
                this.writer.AddBinaryListFileContent(type.Name, x => this.SaveBuildData(x, closure));
            }

            this.writer.Save(this.TargetFile);
        }

        public GameDataId BuildGameDataId(GameDataObject objectData)
        {
            if (!objectData.IsValid())
            {
                Logger.Warn("BuildGameDataId() called for invalid ref");
                return GameDataId.Invalid;
            }

            return new GameDataId(objectData.Guid, this.GetIdForGuid(objectData.Guid));
        }

        public GameDataId BuildGameDataId(GameDataRefBase refData)
        {
            if (!refData.IsValid())
            {
                Logger.Warn("BuildGameDataId() called for invalid ref");
                return GameDataId.Invalid;
            }

            return new GameDataId(refData.RefGuid, this.GetIdForGuid(refData.RefGuid));
        }

        // -------------------------------------------------------------------
        // Private
        // -------------------------------------------------------------------
        private void SaveBuildData(BinaryWriter protoWriter, Type protoType)
        {
            IList<byte[]> entries = this.BuildData[protoType];
            protoWriter.Write(entries.Count);
            foreach (byte[] entry in entries)
            {
                protoWriter.Write(entry.Length);
                protoWriter.Write(entry);
            }
        }
    }
}

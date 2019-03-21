namespace Craiel.UnityGameData.Editor.Builder
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Text;
    using Common;
    using Runtime;
    using UnityEngine;
    using UnityEssentials.Runtime.IO;

    public class GameDataBuildContext : GameDataBuildBaseContext
    {
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
                GameDataEditorCore.Logger.Warn("Empty data object Ignored: {0} - {1}", data.GetType(), data.Id);
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

        public GameDataId BuildGameDataId(GameDataObject owner, GameDataObject objectData)
        {
            if (!objectData.IsValid())
            {
                GameDataEditorCore.Logger.Warn("BuildGameDataId() called for invalid ref");
                return GameDataId.Invalid;
            }

            return new GameDataId(objectData.Guid, this.GetIdForGuid(objectData.Guid));
        }

        public GameDataId BuildGameDataId(GameDataObject owner, GameDataRefBase refData)
        {
            if (!refData.IsValid())
            {
                GameDataEditorCore.Logger.Warn("BuildGameDataId() called for invalid ref");
                return GameDataId.Invalid;
            }

            return new GameDataId(refData.RefGuid, this.GetIdForGuid(refData.RefGuid));
        }

        public GameDataId[] BuildGameDataIds<T>(GameDataObject owner, T[] entries)
            where T : GameDataRefBase
        {
            if (entries == null)
            {
                return new GameDataId[0];
            }

            List<GameDataId> result = new List<GameDataId>();
            foreach (T entry in entries)
            {
                GameDataId tagId = this.BuildGameDataId(owner, entry);
                if (tagId == GameDataId.Invalid)
                {
                    continue;
                }

                result.Add(tagId);
            }

            return result.ToArray();
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

namespace Craiel.UnityGameData.Editor
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using Runtime;
    using UnityEssentials.Runtime.Data.SBT;
    using UnityEssentials.Runtime.Data.SBT.Nodes;
    using UnityEssentials.Runtime.IO;

    public class GameDataWriter
    {
        private readonly IDictionary<ManagedFile, ManagedFile> customDataFiles;
        private readonly IDictionary<ManagedFile, Action<BinaryWriter>> customDataContent;

        // -------------------------------------------------------------------
        // Constructor
        // -------------------------------------------------------------------
        public GameDataWriter()
        {
            this.customDataFiles = new Dictionary<ManagedFile, ManagedFile>();
            this.customDataContent = new Dictionary<ManagedFile, Action<BinaryWriter>>();
        }

        // -------------------------------------------------------------------
        // Public
        // -------------------------------------------------------------------
        public void AddCustomFile(ManagedFile fileInDb, ManagedFile fileOnDisk)
        {
            this.customDataFiles.Add(fileInDb, fileOnDisk);
        }

        public void AddCustomFileContent(ManagedFile fileInDb, Action<BinaryWriter> content)
        {
            this.customDataContent.Add(fileInDb, content);
        }

        public void AddBinaryListFileContent(string name, Action<BinaryWriter> content)
        {
           this.AddCustomFileContent(GameDataCore.GameDataListPath.ToFile(string.Concat(name, GameDataCore.GameDataListExtension)), content);
        }

        public void Save(ManagedFile file)
        {
            file.DeleteIfExists();
            var db = new SBTDictionary();

            IList<ManagedFile> fileCheck = new List<ManagedFile>();

            // Store the custom files
            foreach (ManagedFile fileInDb in this.customDataFiles.Keys)
            {
                if (fileCheck.Contains(fileInDb))
                {
                    GameDataEditorCore.Logger.Error("Duplicate file in game data: {0}", fileInDb);
                    continue;
                }

                fileCheck.Add(fileInDb);

                ManagedFile dataFile = this.customDataFiles[fileInDb];
                if (!dataFile.Exists)
                {
                    GameDataEditorCore.Logger.Error("Could not save data file {0}: does not exist", dataFile);
                    continue;
                }

                db.AddArray(fileInDb.GetPath(), dataFile.ReadAsByte());
            }

            // Store custom content
            foreach (ManagedFile fileInDb in this.customDataContent.Keys)
            {
                if (fileCheck.Contains(fileInDb))
                {
                    GameDataEditorCore.Logger.Error("Duplicate file in game data: {0}", fileInDb);
                    continue;
                }

                fileCheck.Add(fileInDb);
                SBTNodeStream stream = db.AddStream(fileInDb.GetUnityPath());
                using (var writer = stream.BeginWrite())
                {
                    this.customDataContent[fileInDb].Invoke(writer);
                    writer.Flush();
                    stream.Flush();
                }
            }
            
            db.SerializeToFileCompressed(file);
        }
    }
}

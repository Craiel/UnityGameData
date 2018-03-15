using ManagedFile = Craiel.UnityEssentials.IO.ManagedFile;

namespace Assets.Scripts.Craiel.GameData.Editor
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using LiteDB;
    using NLog;

    public class GameDataWriter
    {
        private static readonly NLog.Logger Logger = LogManager.GetCurrentClassLogger();

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
            using (var db = new LiteDatabase(file.GetPath()))
            {
                IList<ManagedFile> fileCheck = new List<ManagedFile>();

                // Store the custom files
                foreach (ManagedFile fileInDb in this.customDataFiles.Keys)
                {
                    if (fileCheck.Contains(fileInDb))
                    {
                        Logger.Error("Duplicate file in game data: {0}", fileInDb);
                        continue;
                    }

                    fileCheck.Add(fileInDb);

                    ManagedFile dataFile = this.customDataFiles[fileInDb];
                    if (!dataFile.Exists)
                    {
                        Logger.Error("Could not save data file {0}: does not exist", dataFile);
                        continue;
                    }

                    using (var stream = dataFile.OpenRead())
                    {
                        db.FileStorage.Upload(fileInDb.GetPath(), fileInDb.FileName, stream);
                    }
                }

                // Store custom content
                foreach (ManagedFile fileInDb in this.customDataContent.Keys)
                {
                    if (fileCheck.Contains(fileInDb))
                    {
                        Logger.Error("Duplicate file in game data: {0}", fileInDb);
                        continue;
                    }

                    fileCheck.Add(fileInDb);
                    using (var stream = db.FileStorage.OpenWrite(fileInDb.GetUnityPath(), fileInDb.FileName))
                    {
                        using (var writer = new BinaryWriter(stream))
                        {
                            this.customDataContent[fileInDb].Invoke(writer);
                            writer.Flush();
                            stream.Flush();
                        }
                    }
                }
                
                db.Shrink();
            }
        }
    }
}

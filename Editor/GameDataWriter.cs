namespace Assets.Scripts.Craiel.GameData.Editor
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using Essentials.IO;
    using LiteDB;
    using NLog;

    public class GameDataWriter
    {
        private static readonly NLog.Logger Logger = LogManager.GetCurrentClassLogger();

        private readonly IDictionary<CarbonFile, CarbonFile> customDataFiles;
        private readonly IDictionary<CarbonFile, Action<BinaryWriter>> customDataContent;

        // -------------------------------------------------------------------
        // Constructor
        // -------------------------------------------------------------------
        public GameDataWriter()
        {
            this.customDataFiles = new Dictionary<CarbonFile, CarbonFile>();
            this.customDataContent = new Dictionary<CarbonFile, Action<BinaryWriter>>();
        }

        // -------------------------------------------------------------------
        // Public
        // -------------------------------------------------------------------
        public void AddCustomFile(CarbonFile fileInDb, CarbonFile fileOnDisk)
        {
            this.customDataFiles.Add(fileInDb, fileOnDisk);
        }

        public void AddCustomFileContent(CarbonFile fileInDb, Action<BinaryWriter> content)
        {
            this.customDataContent.Add(fileInDb, content);
        }

        public void AddBinaryListFileContent(string name, Action<BinaryWriter> content)
        {
           this.AddCustomFileContent(GameDataCore.GameDataListPath.ToFile(string.Concat(name, GameDataCore.GameDataListExtension)), content);
        }

        public void Save(CarbonFile file)
        {
            file.DeleteIfExists();
            using (var db = new LiteDatabase(file.GetPath()))
            {
                IList<CarbonFile> fileCheck = new List<CarbonFile>();

                // Store the custom files
                foreach (CarbonFile fileInDb in this.customDataFiles.Keys)
                {
                    if (fileCheck.Contains(fileInDb))
                    {
                        Logger.Error("Duplicate file in game data: {0}", fileInDb);
                        continue;
                    }

                    fileCheck.Add(fileInDb);

                    CarbonFile dataFile = this.customDataFiles[fileInDb];
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
                foreach (CarbonFile fileInDb in this.customDataContent.Keys)
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

namespace Craiel.UnityGameData
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using Events;
    using NLog;
    using UnityEngine;
    using UnityEssentials.Enums;
    using UnityEssentials.Event;
    using UnityEssentials.Resource;
    using UnityEssentials.Scene;
    using UnityEssentials.Singletons;

    public class GameRuntimeData : UnitySingletonBehavior<GameRuntimeData>
    {
        private static readonly NLog.Logger Logger = LogManager.GetCurrentClassLogger();

        private static readonly IList<Type> DataRegister = new List<Type>();

        private readonly GameDataReader reader;
        
        // -------------------------------------------------------------------
        // Constructor
        // -------------------------------------------------------------------
        public GameRuntimeData()
        {
            this.reader = InitializeReader();
        }

        // -------------------------------------------------------------------
        // Public
        // -------------------------------------------------------------------
        public bool IsLoaded { get; private set; }

        public override void Initialize()
        {
            this.RegisterInController(SceneObjectController.Instance, SceneRootCategory.System, true);

            base.Initialize();
        }
        
        public bool GetAll<T>(IList<T> target)
        {
            return this.reader.GetAll(target);
        }

        public T Get<T>(GameDataId dataId)
        {
            return this.reader.Get<T>(dataId);
        }
        
        public GameDataId GetRuntimeId(GameDataRuntimeRefBase refData)
        {
            if (refData == null || string.IsNullOrEmpty(refData.RefGuid))
            {
                return GameDataId.Invalid;
            }

            uint runtimeId = this.reader.GetId(refData.RefGuid);
            return new GameDataId(refData.RefGuid, runtimeId);
        }
        
        public void Load(ResourceKey resourceKey)
        {
            using (var resource = ResourceProvider.Instance.AcquireOrLoadResource<TextAsset>(resourceKey))
            {
                if (resource == null || resource.Data == null)
                {
                    Logger.Error("Could not load RuntimeData from resource {0}", resourceKey);
                    return;
                }
                
                this.Load(resource.Data.bytes);
            }
        }

        public void Load(byte[] data)
        {
            using (var stream = new MemoryStream(data))
            {
                this.reader.Load(stream);
            }
            
            this.IsLoaded = true;
            
            GameEvents.Instance.Send(new EventGameDataLoaded());
        }

        public static void RegisterData<T>()
            where T : RuntimeGameData
        {
            DataRegister.Add(typeof(T));
        }

        public static GameDataReader InitializeReader()
        {
            var reader = new GameDataReader();

            foreach (Type dataType in DataRegister)
            {
                reader.RegisterData(dataType);
            }

            return reader;
        }
    }
}
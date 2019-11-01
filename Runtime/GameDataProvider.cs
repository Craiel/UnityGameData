namespace Craiel.UnityGameData.Runtime
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using Events;
    using UnityEssentials.Runtime.Event;
    using UnityEssentials.Runtime.Extensions;

    public abstract class GameDataProvider<T> : IEnumerable<GameDataId>, IDisposable
         where T : RuntimeGameData
    {
        private readonly IDictionary<GameDataId, T> idLookup;

        private BaseEventSubscriptionTicket gameDataLoadedTicket;

        private bool isLoaded;

        // -------------------------------------------------------------------
        // Constructor
        // -------------------------------------------------------------------
        protected GameDataProvider()
        {
            this.idLookup = new Dictionary<GameDataId, T>();
            this.Values = new List<T>();
            this.FilteredList = new List<T>();

            GameEvents.Subscribe<EventGameDataLoaded>(this.OnGameDataLoaded, out this.gameDataLoadedTicket);
        }

        public IList<T> Values { get; private set; }

        public IList<T> FilteredList { get; private set; }

        // -------------------------------------------------------------------
        // Public
        // -------------------------------------------------------------------
        public int Count => this.idLookup.Count;
        
        IEnumerator<GameDataId> IEnumerable<GameDataId>.GetEnumerator()
        {
            return this.idLookup.Keys.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.idLookup.Keys.GetEnumerator();
        }

        public T Get(GameDataId id)
        {
            if (!this.isLoaded)
            {
                this.Reload();
            }

            T result;
            if (this.idLookup.TryGetValue(id, out result))
            {
                return result;
            }

            return null;
        }

        public virtual GameDataProvider<T> Reset()
        {
            if (!this.isLoaded)
            {
                this.Reload();
            }

            this.FilteredList.Clear();
            this.FilteredList.AddRange(this.Values);

            return this;
        }

        public virtual T GetRandom()
        {
            if (this.FilteredList.Count == 0)
            {
                return default;
            }

            return this.FilteredList[UnityEngine.Random.Range(0, this.FilteredList.Count)];
        }

        public virtual void Reload()
        {
            this.isLoaded = true;

            this.idLookup.Clear();
            this.Values.Clear();
            this.FilteredList.Clear();

            if (!GameRuntimeData.Instance.GetAll(this.Values))
            {
                GameDataCore.Logger.Warn("Game Data had no Monster information!");
                return;
            }

            for (var i = 0; i < this.Values.Count; i++)
            {
                T data = this.Values[i];
                this.idLookup.Add(data.Id, data);
            }
            
            this.PostLoad();
        }

        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        // -------------------------------------------------------------------
        // Protected
        // -------------------------------------------------------------------
        protected virtual void PostLoad()
        {
        }

        // -------------------------------------------------------------------
        // Private
        // -------------------------------------------------------------------
        private void Dispose(bool isDisposing)
        {
            if (isDisposing)
            {
                GameEvents.Unsubscribe(ref this.gameDataLoadedTicket);
            }
        }
        
        private void OnGameDataLoaded(EventGameDataLoaded eventdata)
        {
            GameDataCore.Logger.Info("Game Data Changed, Reloading Monster Provider");
            this.Reload();
        }
    }
}

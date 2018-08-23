namespace Craiel.UnityGameData.Runtime
{
    using System.Collections;
    using System.Collections.Generic;
    using Events;
    using UnityEssentials.Runtime.Event;
    using UnityEssentials.Runtime.Extensions;

    public abstract class GameDataProvider<T> : IEnumerable<GameDataId>
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

            this.gameDataLoadedTicket = GameEvents.Instance.Subscribe<EventGameDataLoaded>(this.OnGameDataLoaded);
        }

        public IList<T> Values { get; private set; }

        public IList<T> FilteredList { get; private set; }

        // -------------------------------------------------------------------
        // Public
        // -------------------------------------------------------------------
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
        }

        // -------------------------------------------------------------------
        // Private
        // -------------------------------------------------------------------
        private void OnGameDataLoaded(EventGameDataLoaded eventdata)
        {
            GameDataCore.Logger.Info("Game Data Changed, Reloading Monster Provider");
            this.Reload();
        }
    }
}

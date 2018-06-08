namespace Craiel.UnityGameData.Runtime
{
    using System;
    using System.ComponentModel;
    using UnityEngine;

    [TypeConverter(typeof(GameDataIdTypeConverter))]
    [Serializable]
    public struct GameDataId
    {
        public const int InvalidId = 0;
        public const int FirstValidId = 1;

        public static readonly GameDataId Invalid = new GameDataId();
        
        // -------------------------------------------------------------------
        // Constructor
        // -------------------------------------------------------------------
        public GameDataId(string guid, uint id)
        {
            this.Guid = guid;
            this.Id = id;
            this.HasGuid = !string.IsNullOrEmpty(guid);
        }

        public GameDataId(string guid)
            : this(guid, InvalidId)
        {
        }

        public GameDataId(uint id)
            : this(null, id)
        {
        }

        // -------------------------------------------------------------------
        // Public
        // -------------------------------------------------------------------
        [SerializeField]
        public string Guid;

        [SerializeField]
        public uint Id;

        public readonly bool HasGuid;

        public static bool operator ==(GameDataId value1, GameDataId value2)
        {
            return value1.Equals(value2);
        }

        public static bool operator !=(GameDataId value1, GameDataId value2)
        {
            return !(value1 == value2);
        }

        public bool Equals(GameDataId other)
        {
            bool noneHasId = this.Id == InvalidId && other.Id == InvalidId;
            bool bothHaveId = this.Id != InvalidId && other.Id != InvalidId;
            bool noneHasGuid = !this.HasGuid && !other.HasGuid;
            bool bothHaveGuid = this.HasGuid && other.HasGuid;

            if (bothHaveId)
            {
                // Easy case, simple id compare
                return (this.Id != InvalidId && other.Id != InvalidId) && this.Id == other.Id;
            }

            if (bothHaveGuid)
            {
                // both have a guid, compare on that
                return string.Equals(this.Guid, other.Guid, StringComparison.OrdinalIgnoreCase);
            }

            // At this point they can only be equal if they have nothing
            return noneHasId && noneHasGuid;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
            {
                return false;
            }

            return obj is GameDataId && this.Equals((GameDataId)obj);
        }

        public override int GetHashCode()
        {
            if (this.Id == InvalidId && this.HasGuid)
            {
                // If no valid id is given guid will determine the hashcode
                return this.Guid.GetHashCode();
            }
            
            return this.Id.GetHashCode();
        }

        public override string ToString()
        {
            return string.Format("({0}:{1})", this.Guid ?? string.Empty, this.Id);
        }
    }
}

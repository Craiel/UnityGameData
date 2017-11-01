namespace Assets.Scripts.Craiel.GameData.Editor.Common
{
    using System;
    using Attributes;
    using Builder;
    using Contracts;
    using UnityEngine;

#if UNITY_EDITOR
    using UnityEditor;
#endif

    [Serializable]
    public abstract class GameDataObject : ScriptableObject, IGameDataBuildContent, IGameDataObject
    {
        // -------------------------------------------------------------------
        // Public
        // -------------------------------------------------------------------
        [ReadOnly]
        [SerializeField]
        public string Guid;

        [SerializeField]
        public string Name;

        [SerializeField]
        public string DisplayName;

        [SerializeField]
        [Multiline]
        public string Notes;

        [SerializeField]
        [Multiline]
        public string Description;

        public bool IsValid()
        {
            return !string.IsNullOrEmpty(this.Guid);
        }
        
        public override string ToString()
        {
            return this.Name;
        }
        
        public abstract void Build(GameDataBuildContext context);

        public virtual void Validate(GameDataBuildValidationContext context)
        {
            if (string.IsNullOrEmpty(this.Guid))
            {
                context.Error(this, this, null, "Missing Guid");
            }

            if (string.IsNullOrEmpty(this.Name))
            {
                context.Error(this, this, null, "Missing Name");
            }
        }

        public virtual void Upgrade(GameDataBuildContext context)
        {
            if (this.DoUpgrade(context))
            {
#if UNITY_EDITOR
                EditorUtility.SetDirty(this);
#endif
            }
        }

        // -------------------------------------------------------------------
        // Protected
        // -------------------------------------------------------------------
        protected virtual bool DoUpgrade(GameDataBuildContext context)
        {
            return false;
        }

        protected void BuildBase(GameDataBuildContext context, RuntimeGameData target)
        {
            target.Id = context.BuildGameDataId(this);
            target.Name = this.Name;
            target.DisplayName = this.DisplayName;
            target.Description = this.Description;
        }
    }
}
namespace Craiel.UnityGameData.Editor.Common
{
    using System;
    using Attributes;
    using Builder;
    using Runtime;
    using Runtime.Attributes;
    using Runtime.Contracts;
    using UnityEditor;
    using UnityEngine;

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
        
        [SerializeField]
        [GameDataIcon]
        public GameResourceSpriteRef IconSmall;
        
        [SerializeField]
        public GameResourceSpriteRef IconLarge;

        [SerializeField]
        public bool Deprecated;
        
        public bool IsValid()
        {
            return !string.IsNullOrEmpty(this.Guid);
        }
        
        public override string ToString()
        {
            return this.Name;
        }

        public void Build(GameDataBuildContext context)
        {
            if (this.Deprecated)
            {
                // Won't build deprecated data
                return;
            }
            
            this.DoBuild(context);
        }

        public void Validate(GameDataBuildValidationContext context)
        {
            if (this.Deprecated)
            {
                // Don't care about validation for deprecated data
                return;
            }
            
            if (string.IsNullOrEmpty(this.Guid))
            {
                context.Error(this, this, null, "Missing Guid");
            }

            if (string.IsNullOrEmpty(this.Name))
            {
                context.Error(this, this, null, "Missing Name");
            }
            
            this.IconSmall.Validate(this, context, false);
            this.IconLarge.Validate(this, context, false);

            this.DoValidate(context);
        }

        public virtual void Upgrade(GameDataBuildContext context)
        {
            if (this.Deprecated)
            {
                return;
            }
            
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
        protected virtual void DoBuild(GameDataBuildContext context)
        {
        }
        
        protected virtual void DoValidate(GameDataBuildValidationContext context)
        {
        }
        
        protected virtual bool DoUpgrade(GameDataBuildContext context)
        {
            return false;
        }

        protected void BuildBase(GameDataBuildContext context, RuntimeGameData target)
        {
            if (this.Deprecated)
            {
                GameDataCore.Logger.Warn("BuildBase Calld on Deprecated Object: {0}", this.Name);
                return;
            }
            
            target.Id = context.BuildGameDataId(this);
            target.Name = this.Name;
            target.DisplayName = this.DisplayName;
            target.Description = this.Description;
            
            if (this.IconSmall != null && this.IconSmall.IsValid())
            {
                target.IconSmallResourcePath = this.IconSmall.GetPath();
            }

            if (this.IconLarge != null && this.IconLarge.IsValid())
            {
                target.IconLargeResourcePath = this.IconLarge.GetPath();
            }
        }
    }
}
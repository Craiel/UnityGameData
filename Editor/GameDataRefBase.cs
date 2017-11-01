namespace Assets.Scripts.Craiel.Editor.GameData
{
    using System.Collections.Generic;
    using Craiel.GameData.Editor.Builder;
    using UnityEngine;
    
    public abstract class GameDataRefBase
    {
        // -------------------------------------------------------------------
        // Public
        // -------------------------------------------------------------------
        [SerializeField]
        public string RefGuid;

        public bool IsValid()
        {
            return !string.IsNullOrEmpty(this.RefGuid);
        }

        public void Reset()
        {
            this.RefGuid = null;
        }
        
        public static void ValidateRefList<T>(object owner, object source, IList<T> refList, GameDataBuildValidationContext context)
            where T : GameDataRefBase
        {
            if (refList.Count == 0)
            {
                return;
            }

            IList<T> emptyEntries = new List<T>();
            foreach (T refData in refList)
            {
                if (refData == null || !refData.IsValid())
                {
                    emptyEntries.Add(refData);
                }
                else
                {
                    refData.Validate(owner, context);
                }
            }

            if (emptyEntries.Count > 0)
            {
                context.WarningFormat(owner, source,
                    ((localOwner, entry) => GameDataBuildValidationFixers.ListRemoveInvalidFixer(localOwner, refList, emptyEntries)),
                    "Ref List has {0} empty entries", emptyEntries.Count);
            }
        }
        
        public bool Validate(object owner, GameDataBuildValidationContext context, bool isOptional = true, bool warnIfMissing = false)
        {
            if (!this.IsValid())
            {
                if (!isOptional)
                {
                    context.ErrorFormat(owner, this, null, "Static Data Ref is not optional: {0}", this.GetType().Name);
                    return false;
                }

                if (warnIfMissing)
                {
                    context.WarningFormat(owner, this, null, "Static Data Ref is recommended to be set: {0}", this.GetType().Name);
                    return false;
                }

                return false;
            }

            if (!context.EntryExists(this.RefGuid))
            {
                context.ErrorFormat(owner, this, GameDataBuildValidationFixers.ResetReferenceFixer, "Static Data Ref is missing: {0}", this.RefGuid);
                return false;
            }

            return true;
        }
    }
}

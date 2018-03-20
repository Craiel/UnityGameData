namespace Craiel.UnityGameData.Editor.Builder
{
    using System.Collections;
    using System.Collections.Generic;
#if UNITY_EDITOR
    using UnityEditor;
#endif

    public static class GameDataBuildValidationFixers
    {
        // -------------------------------------------------------------------
        // Public
        // -------------------------------------------------------------------
        public static bool ResetReferenceFixer(object owner, object entry)
        {
            var typed = entry as GameDataRefBase;
            if (typed != null)
            {
                typed.Reset();
                return true;
            }

            return false;
        }

        public static bool ListClearFixer(object owner, IList list)
        {
            if (list.Count == 0)
            {
                return false;
            }

            list.Clear();
            return true;
        }

        public static bool ListRemoveExcessFixer(object owner, IList list, int itemsToKeep = 1)
        {
            if (list.Count <= itemsToKeep)
            {
                return false;
            }

            while (list.Count > itemsToKeep)
            {
                list.RemoveAt(list.Count - 1);
            }

            return true;
        }

        public static bool ListRemoveInvalidFixer<T>(object owner, IList<T> list, T entry)
        {
            list.Remove(entry);
            return true;
        }

        public static bool ListRemoveInvalidFixer<T>(object owner, IList<T> list, IList<T> entries)
        {
            if (entries.Count == 0)
            {
                return false;
            }
            
            foreach (T entry in entries)
            {
                list.Remove(entry);
            }

            return true;
        }

        public static bool ListRemoveInvalidFixer(object owner, IList list, object entry)
        {
            list.Remove(entry);
            return true;
        }

        public static void MarkOwnerDirty(object owner)
        {
#if UNITY_EDITOR
            var typed = owner as UnityEngine.Object;
            if (typed != null)
            {
                EditorUtility.SetDirty(typed);
            }
#endif
        }
    }
}
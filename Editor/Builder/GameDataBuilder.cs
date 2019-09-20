#if UNITY_EDITOR
namespace Craiel.UnityGameData.Editor.Builder
{
    using Common;
    using UnityEditor;
    using UnityEssentials.Runtime;
    using UnityEssentials.Runtime.IO;

    public static class GameDataBuilder
    {
        private const string BuildProgressTitle = "Building Static Data";
        private const string ValidateProgressTitle = "Validating Static Data";
        private const string UpgradeProgressTitle = "Upgrading Static Data";

        private const int ProgressUpdateInterval = 10;

        private static string activeProgressTitle;
        private static int activeProgressStageMax;
        private static int activeProgressStage;

        // -------------------------------------------------------------------
        // Public
        // -------------------------------------------------------------------
        public static ManagedFile TargetFile { get; set; }

        public static void Build()
        {
            if (TargetFile == null || TargetFile.IsNull)
            {
                GameDataEditorCore.Logger.Error("GameDataBuilder Target file is not set!");
                return;
            }

            TargetFile.DeleteIfExists();

            var context = new GameDataBuildContext(TargetFile);

            activeProgressTitle = BuildProgressTitle;
            activeProgressStageMax = 3;
            activeProgressStage = 0;

            // First we build the scriptable objects
            FindStaticScriptableObjects(context);

            activeProgressStage++;
            BuildStaticScriptableObjects(context);

            activeProgressStage++;
            context.Save();

            AssetDatabase.ImportAsset(TargetFile.GetUnityPath());

            EditorUtility.ClearProgressBar();
        }

        public static GameDataBuildValidationContext Validate()
        {
            var context = new GameDataBuildValidationContext();

            activeProgressTitle = ValidateProgressTitle;
            activeProgressStageMax = 3;
            activeProgressStage = 0;

            // First we build the scriptable objects
            FindStaticScriptableObjects(context);

            activeProgressStage++;
            ValidateStaticScriptableObjects(context);

            EditorUtility.ClearProgressBar();

            return context;
        }

        public static void Upgrade()
        {
            var context = new GameDataBuildContext();

            activeProgressTitle = UpgradeProgressTitle;
            activeProgressStageMax = 2;
            activeProgressStage = 0;

            // First we build the scriptable objects
            FindStaticScriptableObjects(context);

            activeProgressStage++;
            UpgradeStaticScriptableObjects(context);

            EditorUtility.ClearProgressBar();
        }

        // -------------------------------------------------------------------
        // Private
        // -------------------------------------------------------------------
        private static void UpdateProgress(int subProgressCurrent, float subProgressMax, string subProgressTitle)
        {
            float stageValue = 1 / (float)activeProgressStageMax;
            float progress = activeProgressStage * stageValue;
            progress += (subProgressCurrent / subProgressMax) * stageValue;
            EditorUtility.DisplayProgressBar(activeProgressTitle, subProgressTitle, progress);
        }

        private static void FindStaticScriptableObjects(GameDataBuildBaseContext context)
        {
            string[] guids = AssetDatabase.FindAssets("t:" + TypeCache<GameDataObject>.Value.Name);
            for (var i = 0; i < guids.Length; i++)
            {
                if (i % ProgressUpdateInterval == 0)
                {
                    UpdateProgress(i, guids.Length, string.Format("Locating {0} Data Entries", guids.Length));
                }

                string path = AssetDatabase.GUIDToAssetPath(guids[i]);
                GameDataObject obj = AssetDatabase.LoadAssetAtPath<GameDataObject>(path);
                context.Add(obj);
            }
        }

        private static void BuildStaticScriptableObjects(GameDataBuildContext context)
        {
            for (var i = 0; i < context.Content.Count; i++)
            {
                if (i % ProgressUpdateInterval == 0)
                {
                    UpdateProgress(i, context.Content.Count, string.Format("Processing {0} Data Entries", context.Content.Count));
                }

                context.Content[i].Build(context);
            }
        }

        private static void ValidateStaticScriptableObjects(GameDataBuildValidationContext context)
        {
            for (var i = 0; i < context.Content.Count; i++)
            {
                if (i % ProgressUpdateInterval == 0)
                {
                    UpdateProgress(i, context.Content.Count, string.Format("Validating {0} Data Entries", context.Content.Count));
                }

                context.Content[i].Validate(context);
            }
        }

        private static void UpgradeStaticScriptableObjects(GameDataBuildContext context)
        {
            for (var i = 0; i < context.Content.Count; i++)
            {
                if (i % ProgressUpdateInterval == 0)
                {
                    UpdateProgress(i, context.Content.Count, string.Format("Upgrading {0} Data Entries", context.Content.Count));
                }

                context.Content[i].Upgrade(context);
            }
        }
    }
}
#endif
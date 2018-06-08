namespace Craiel.UnityGameData.Editor.Builder
{
    using System.Collections.Generic;
    using Runtime.Contracts;

    public class GameDataBuildValidationContext : GameDataBuildBaseContext, IGameDataRuntimeValidationContext
    {
        // -------------------------------------------------------------------
        // Constructor
        // -------------------------------------------------------------------
        public GameDataBuildValidationContext()
        {
            this.Errors = new List<GameDataBuildValidationResult>();
            this.Warnings = new List<GameDataBuildValidationResult>();
        }

        // -------------------------------------------------------------------
        // Public
        // -------------------------------------------------------------------
        public IList<GameDataBuildValidationResult> Errors { get; private set; }

        public IList<GameDataBuildValidationResult> Warnings { get; private set; }

        public void Warning(object owner, object source, GameDataValidationFixDelegate fixDelegate, string message)
        {
            this.Warnings.Add(new GameDataBuildValidationResult(message, message, owner, source, fixDelegate));
        }

        public void WarningFormat(object owner, object source, GameDataValidationFixDelegate fixDelegate, string message, params object[] formatArguments)
        {
            string formattedMessage = string.Format(message, formatArguments);
            this.Warnings.Add(new GameDataBuildValidationResult(message, formattedMessage, owner, source, fixDelegate));
        }
        
        public void Error(object owner, object source, GameDataValidationFixDelegate fixDelegate, string message)
        {
            this.Errors.Add(new GameDataBuildValidationResult(message, message, owner, source, fixDelegate));
        }
        
        public void ErrorFormat(object owner, object source, GameDataValidationFixDelegate fixDelegate, string message, params object[] formatArguments)
        {
            string formattedMessage = string.Format(message, formatArguments);
            this.Errors.Add(new GameDataBuildValidationResult(message, formattedMessage, owner, source, fixDelegate));
        }
    }
}
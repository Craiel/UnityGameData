namespace Craiel.UnityGameData.Editor.Builder
{
    using Runtime.Contracts;

    public class GameDataBuildValidationResult
    {
        // -------------------------------------------------------------------
        // Constructor
        // -------------------------------------------------------------------
        public GameDataBuildValidationResult(string rawMessage, string formattedMessage, object owner, object source, GameDataValidationFixDelegate fixDelegate)
        {
            this.RawMessage = rawMessage;
            this.FormattedMessage = formattedMessage;
            this.Owner = owner;
            this.Source = source;
            this.FixDelegate = fixDelegate;
        }
        
        // -------------------------------------------------------------------
        // Public
        // -------------------------------------------------------------------
        public string RawMessage { get; private set; }

        public string FormattedMessage { get; private set; }

        public object Source { get; private set; }

        public object Owner { get; private set; }

        public GameDataValidationFixDelegate FixDelegate { get; private set; }
    }
}

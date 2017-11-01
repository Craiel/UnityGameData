namespace Assets.Scripts.Craiel.GameData.Editor.Attributes
{
    using NLog;
    using UnityEngine;

    public class HelpBoxAttribute : PropertyAttribute
    {
        // -------------------------------------------------------------------
        // Constructor
        // -------------------------------------------------------------------
        public HelpBoxAttribute(string message)
            : this(message, LogLevel.Info)
        {
        }

        public HelpBoxAttribute(string message, LogLevel level)
        {
            this.Message = message;
            this.Level = level;
        }

        // -------------------------------------------------------------------
        // Public
        // -------------------------------------------------------------------
        public string Message { get; private set; }
        public LogLevel Level { get; private set; }
    }
}
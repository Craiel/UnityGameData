namespace Craiel.UnityGameData.Editor.Window
{
    using UnityEngine;

    public static class GameDataNodeStyle
    {
        static GameDataNodeStyle()
        {
            Content = new GUIStyle("OL Box");

        }

        public static GUIStyle Content { get; private set; }
    }
}

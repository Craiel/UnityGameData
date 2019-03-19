namespace Craiel.UnityGameData.Editor
{
    using UnityEngine;

    public static class GameDataStyles
    {
        public const int FinderPopupWidth = 530;
        public const int FinderPopupHeight = 530;
        
        public static readonly Vector2 FinderPopupSize = new Vector2(FinderPopupWidth, FinderPopupHeight);
            
        public static readonly GUIContent EmptyGuiContent = new GUIContent(string.Empty);

        private static GUIStyle finderButton;
        private static GUISkin finderSkin;

        private static GUISkin resetSkin;

        // -------------------------------------------------------------------
        // Public
        // -------------------------------------------------------------------
        public static GUIStyle FinderButton
        {
            get
            {
                return finderButton ??
                       (finderButton = new GUIStyle(GUI.skin.button)
                       {
                           alignment = TextAnchor.MiddleLeft,
                           fixedHeight = 30,
                           fontSize = 12,
                           margin = new RectOffset(2, 2, 2, 2)
                       });
            }
        }

        public static GUISkin FinderSkin
        {
            get
            {
                if (finderSkin == null)
                {
                    finderSkin = Object.Instantiate(GUI.skin);
                    finderSkin.button = FinderButton;
                }

                return finderSkin;
            }
        }

        public static void BeginStyle(GUISkin style)
        {
            if (resetSkin == null)
            {
                resetSkin = GUI.skin;
                GUI.skin = style;
            }
        }

        public static void EndStyle()
        {
            GUI.skin = resetSkin;
            resetSkin = null;
        }
    }
}

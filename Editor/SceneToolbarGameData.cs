namespace Craiel.UnityGameData.Editor
{
    using UnityEditor;
    using UnityEngine;
    using UnityEssentials.Editor;
    using Window;

    public class SceneToolbarGameData : SceneToolbarWidget
    {
        // -------------------------------------------------------------------
        // Public
        // -------------------------------------------------------------------
        public override void DrawGUI()
        {
            base.DrawGUI();
            
            if (EditorGUILayout.DropdownButton(new GUIContent("GameData"), FocusType.Passive, "ToolbarDropDown"))
            {
                var menu = new GenericMenu();
                menu.AddItem(new GUIContent("GameData Editor"), false, GameDataEditorWindow.OpenWindow);
                menu.AddSeparator("");
                menu.AddItem(new GUIContent("Reconfigure"), false, x => GameDataEditorCore.Configure(true), null);
                menu.ShowAsContext();
                Event.current.Use();
            }
        }
    }
}
using SceneToolbarWidget = Craiel.UnityEssentials.Editor.SceneToolbarWidget;

namespace Assets.Scripts.Craiel.GameData.Editor
{
    using Essentials.Editor;
    using UnityEditor;
    using UnityEngine;
    using Window;

    public class SceneToolbarGameData : SceneToolbarWidget
    {
        // -------------------------------------------------------------------
        // Public
        // -------------------------------------------------------------------
        public override void OnGUi()
        {
            base.OnGUi();
            if (EditorGUILayout.DropdownButton(new GUIContent("GameData"), FocusType.Passive, "ToolbarDropDown"))
            {
                var menu = new GenericMenu();
                menu.AddItem(new GUIContent("GameData Editor"), false, GameDataEditorWindow.OpenWindow);
                menu.AddSeparator("");
                menu.ShowAsContext();
                Event.current.Use();
            }
        }
    }
}
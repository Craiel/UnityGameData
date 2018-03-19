namespace Craiel.UnityGameData.Editor.Window
{
    using System;
    using UnityEditor;
    using UnityEngine;

    public class GameDataCreatePrompt : EditorWindow
    {
        private const string TextFieldName = "gameDataCreateTextField";
        
        private string newName = string.Empty;

        private bool focused;

        private Action<string> callback;

        // -------------------------------------------------------------------
        // Public
        // -------------------------------------------------------------------
        public void Init(Action<string> newCallback)
        {
            this.callback = newCallback;

            this.minSize = this.maxSize = new Vector2(450, 100);

            this.titleContent = new GUIContent("New Item");
            this.ShowAuxWindow();
            this.Focus();
        }

        public void OnGUI()
        {
            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();

            GUILayout.BeginVertical(GUILayout.Width(400));
            GUILayout.FlexibleSpace();
            
            GUI.SetNextControlName(TextFieldName);
            this.newName = EditorGUILayout.TextField("Name", this.newName);
            
            GUILayout.Space(10);
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Ok", GUILayout.Height(30)))
            {
                this.CreateItem();
            }

            if (GUILayout.Button("Cancel", GUILayout.Height(30)))
            {
                this.Close();
            }
            GUILayout.EndHorizontal();
            
            GUILayout.FlexibleSpace();
            GUILayout.EndVertical();

            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
            if (!this.focused)
            {
                this.FocusTextField();
                this.focused = true;
            }

            if (Event.current.isKey)
            {
                switch (Event.current.keyCode)
                {
                    case KeyCode.Return:
                    case KeyCode.KeypadEnter:
                        this.CreateItem();
                        Event.current.Use();
                        break;

                    case KeyCode.Escape:
                        Event.current.Use();
                        this.Close();
                        break;
                }
            }
        }

        // -------------------------------------------------------------------
        // Private
        // -------------------------------------------------------------------
        private void CreateItem()
        {
            var s = this.newName.Trim();
            if (!string.IsNullOrEmpty(s))
            {
                this.callback(s);
            }

            this.Close();
        }

        private void FocusTextField()
        {
            EditorGUI.FocusTextInControl(TextFieldName);
        }
    }
}
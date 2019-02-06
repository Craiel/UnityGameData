namespace Craiel.UnityGameData.Editor
{
    using Common;
    using UnityEditor;
    using UnityEngine;
    using UnityEssentials.Editor;
    using UnityEssentials.Editor.UserInterface;

    [CustomPropertyDrawer (typeof (GameDataPrefabLink))]
    public class GameDataPrefabLinkEditor : GameDataPropertyEditor
    {
        // -------------------------------------------------------------------
        // Protected
        // -------------------------------------------------------------------
        protected override void DrawFull()
        {
            base.DrawFull();

            this.DrawProperties();
        }
        
        // -------------------------------------------------------------------
        // Private
        // -------------------------------------------------------------------
        private void DrawProperties()
        {
            this.DrawLinkedPrefabControls();
        }

        private void DrawLinkedPrefabControls()
        {
            var typedTarget = this.Target as GameDataPrefabLink;
            
            this.DrawProperty<GameDataPrefabLink>(x => x.Ref, new GUIContent("Linked Prefab"));
            
            var region = LayoutRegion.StartAligned(isHorizontal: true);
            
            GUILayout.Space(EditorGUIUtility.labelWidth + 10);
            if (GUILayout.Button("Check", GUILayout.Width(100)))
            {
            }
            
            if (typedTarget.Ref == null || !typedTarget.Ref.IsValid())
            {
                if (GUILayout.Button("Create", GUILayout.Width(100)))
                {
                }
            }
            else
            {
                if (GUILayout.Button("Edit", GUILayout.Width(100)))
                {
                }
            }
            
            region.End();
        }
    }
}
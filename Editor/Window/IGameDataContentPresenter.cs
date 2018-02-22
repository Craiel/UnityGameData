namespace Assets.Scripts.Craiel.GameData.Editor.Window
{
    using UnityEngine;

    public interface IGameDataContentPresenter
    {
        void Draw(Rect drawArea, GameDataEditorContent content);

        bool ProcessEvent(Event e);
    }
}

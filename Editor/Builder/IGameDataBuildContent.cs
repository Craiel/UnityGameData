﻿namespace Assets.Scripts.Craiel.GameData.Editor.Builder
{
    public interface IGameDataBuildContent
    {
        void Build(GameDataBuildContext context);

        void Validate(GameDataBuildValidationContext context);

        void Upgrade(GameDataBuildContext context);
    }
}
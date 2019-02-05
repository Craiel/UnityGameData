namespace Craiel.UnityGameData.Editor.Common
{
    using System;
    using UnityEngine;

    // ---------------------------------------------------------------------------------------------
    // Generic Class, only use for derived types
    // ---------------------------------------------------------------------------------------------
    internal static class GameResourceRefStatic
    {
    }

    [Serializable]
    public abstract class GameResourceRef<T> : GameResourceRefBase
        where T : UnityEngine.Object
    {
    }

    [Serializable]
    public class GameResourceCustomRef : GameResourceRef<GameObject>
    {
    }

    [Serializable]
    public class GameResourceGameObjectRef : GameResourceRef<GameObject>
    {
    }

    [Serializable]
    public class GameResourcePrefabRef : GameResourceRef<GameObject>
    {
    }

    [Serializable]
    public class GameResourceSpriteRef : GameResourceRef<Sprite>
    {
    }

    [Serializable]
    public class GameResourceAudioClipRef : GameResourceRef<AudioClip>
    {
    }

    [Serializable]
    public class GameResourceAnimationClipRef : GameResourceRef<AnimationClip>
    {
    }
}
﻿namespace Craiel.UnityGameData.Runtime
{
    using System;

    [Serializable]
    public abstract class GameDataRuntimeRef : GameDataRuntimeRefBase
    {
    }
    
    [Serializable]
    public class GameDataRuntimeAudioRef : GameDataRuntimeRef
    {
    }

    [Serializable]
    public class GameDataRuntimeEncounterRef : GameDataRuntimeRef
    {
    }

    [Serializable]
    public class GameDataRuntimeArenaRef : GameDataRuntimeRef
    {
    }
}
namespace Craiel.UnityGameData.Runtime.Attributes
{
    using System;
    using UnityEngine;

    [AttributeUsage(AttributeTargets.Field)]
    public class ReadOnlyAttribute : PropertyAttribute
    {
    }
}
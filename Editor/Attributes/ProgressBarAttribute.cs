namespace Assets.Scripts.Craiel.GameData.Editor.Attributes
{
    using System;
    using UnityEngine;

    [AttributeUsage(AttributeTargets.Field)]
    public class ProgressBarAttribute : PropertyAttribute
    {
        public readonly float Min;
        public readonly float Max;

        /// <summary>
        ///   <para>Attribute used to make a float or int variable in a script be restricted to a specific range.</para>
        /// </summary>
        /// <param name="min">The minimum allowed value.</param>
        /// <param name="max">The maximum allowed value.</param>
        public ProgressBarAttribute(float min, float max)
        {
            this.Min = min;
            this.Max = max;
        }
    }
}
namespace Assets.Scripts.Craiel.GameData.Editor.Attributes
{
    using UnityEngine;

    public class AngleAttribute : PropertyAttribute
    {
        // -------------------------------------------------------------------
        // Constructor
        // -------------------------------------------------------------------
        public AngleAttribute()
            : this(Color.gray, Color.red)
        {
        }
        
        public AngleAttribute(Color backgroundColor, Color activeColor, float min = -1, float max = -1, string unit = "", bool showValue = true, float knobSize = 32)
        {
            this.Min = min;
            this.Max = max;
            this.Unit = unit;
            this.ShowValue = showValue;
            this.KnobSize = knobSize;

            this.BackgroundColor = backgroundColor;
            this.ActiveColor = activeColor;
        }

        // -------------------------------------------------------------------
        // Public
        // -------------------------------------------------------------------
        public float Min { get; private set; }
        public float Max { get; private set; }
        public string Unit { get; private set; }
        public Color BackgroundColor { get; private set; }
        public Color ActiveColor { get; private set; }
        public bool ShowValue { get; private set; }
        public float KnobSize { get; private set; }
    }    
}
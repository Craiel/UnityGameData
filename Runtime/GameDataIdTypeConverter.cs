namespace Craiel.UnityGameData.Runtime
{
    using System;
    using System.ComponentModel;
    using System.Globalization;
    using UnityEssentials.Runtime;

    public class GameDataIdTypeConverter : TypeConverter
    {
        private const char Separator = ':';

        // -------------------------------------------------------------------
        // Public
        // -------------------------------------------------------------------
        public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
        {
            if (sourceType == TypeCache<string>.Value)
            {
                return true;
            }

            return base.CanConvertFrom(context, sourceType);
        }
        // Overrides the ConvertFrom method of TypeConverter.
        public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
        {
            var typed = value as string;
            if (typed != null)
            {
                string[] values = typed.Split(Separator);
                if (values.Length == 2)
                {
                    return new GameDataId(values[0], uint.Parse(values[1]));
                }
            }

            return base.ConvertFrom(context, culture, value);
        }
        // Overrides the ConvertTo method of TypeConverter.
        public override object ConvertTo(ITypeDescriptorContext context,
            CultureInfo culture, object value, Type destinationType)
        {
            if (destinationType == TypeCache<string>.Value)
            {
                return string.Format("{0}" + Separator + "{1}", ((GameDataId)value).Guid, ((GameDataId)value).Id);
            }
            return base.ConvertTo(context, culture, value, destinationType);
        }
    }
}

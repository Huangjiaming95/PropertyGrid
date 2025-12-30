using System;
using System.ComponentModel;

namespace EnhancedPropertyGrid.Core
{
    internal static class Converters
    {
        public static object ChangeType(object value, Type targetType, TypeConverter explicitConverter = null)
        {
            if (targetType == null) throw new ArgumentNullException(nameof(targetType));

            if (value == null || value == DBNull.Value)
            {
                if (!targetType.IsValueType || Nullable.GetUnderlyingType(targetType) != null)
                    return null;
                return Activator.CreateInstance(targetType);
            }

            var underlying = Nullable.GetUnderlyingType(targetType) ?? targetType;

            if (underlying.IsInstanceOfType(value))
                return value;

            var converter = explicitConverter ?? TypeDescriptor.GetConverter(underlying);

            if (converter != null)
            {
                if (converter.CanConvertFrom(value.GetType()))
                {
                    return converter.ConvertFrom(value);
                }
                if (value is string s)
                {
                    return converter.ConvertFromInvariantString(s);
                }
            }

            return System.Convert.ChangeType(value, underlying);
        }
    }
}


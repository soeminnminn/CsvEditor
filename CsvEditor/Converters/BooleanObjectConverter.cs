using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace CsvEditor.Converters
{
    public class BooleanObjectConverter : IValueConverter
    {
        #region Properties
        public object Given { get; set; }

        public bool IsInvert { get; set; }
        #endregion

        #region IValueConverter
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var val = IsTrue(value, Given);
            if (parameter != null)
                val = IsTrue(value, parameter);

            var result = IsInvert ? !val : val;
            if (targetType == typeof(Visibility))
            {
                var notVisibleValue = VisibilityOf(Given);
                return result ? Visibility.Visible : notVisibleValue;
            }
            else if (Given != null)
            {
                if (IsNumericType(targetType))
                    return result ? NumberOf(Given, targetType) : 0;

                else if (targetType.IsEnum)
                    return result ? EnumOf(Given, targetType) : null;
            }

            return result;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value != null && value is Visibility visibility)
                return Convert(visibility == Visibility.Visible, targetType, parameter, culture);

            return Convert(value, targetType, parameter, culture);
        }
        #endregion

        #region Methods
        private static bool IsTrue(object value, object compareValue)
        {
            if (value == null) return false;
            if (compareValue != null)
            {
                if (compareValue.GetType() == value.GetType())
                    return compareValue == value;

                if (compareValue is string str)
                    return str == $"{value}";
            }

            if (value is bool b)
                return b == true;

            if (value is string strVal)
                return string.Equals(strVal, "true", StringComparison.InvariantCultureIgnoreCase);

            if (decimal.TryParse($"{value}", out decimal val))
                return val > 0;

            return true;
        }

        private static Visibility VisibilityOf(object value, Visibility fallback = Visibility.Collapsed)
        {
            if (value is Visibility visibility) return visibility;
            if (value is string str && Enum.TryParse(str, out Visibility o)) return o;
            return fallback;
        }

        private static object EnumOf(object value, Type targetType)
        {
            if (value == null) return null;

            if (value.GetType() == targetType)
                return System.Convert.ChangeType(value, targetType);

#if NET
            if (Enum.TryParse(targetType, $"{value}", out object val))
                return val;
#else
            try
            {
                var val = Enum.Parse(targetType, $"{value}");
                if (val != null) return val;
            }
            catch
            { }
#endif
            return null;
        }

        private static bool IsNumericType(Type type)
        {
            switch (Type.GetTypeCode(type))
            {
                case TypeCode.Byte:
                case TypeCode.SByte:
                case TypeCode.UInt16:
                case TypeCode.UInt32:
                case TypeCode.UInt64:
                case TypeCode.Int16:
                case TypeCode.Int32:
                case TypeCode.Int64:
                case TypeCode.Decimal:
                case TypeCode.Double:
                case TypeCode.Single:
                    return true;
                case TypeCode.Object:
                    if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>))
                        return IsNumericType(Nullable.GetUnderlyingType(type));
                    return false;
                default:
                    return false;
            }
        }

        private static object NumberOf(object value, Type targetType)
        {
            if (value == null) return 0;

            var valStr = $"{value}";

            var type = targetType;
            if (targetType.IsGenericType && targetType.GetGenericTypeDefinition() == typeof(Nullable<>))
                type = Nullable.GetUnderlyingType(targetType);

            switch (Type.GetTypeCode(type))
            {
                case TypeCode.Byte:
                    if (byte.TryParse(valStr, out byte b)) return b;
                    break;
                case TypeCode.SByte:
                    if (sbyte.TryParse(valStr, out sbyte sb)) return sb;
                    break;
                case TypeCode.UInt16:
                    if (ushort.TryParse(valStr, out ushort us)) return us;
                    break;
                case TypeCode.UInt32:
                    if (uint.TryParse(valStr, out uint ui)) return ui;
                    break;
                case TypeCode.UInt64:
                    if (ulong.TryParse(valStr, out ulong ul)) return ul;
                    break;
                case TypeCode.Int16:
                    if (short.TryParse(valStr, out short s)) return s;
                    break;
                case TypeCode.Int32:
                    if (int.TryParse(valStr, out int i)) return i;
                    break;
                case TypeCode.Int64:
                    if (long.TryParse(valStr, out long l)) return l;
                    break;
                case TypeCode.Decimal:
                    if (decimal.TryParse(valStr, out decimal d)) return d;
                    break;
                case TypeCode.Single:
                    if (float.TryParse(valStr, out float f)) return f;
                    break;
                case TypeCode.Double:
                    if (double.TryParse(valStr, out double dbl)) return dbl;
                    break;
                default:
                    break;
            }

            return 0;
        }
        #endregion
    }
}

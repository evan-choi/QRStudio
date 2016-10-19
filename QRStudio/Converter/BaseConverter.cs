using System;
using System.Windows.Data;
using System.Globalization;

namespace QRStudio.Converter
{
    public abstract class BaseConverter<ValueType, TargetType> : MarkupSupport, IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return OnConvert(Convert<ValueType>(value), parameter);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return OnConvertBack(Convert<TargetType>(value), parameter);
        }

        protected virtual TargetType OnConvert(ValueType value, object parameter)
        {
            return default(TargetType);
        }

        protected virtual ValueType OnConvertBack(TargetType value, object parameter)
        {
            return default(ValueType);
        }
        
        private OutT Convert<OutT>(object value)
        {
            if (typeof(OutT) == typeof(object))
            {
                return (OutT)value;
            }
            else
            {
                return (OutT)System.Convert.ChangeType(value, typeof(OutT));
            }
        }
    }

    public class BaseConverter<ValueType, TargetType, ParamType> : BaseConverter<ValueType, TargetType>
    {
        protected override TargetType OnConvert(ValueType value, object parameter)
        {
            ParamType v = default(ParamType);
            if (parameter != null) v = (ParamType)System.Convert.ChangeType(parameter, typeof(ParamType));

            return OnConvert(value, ParseParameter(parameter));
        }

        protected virtual TargetType OnConvert(ValueType value, ParamType parameter)
        {
            return default(TargetType);
        }

        protected override ValueType OnConvertBack(TargetType value, object parameter)
        {
            return OnConvertBack(value, ParseParameter(parameter));
        }

        protected virtual ValueType OnConvertBack(TargetType value, ParamType parameter)
        {
            return default(ValueType);
        }

        private ParamType ParseParameter(object parameter)
        {
            ParamType v = default(ParamType);
            if (parameter != null) v = (ParamType)System.Convert.ChangeType(parameter, typeof(ParamType));

            return v;
        }
    }
}

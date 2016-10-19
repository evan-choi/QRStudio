using System;
using System.Linq;
using System.Windows.Data;
using System.Globalization;

namespace QRStudio.Converter
{
    public abstract class BaseMultiConverter<ValueType, TargetType> : MarkupSupport, IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            return OnConvert(ConvertArray<ValueType, object>(values), parameter);
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            return ConvertArray<object, ValueType>(OnConvertBack((TargetType)value, parameter));
        }

        protected virtual TargetType OnConvert(ValueType[] values, object parameter)
        {
            return default(TargetType);
        }

        protected virtual ValueType[] OnConvertBack(TargetType value, object parameter)
        {
            return default(ValueType[]);
        }

        private OutT[] ConvertArray<OutT, InT>(InT[] arr)
        {
            return arr.ToList().Select((v) => (OutT)System.Convert.ChangeType(v, typeof(OutT))).ToArray();
        }
    }
}

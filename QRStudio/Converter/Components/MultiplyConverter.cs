using System;
using System.Globalization;
using System.Linq;
using System.Windows.Data;

namespace QRStudio.Converter
{
    class MultiplyConverter : BaseMultiConverter<double, object>
    {
        public IMultiValueConverter OutputConverter { get; set; } = null;

        protected override object OnConvert(double[] values, object parameter)
        {
            double s = values.Aggregate(1.0, (x, y) => x * y);

            if (OutputConverter != null)
            {
                return OutputConverter.Convert(new object[] { s }, typeof(object), parameter, CultureInfo.CurrentCulture);
            }

            return s;
        }
    }
}

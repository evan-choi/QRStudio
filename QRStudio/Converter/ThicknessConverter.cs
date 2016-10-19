using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace QRStudio.Converter
{
    class ThicknessConverter : BaseMultiConverter<double, Thickness>
    {
        protected override Thickness OnConvert(double[] values, object parameter)
        {
            switch (values.Length)
            {
                case 1:
                    return new Thickness(values[0]);

                case 2:
                    return new Thickness(values[0], values[1], values[0], values[1]);

                case 4:
                    return new Thickness(values[0], values[1], values[2], values[3]);
            }

            return default(Thickness);
        }
    }
}

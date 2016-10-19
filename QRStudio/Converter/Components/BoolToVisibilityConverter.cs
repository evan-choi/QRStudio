using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace QRStudio.Converter
{
    public class BoolToVisibilityConverter : BaseConverter<bool, Visibility>
    {
        public bool Inverse { get; set; } = false;

        protected override Visibility OnConvert(bool value, object parameter)
        {
            return value ^ Inverse ? Visibility.Visible : Visibility.Collapsed;
        }
    }
}

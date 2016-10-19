using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace QRStudio.Converter
{
    public class NotConverter : BaseConverter<bool, bool>
    {
        protected override bool OnConvert(bool value, object parameter)
        {
            return !value;
        }
    }
}

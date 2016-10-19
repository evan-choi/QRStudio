using System;
using System.Windows.Markup;

namespace QRStudio.Converter
{
    public class MarkupSupport : MarkupExtension
    {
        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            return this;
        }
    }
}

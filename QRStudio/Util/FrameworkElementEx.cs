using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace QRStudio.Util
{
    public static class FrameworkElementEx
    {
        public static void SetSize(this FrameworkElement ui, double width, double height)
        {
            ui.Width = width;
            ui.Height = height;
        }

        public static void SetSize(this FrameworkElement ui, FrameworkElement source)
        {
            ui.Width = source.Width;
            ui.Height = source.Height;
        }
    }
}

using System;
using System.Windows.Media;
using System.Windows;

namespace QRStudio.Control
{
    public class ColorChangedEventArgs : RoutedEventArgs
    {
        public ColorChangedEventArgs(RoutedEvent routedEvent, Color color)
        {
            this.RoutedEvent = routedEvent;
            this.Color = color;
        }

        private Color _Color;
        public Color Color
        {
            get { return _Color; }
            set { _Color = value; }
        }
    }
}

using System;
using System.Windows.Media;
using System.Windows;

namespace QRStudio.Control
{
    public class BrushChangedEventArgs : RoutedEventArgs
    {
        public BrushChangedEventArgs(RoutedEvent routedEvent, Brush brush)
        {
            this.RoutedEvent = routedEvent;
            this.Brush = brush;
        }

        private Brush mBrush;
        public Brush Brush
        {
            get { return mBrush; }
            set { mBrush = value; }
        }
    }
}

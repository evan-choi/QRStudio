using System;
using System.Windows.Media;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace QRStudio.Control
{
    abstract class BaseSelector : FrameworkElement
    {
        public Orientation Orientation
        {
            get { return (Orientation)GetValue(OrientationProperty); }
            set { SetValue(OrientationProperty, value); }
        }
        public static readonly DependencyProperty OrientationProperty =
            DependencyProperty.Register("Orientation", typeof(Orientation), typeof(BaseSelector), new UIPropertyMetadata(Orientation.Vertical));
        
        public Color Color
        {
            get { return (Color)GetValue(ColorProperty); }
            protected set { SetValue(ColorProperty, value); }
        }
        public static readonly DependencyProperty ColorProperty =
            DependencyProperty.Register("Color", typeof(Color), typeof(BaseSelector), new UIPropertyMetadata(Colors.Red));       
    }
}

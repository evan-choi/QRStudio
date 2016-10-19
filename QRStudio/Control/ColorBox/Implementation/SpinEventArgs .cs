using System.Windows;

namespace QRStudio.Control
{
    class SpinEventArgs : RoutedEventArgs
    {
        public SpinDirection Direction
        {
            get;
            private set;
        }
      
        public SpinEventArgs(SpinDirection direction)
            : base()
        {
            Direction = direction;
        }
    }
}



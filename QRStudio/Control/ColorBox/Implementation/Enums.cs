using System;

namespace QRStudio.Control
{    
    internal enum SpinDirection
    {
        Increase,
        Decrease
    }


    internal enum ValidSpinDirections
    {
        None,
        Increase,
        Decrease
    }


    internal enum AllowedSpecialValues
    {
        None = 0,
        NaN = 1,
        PositiveInfinity = 2,
        NegativeInfinity = 4,
        AnyInfinity = PositiveInfinity | NegativeInfinity,
        Any = NaN | AnyInfinity
    }
}

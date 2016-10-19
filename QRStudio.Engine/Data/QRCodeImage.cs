using System;

namespace QRStudio.Engine.Codec.Data
{
    public interface QRCodeImage
    {
        int Width { get; }
        int Height { get; }
        int GetPixel(int x, int y);
    }
}
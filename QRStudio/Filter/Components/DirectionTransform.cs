using System;
using QRStudio.Engine.Geom;

namespace QRStudio.Filter
{
    class DirectionFilter : MatrixFilter
    {
        public enum Direction : byte
        {
            Left   = 4,
            Top    = 8,
            Right  = 16,
            Bottom = 32
        }

        public Point[] Preset { get; set; } =
        {
            new Point(1, 0),   // Right
            new Point(0, 1),   // Bottom
            new Point(-1, 0),  // Left
            new Point(0, -1)   // Top
        };

        public DirectionFilter()
        {
        }

        public override void Translate(ref byte[][] matrix)
        {
            byte[][] temp = matrix;

            int width = matrix.Length;
            int height = matrix[0].Length;

            Func<int, int, byte> getLevel =
                (x, y) =>
                {
                    if ((x < width && x >= 0) && (y < height && y >= 0))
                        return temp[x][y];

                    return 0;
                };

            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    int cLvl = matrix[x][y] >> 2;

                    if (matrix[x][y] == 2)
                    {
                        for (byte i = 0; i < Preset.Length; i++)
                        {
                            byte lvl = getLevel(x + Preset[i].X, y + Preset[i].Y);

                            if (lvl > 0)
                                matrix[x][y] = (byte)(Math.Pow(2, i + 2));
                        }
                    }
                    else if (cLvl % 3 == 0 && cLvl > 2 && cLvl < 15)
                    {
                        switch (cLvl)
                        {
                            case 3:
                                matrix[x][y] = (byte)(Direction.Right | Direction.Bottom);
                                break;

                            case 6:
                                matrix[x][y] = (byte)(Direction.Left | Direction.Bottom);
                                break;

                            case 9:
                                matrix[x][y] = (byte)(Direction.Right | Direction.Top);
                                break;

                            case 12:
                                matrix[x][y] = (byte)(Direction.Left | Direction.Top);
                                break;
                        }
                    }
                }
            }
        }
    }
}
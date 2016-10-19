using System;
using QRStudio.Engine.Geom;

namespace QRStudio.Filter
{
    class EdgeFilter : MatrixFilter
    {
        public Point[] Preset { get; set; } =
        {
            new Point(-1, 0),
            new Point(0, -1),
            new Point(1, 0),
            new Point(0, 1)
        };

        public int Level { get; set; } = 1;   
        
        public EdgeFilter(byte[][] matrix) : base(matrix)
        {
        }

        public EdgeFilter()
        {

        }

        public override void Translate(ref byte[][] matrix)
        {
            byte[][] temp = matrix;

            int width = matrix.Length;
            int height = matrix[0].Length;

            Func<int, int, int> getValue =
                (x, y) =>
                {
                    if ((x < width && x >= 0) && (y < height && y >= 0))
                        return Math.Min((int)temp[x][y], 1);

                    return 0;
                };

            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    int eLvl = 0;
                    int cLvl = 0;

                    if (matrix[x][y] == 1)
                    {
                        for (int i = 0; i <Preset.Length; i++)
                        {
                            int v = getValue(x + Preset[i].X, y + Preset[i].Y);

                            eLvl += v;
                            cLvl += (v > 0 ? (int)Math.Pow(2, i) : 0);
                        }

                        if (eLvl == Level)
                            matrix[x][y] = 2;

                        if (cLvl % 3 == 0 && cLvl < 15 && cLvl > 0)
                        {
                            matrix[x][y] = (byte)(cLvl << 2);
                        }
                    }
                }
            }
        }
    }
}
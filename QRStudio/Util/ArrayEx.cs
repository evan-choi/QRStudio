using System;
using System.Linq;

namespace QRStudio.Util
{
    public static class ArrayEx
    {
        public static T[][] Copy2D<T>(this T[][] source)
        {
            T[][] dest = new T[source.Length][];

            for (int x = 0; x < dest.Length; x++)
            {
                dest[x] = new T[source[x].Length];

                Array.Copy(source[x], dest[x], source[x].Length);
            }

            return dest;
        }

        public static T[][] Cast2D<T, ArrayT>(this ArrayT[][] source, Func<ArrayT, T> cast = null)
        {
            T[][] dest = new T[source.Length][];

            for (int x = 0; x < dest.Length; x++)
            {
                dest[x] = new T[source[x].Length];

                if (cast != null)
                {
                    for (int y = 0; y < source[x].Length; y++)
                    {
                        dest[x][y] = cast(source[x][y]);
                    }
                }
                else
                {
                    Array.Copy(source[x].Cast<T>().ToArray(), dest, dest.Length);
                }
            }

            return dest;
        }
    }
}

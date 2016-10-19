using QRStudio.Util;

using System;
using System.Collections.ObjectModel;

namespace QRStudio.Filter
{
    public class MatrixFilter : IFilter
    {
        public byte[][] RawMatrix { get; set; } = null;

        public Collection<IFilter> Filters { get; } = new Collection<IFilter>();
        
        public MatrixFilter(byte[][] matrix)
        {
            this.RawMatrix = matrix;
        }

        public MatrixFilter()
        {
        }

        public virtual byte[][] Translate()
        {
            if (RawMatrix == null)
                throw new Exception("계산 전용 변환 클래스입니다.");

            byte[][] temp = RawMatrix.Copy2D();

            this.Translate(ref temp);

            foreach (var t in Filters)
            {
                t.Translate(ref temp);
            }

            return temp;
        }

        public virtual void Translate(ref byte[][] array)
        {
        }
    }
}

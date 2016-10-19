using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QRStudio.Filter
{
    public interface IFilter
    {
        byte[][] Translate();
        void Translate(ref byte[][] array);
    }
}
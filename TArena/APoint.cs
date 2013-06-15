using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TArena
{
    public class APoint
    {
        public int X { get; set; }
        public int Y { get; set; }

        public APoint(int x, int y)
        {
            X = x;
            Y = y;
        }

        public APoint()
            : this(int.MinValue, int.MinValue)
        {

        }
    }
}

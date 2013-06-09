using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TMapper.Structures
{
    public class MapStaticInfo
    {
        public int Rows { get; set; }
        public int Cols { get; set; }
        public String WorldName { get; set; }
        public DateTime Generated { get; set; }

        public MapStaticInfo()
        {
            Rows = 0;
            Cols = 0;
            WorldName = "World";
            Generated = DateTime.Now;
        }

        public override String ToString()
        {
            return String.Format("{0};{1};{2};{3}", Rows, Cols, WorldName, Generated.ToString());
        }
    }
}

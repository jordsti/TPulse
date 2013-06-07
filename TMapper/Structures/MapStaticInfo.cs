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

        public String GetJavascript()
        {
            return String.Format("//Automatically generated file\n var MapsRows = {0}; var MapsCols = {1}; var WorldName = '{2}'; var GeneratedOn = '{3}'\n", Rows, Cols, WorldName.Replace("'","\\'"), Generated.ToString());
        }
    }
}

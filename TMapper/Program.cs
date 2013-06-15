using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading;
using TMapper.Structures;

namespace TMapper
{

    //Main flow example, at the moment, need to be placed into TPulse with a setting
    public class Program
    {
        public static bool mapperRunning = false;

        public static void Main(string[] args)
        {
            String error = Global.Instance.Initialize();

            if (args.Length != 2)
            {
                Console.WriteLine("TMapper");
                Console.WriteLine("You must specify a world and a image filename output");
                Console.WriteLine("TMapper.exe [worldpath] [imageout]");
            }
            else
            {


                string mappath = args[0];


                TileProperties.Initialize();
                ResourceManager.Instance.Initialize();
                SettingsManager.Instance.Initialize();

                string file = mappath;
                Console.WriteLine(file);
                Global.Instance.InConsole = true;
                WorldMapper mapper = new WorldMapper();
                mapper.Initialize();
                mapper.OpenWorld();

                mapper.ProcessWorld(file, null);
                mapper.CreatePreviewPNG(args[1], null);
            }

        }

    }
}

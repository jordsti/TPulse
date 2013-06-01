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
    public class Program
    {
        public static bool mapperRunning = false;

        public static void Main(string[] args)
        {
            String error = Global.Instance.Initialize();
            string mappath = @"C:\Users\JordSti\Documents\My Games\Terraria\Worlds";

            string[] files = Directory.GetFiles(mappath, "*.wld");

            Console.WriteLine("TMapper");
            Console.WriteLine("World(s): ");
            int i = 1;
            foreach (string f in files)
            {
                Console.WriteLine(String.Format("{0} : {1}", i, Path.GetFileNameWithoutExtension(f)));
                i++;
            }

            Console.Write("World id :");

            string data = Console.ReadLine();

            int wid = 0;

            int.TryParse(data, out wid);

            if (wid > files.Length || wid <= 0)
            {
                Console.WriteLine("Invalid world id");
            }
            else
            {
                TileProperties.Initialize();
                ResourceManager.Instance.Initialize();
                SettingsManager.Instance.Initialize();

                string file = files[wid-1];
                Console.WriteLine(file);
                Global.Instance.InConsole = true;
                WorldMapper mapper = new WorldMapper();
                mapper.Initialize();
                mapper.OpenWorld();

                mapper.ProcessWorld(file, null);
                mapper.CreatePreviewPNG("test.png", null);
                
            }

            Console.Read();
        }

    }
}

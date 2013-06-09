using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Terraria;
using TMapper;
using TMapper.Structures;
using System.Threading;

namespace TPulseAPI
{
    public class MapImageGenerator
    {

        public const String MapFileName = "maps.png";
        public const String CutsFolder = "maps";
        public const String MapInfoFile = "maps.txt";
        public const String StampFile = "stamp.txt";

        public WorldInfo World { get; protected set; }
        public String OutputFolder { get; set; }

        private static Thread thread = null;
        private static bool Initialized = false;

        public MapImageGenerator(string outputFolder)
        {
            Console.WriteLine("MapImageGenerator Debug: " + outputFolder);
            OutputFolder = outputFolder;

            World = new WorldInfo();
            Console.WriteLine(World.WorldPath);
            if (!File.Exists(World.WorldPath))
            {
                throw new FileNotFoundException(World.WorldPath);
            }

            if (!Initialized)
            {
                Initialized = true;
                Global.Instance.Initialize();
                TileProperties.Initialize();
                ResourceManager.Instance.Initialize();
                SettingsManager.Instance.Initialize();
                Global.Instance.InConsole = true;
            }
        }

        public void ThreadGenerate()
        {
            if (thread == null)
            {
                ThreadStart ts = new ThreadStart(Generate);
                thread = new Thread(ts);
                thread.Start();
            }
            else
            {
                Console.WriteLine("MapGenerator thread already running...");
            }
        }

        public void Generate()
        {
            try
            {
                Console.WriteLine("Generating the map image...");

				Console.WriteLine("World Path: "+World.WorldPath);

                WorldMapper mapper = new WorldMapper();
                mapper.Initialize();
                mapper.OpenWorld();

                if (!Directory.Exists(OutputFolder))
                {
                    Directory.CreateDirectory(OutputFolder);
                }

                string path = Path.Combine(OutputFolder, MapFileName);

                mapper.ProcessWorld(World.WorldPath, null);
                mapper.CreatePreviewPNG(path, null);
                ImageCutter cutter = new ImageCutter(path, Path.Combine(OutputFolder, CutsFolder));

                cutter.Cuts();
                //Generating the map info

                MapStaticInfo msi = new MapStaticInfo();

                msi.Cols = cutter.Cols;
                msi.Rows = cutter.Rows;
                msi.WorldName = World.WorldName;

                File.WriteAllText(Path.Combine(OutputFolder, CutsFolder, MapInfoFile), msi.ToString());
                File.WriteAllText(Path.Combine(OutputFolder, CutsFolder, StampFile), msi.Generated.ToString());
                Console.WriteLine("Map image generated!");
                //Release thread ref
                thread = null;
            }
            catch (Exception e)
            {
                Console.WriteLine("Error while generating the image map");
                Console.WriteLine(e.StackTrace);
                Console.WriteLine(e.Message);
                //thread.Abort();
                thread = null;
            }
        }

    }
}

﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Terraria;
using TMapper;
using System.IO;
namespace TPulseAPI
{
    public class WorldInfo
    {
        public String WorldPath { get; protected set; }
        public int WorldIndex { get; protected set; }
        public String WorldName { get; protected set; }


        public WorldInfo ()
		{

			WorldPath = Main.worldPathName;

			if (!Main.runningMono) {
				WorldPath = Path.Combine(Main.WorldPath, WorldPath);
			}

            Console.WriteLine("WorldInfo Debug: " + WorldPath);
            WorldIndex = Main.worldID;
            WorldName = Main.worldName;
        }
    }
}

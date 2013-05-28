using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TPulseAPI.Events
{
    public delegate void WorldSavedHandler(WorldSavedEventArgs args);

    public class WorldSavedEventArgs
    {
        public DateTime Time { get; protected set; }
        public String WorldPath { get; protected set; }

        public WorldSavedEventArgs(DateTime time, string worldPath)
        {
            Time = time;
            WorldPath = worldPath;
        }

    }
}

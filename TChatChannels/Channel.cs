using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TPulseAPI;
using Terraria;

namespace TChatChannels
{
    public class Channel
    {
        public String Name { get; protected set; }

        protected List<TPPlayer> Players = new List<TPPlayer>();

        public Channel(string name)
        {
            Name = name;

        }

    }
}

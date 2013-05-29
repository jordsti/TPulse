using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Terraria;
using TPulseAPI;

namespace TPluginSample
{
    [APIVersion(1, 12)]
    public class TPluginSample : TerrariaPlugin
    {
        public TPluginSample(Main game)
            : base(game)
        {
            PlugInHandler.AddPlugIn(this);
        }

        public override Version Version
        {
            get
            {
                return new Version("0.0");
            }
        }

        public override string Name
        {
            get
            {
                return "TPluginSample";
            }
        }

        public override string Author
        {
            get
            {
                return "Author Here";
            }
        }

        public override string Description
        {
            get
            {
                return "Short Description";
            }
        }

        public override void Initialize()
        {
        }
    }
}

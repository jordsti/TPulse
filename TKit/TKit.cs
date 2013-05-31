using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Terraria;
using TPulseAPI;

namespace TKit
{

    namespace TKit
    {
        [APIVersion(1, 12)]
        public class TKit : TerrariaPlugin
        {
            private TPulse tPulse;

            public TKit(Main game)
                : base(game)
            {
                PlugInHandler.AddPlugIn(this);
 
            }

            public override Version Version
            {
                get
                {
                    return new Version("0.1");
                }
            }

            public override string Name
            {
                get
                {
                    return "TKit";
                }
            }

            public override string Author
            {
                get
                {
                    return "JordSti";
                }
            }

            public override string Description
            {
                get
                {
                    return "Give item kit to players";
                }
            }

            public override void Initialize()
            {
                tPulse = (TPulse)PlugInHandler.GetPluginByType(typeof(TPulse));
                //chat commands to add

            }
        }
    }

}

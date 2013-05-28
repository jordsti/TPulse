using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Terraria;
using TPulseAPI;

namespace TChatChannels
{
    [APIVersion(1, 12)]
    public class TChatChannels : TerrariaPlugin
    {
        public TChatChannels(Main game)
            : base(game)
        {

        }

                //Plug-In Info
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
                return "TChatChannels";
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
                return "The user can join and chat to a channel";
            }
        }

        //Initialization

        public override void Initialize()
        {
            //Commands
            //  /join : to join a channel
            //  /channel : super-command
            //      add : add channel (need group < admin)
            //      del : delete a channel (need group < admin)
            //      list : listing

            
        }
    }
}

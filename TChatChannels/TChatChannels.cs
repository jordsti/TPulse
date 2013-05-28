using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using Terraria;
using TPulseAPI;

namespace TChatChannels
{
    [APIVersion(1, 12)]
    public class TChatChannels : TerrariaPlugin
    {
        public Channel DefaultChannel { get; set; }
        public bool AutoJoin { get; set; }
        
        protected ChannelManager Manager;

        public TChatChannels(Main game)
            : base(game)
        {
            AutoJoin = true;
            PlugInHandler.AddPlugIn(this);
            Manager = new ChannelManager();
            DefaultChannel = Manager.DefaultChannel;
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


        public override void Initialize()
        {
            //Commands
            //  /join : to join a channel
            //  /channel : super-command
            //      add : add channel (need group < admin)
            //      del : delete a channel (need group < admin)
            //      list : listing
            //can be null be aware !

            Commands.ChatCommands.Add(new Command("", JoinCommand, "/join"));
            Commands.ChatCommands.Add(new Command("", LeaveCommand, "/leave"));
            Commands.ChatCommands.Add(new Command("", ChannelCommand, "/channel"));

            List<Channel> channels = Manager.ChatChannels;

            foreach (Channel c in channels)
            {
                //TEST
                Console.WriteLine("Creating channel: " + c.Name);
            }

            if (DefaultChannel != null)
            {
                Console.WriteLine("Default channel is " + DefaultChannel.Name);
            }

            Hooks.WorldHooks.SaveWorld += new Hooks.WorldHooks.SaveWorldD(OnSaveWorld);

        }

        protected void OnSaveWorld(bool resettime, HandledEventArgs args)
        {
            //Saving channels...
            //Some verbose info needed here
            Manager.Save();
        }

        protected void ChannelCommand(CommandArgs args)
        {
            TPPlayer player = args.Player;

            if (args.Parameters.Count > 1)
            {

            }
            else if (args.Parameters.Count == 1 && args.Parameters[0] == "list")
            {
                //listing channels
            }
            else
            {
                player.SendErrorMessage("Channels: Invalid command");
            }
        }

        protected void LeaveCommand(CommandArgs args)
        {

        }

        protected void JoinCommand(CommandArgs args)
        {

        }

        void tpulse_OnPlayerJoin(TPulseAPI.Events.PlayerConnectionEventArgs args)
        {
            Console.WriteLine("Chats!!"+args.Player.Name);
        }
    }
}

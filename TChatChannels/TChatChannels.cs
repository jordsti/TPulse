using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using Terraria;
using TPulseAPI;
using TPulseAPI.Events;

namespace TChatChannels
{
    [APIVersion(1, 12)]
    public class TChatChannels : TerrariaPlugin
    {
        public Channel DefaultChannel { get; set; }
        public bool AutoJoin { get; set; }
        
        protected ChannelManager Manager;

        protected List<Channel> Channels = new List<Channel>();

        private TPulse tPulse;

        public TChatChannels(Main game)
            : base(game)
        {
            AutoJoin = true;
            PlugInHandler.AddPlugIn(this);
            Manager = new ChannelManager();
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
            //Get TPulse instance

            tPulse = (TPulse)PlugInHandler.GetPluginByType(typeof(TPulse));


            //Commands
            //  /join : to join a channel
            //  /channel : super-command
            //      add : add channel (need group < admin)
            //      del : delete a channel (need group < admin)
            //      list : listing
            //  /cmsg : send a message in the channel
            //can be null be aware !

            tPulse.Commands.ChatCommands.Add(new Command("", JoinCommand, "join"));
            tPulse.Commands.ChatCommands.Add(new Command("", LeaveCommand, "leave"));
            tPulse.Commands.ChatCommands.Add(new Command("", ChannelCommand, "channel"));
            tPulse.Commands.ChatCommands.Add(new Command("", ChannelMessage, "cmsg"));
            
            DefaultChannel = Manager.DefaultChannel;
            
            Channels.AddRange(Manager.ChatChannels);

            //Connection hooks

            tPulse.OnPlayerJoin += new PlayerConnectionHandler(PlayerJoinServer);
            tPulse.OnPlayerLeave += new PlayerConnectionHandler(PlayerLeaveServer);

            //World Saved Hooks

            tPulse.OnWorldSaved += new WorldSavedHandler(OnWorldSaved);
        }

        protected void PlayerLeaveServer(PlayerConnectionEventArgs args)
        {
            foreach (Channel c in Channels)
            {
                if (c.ContainsPlayer(args.Player))
                {
                    c.Leave(args.Player);
                    break;
                }
            }
        }

        protected void ChannelMessage(CommandArgs args)
        {
            TPPlayer player = args.Player;

            Channel pChannel = null;

            foreach (Channel c in Channels)
            {
                if (c.ContainsPlayer(player))
                {
                    pChannel = c;
                    break;
                }
            }

            if (pChannel == null)
            {
                player.SendErrorMessage("Channels: You're not in a channel!");
            }
            else
            {
                string message = "";

                for(int i=0; i<args.Parameters.Count; i++)
                {
                    message += args.Parameters[i] + " ";
                }

                pChannel.Send(player, message);
            }

        }

        protected void PlayerJoinServer(PlayerConnectionEventArgs args)
        {
            if (AutoJoin && DefaultChannel != null)
            {
                args.Player.SendInfoMessage(String.Format("Channels: You're entering {0} channel", DefaultChannel.Name));
                DefaultChannel.Join(args.Player);
            }
        }

        protected void OnWorldSaved(WorldSavedEventArgs args)
        {
            //Saving channels...
            //Some verbose info needed here
            Console.WriteLine("Channels saved!");
            Manager.Save();
        }

        protected Channel RemoveChannel(string cname)
        {
            Channel toRemove = null;

            foreach (Channel c in Channels)
            {
                if (c.Name == cname)
                {
                    toRemove = c;
                    break;
                }
            }

            if (toRemove != null)
            {
                Channels.Remove(toRemove);
            }

            return toRemove;
        }

        protected void ChannelCommand(CommandArgs args)
        {
            TPPlayer player = args.Player;

            if (args.Parameters.Count > 1)
            {
                if ((player.Group.ContainsGroup("admin") || player.Group.ContainsGroup("superadmin")) && player.IsLoggedIn)
                {
                    string pram = args.Parameters[0];

                    if (pram == "add")
                    {
                        //adding channel
                        string cname = args.Parameters[1];

                        if (Manager.Contains(cname))
                        {
                            player.SendErrorMessage(String.Format("Channels: {0} already exists!"));
                        }
                        else
                        {
                            Channel c;
                            if (args.Parameters.Count == 5)
                            {
                                //with custom color
                                byte r = 0;
                                byte g = 0;
                                byte b = 0;

                                byte.TryParse(args.Parameters[2], out r);
                                byte.TryParse(args.Parameters[3], out g);
                                byte.TryParse(args.Parameters[4], out b);
                                c = new Channel(cname, r, g, b);
                            }
                            else
                            {
                                c = new Channel(cname);
                            }

                            Manager.Add(c);
                            Channels.Add(c);

                            player.SendInfoMessage(String.Format("Channels: {0} channel added with success!", cname));
                        }
                    }
                    else if (pram == "del")
                    {
                        //deleting channel
                        string cname = args.Parameters[1];

                        Channel c = RemoveChannel(cname);

                        if (c != null)
                        {
                            Manager.Remove(cname);
                            player.SendInfoMessage(String.Format("Channels: {0} channel removed!", cname));
                        }
                        else
                        {
                            player.SendErrorMessage("Channels: This channel doesn't exists!");
                        }

                    }
                    else
                    {
                        player.SendErrorMessage("Channels: Invalid command!");
                    }
                }
                else
                {
                    player.SendErrorMessage("Channels: You don't have the right to do this!");
                }
            }
            else if (args.Parameters.Count == 1 && args.Parameters[0] == "list")
            {
                //listing channels
                player.SendInfoMessage("Channels: Listing channels");
                foreach (Channel c in Channels)
                {
                    player.SendInfoMessage(String.Format("{0} : {1}", c.Name, c.Count));

                }

                player.SendInfoMessage(String.Format("{0} channel(s)", Channels.Count));
            }
            else
            {
                player.SendErrorMessage("Channels: Invalid command");
            }
        }

        protected void LeaveCommand(CommandArgs args)
        {
            TPPlayer player = args.Player;

            foreach (Channel c in Channels)
            {
                if (c.ContainsPlayer(player))
                {
                    c.Leave(player);
                    player.SendInfoMessage("Channels: Channel left");
                    return;
                }
            }

            player.SendErrorMessage("Channels: You're not in any channel!");
        }

        protected void JoinCommand(CommandArgs args)
        {
            TPPlayer player = args.Player;

            if (args.Parameters.Count == 1)
            {
                string cname = args.Parameters[0];

                if (Manager.Contains(cname))
                {
                    foreach (Channel c in Channels)
                    {
                        if (c.ContainsPlayer(player))
                        {
                            c.Leave(player);
                        }
                    }

                    foreach (Channel c in Channels)
                    {
                        if (c.Name == cname)
                        {
                            c.Join(player);
                            player.SendInfoMessage(String.Format("Channels: You've entered {0} channel", cname));
                            break;
                        }
                    }

                    
                }
                else
                {
                    player.SendErrorMessage(String.Format("Channels: {0} doesn't exists!", cname));
                }

            }
            else
            {
                player.SendErrorMessage("Channels: Missing or too much arguments!");
            }
        }
    }
}

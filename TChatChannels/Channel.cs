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
        public Color TextColor { get; protected set; }

        public String Name { get; protected set; }

        protected List<TPPlayer> Players = new List<TPPlayer>();

        public bool IsLoginRequired { get; set; }

        public Channel(string name)
            : this(name, Color.White)
        {

        }


        public Channel(string name, byte red, byte green, byte blue)
            : this(name, new Color(red, green, blue))
        {

        }

        public Channel(string name, Color textColor)
        {
            IsLoginRequired = false;
            TextColor = textColor;
            Name = name;
        }

        protected void BroadcastMessage(string message)
        {
            foreach (TPPlayer player in Players)
            {
                player.SendMessage(message, TextColor);
            }
        }

        public void Send(TPPlayer from, string message)
        {
            if (Players.Contains(from))
            {
                String fmessage = String.Format("{0}: {1}", from.Name, message);

                foreach (TPPlayer player in Players)
                {
                    player.SendMessage(fmessage, TextColor);
                }
            }
        }

        public void Join(TPPlayer player)
        {
            if (!Players.Contains(player))
            {
                BroadcastMessage(String.Format("{0} has join", player.Name));
                Players.Add(player);
            }
        }

        public void Leave(TPPlayer player)
        {
            if(Players.Contains(player))
            {
                Players.Remove(player);
                BroadcastMessage(String.Format("{0} has left", player.Name));
            }
        }

        public override string ToString()
        {
            return String.Format("{0} r:{1} g:{2} b:{3}", Name, TextColor.R, TextColor.G, TextColor.B);
        }

    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Terraria;

namespace TPulseAPI
{
    public class CommandArgs
    {
        public string Message { get; private set; }
        public TPPlayer Player { get; private set; }


        /// <summary>
        /// Parameters passed to the arguement. Does not include the command name.
        /// IE '/kick "jerk face"' will only have 1 argument
        /// </summary>
        public List<string> Parameters { get; private set; }

        public Player TPlayer
        {
            get { return Player.TPlayer; }
        }

        public CommandArgs(string message, TPPlayer ply, List<string> args)
        {
            Message = message;
            Player = ply;
            Parameters = args;
        }
    }
}

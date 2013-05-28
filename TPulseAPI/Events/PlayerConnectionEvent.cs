using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TPulseAPI.Events
{
    public delegate void PlayerConnectionHandler(PlayerConnectionEventArgs args);

    public enum PlayerConnectionAction
    {
        Join,
        Leave
    }

    public class PlayerConnectionEventArgs
    {
        public TPPlayer Player { get; protected set; }
        public PlayerConnectionAction Action { get; protected set; }

        public PlayerConnectionEventArgs(TPPlayer player, PlayerConnectionAction action)
        {
            Player = player;
            Action = action;
        }

    }
}

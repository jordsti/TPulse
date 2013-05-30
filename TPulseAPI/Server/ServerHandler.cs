using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Terraria;

namespace TPulseAPI.Server
{
    public class ServerHandler
    {
        private TPulse tPulse;

        public ServerHandler(TPulse tPulse)
        {
            this.tPulse = tPulse;
        }

        /// <summary>
        /// Stops the server after kicking all players with a reason message, and optionally saving the world
        /// </summary>
        /// <param name="save">bool perform a world save before stop (default: true)</param>
        /// <param name="reason">string reason (default: "Server shutting down!")</param>
        public void StopServer(bool save = true, string reason = "Server shutting down!")
        {
            tPulse.ForceKickAll(reason);
            if (save)
                SaveManager.Instance.SaveWorld();

            // Save takes a while so kick again
            tPulse.ForceKickAll(reason);

            // Broadcast so console can see we are shutting down as well
            Utils.Broadcast(reason, Color.Red);

            // Disconnect after kick as that signifies server is exiting and could cause a race
            Netplay.disconnect = true;
        }

    }
}

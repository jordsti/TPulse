using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Terraria;

namespace TPulseAPI.Server
{
    public class PlayerHandler
    {
        private TPulse tPulse;

        public PlayerHandler(TPulse tPulse)
        {
            this.tPulse = tPulse;
        }

        public bool Kick(TPPlayer player, string reason, string adminUserName)
        {
            return Kick(player, reason, false, false, adminUserName);
        }

        /// <summary>
        /// Kicks a player from the server..
        /// </summary>
        /// <param name="ply">int player</param>
        /// <param name="reason">string reason</param>
        /// <param name="force">bool force (default: false)</param>
        /// <param name="silent">bool silent (default: false)</param>
        /// <param name="adminUserName">string adminUserName (default: null)</param>
        /// <param name="saveSSI">bool saveSSI (default: false)</param>
        public bool Kick(TPPlayer player, string reason, bool force = false, bool silent = false, string adminUserName = null, bool saveSSI = false)
        {
            if (!player.ConnectionAlive)
                return true;
            if (force || !player.Group.HasPermission(Permissions.immunetokick))
            {
                string playerName = player.Name;
                player.SilentKickInProgress = silent;
                if (player.IsLoggedIn && saveSSI)
                    player.SaveServerInventory();
                player.Disconnect(string.Format("Kicked: {0}", reason));
                Log.ConsoleInfo(string.Format("Kicked {0} for : {1}", playerName, reason));
                string verb = force ? "force " : "";
                if (!silent)
                {
                    if (string.IsNullOrWhiteSpace(adminUserName))
                        Utils.Broadcast(string.Format("{0} was {1}kicked for {2}", playerName, verb, reason.ToLower()), Color.Green);
                    else
                        Utils.Broadcast(string.Format("{0} {1}kicked {2} for {3}", adminUserName, verb, playerName, reason.ToLower()), Color.Green);
                }
                return true;
            }
            return false;
        }

        public void ForceKick(TPPlayer player, string reason, bool silent = false, bool saveSSI = false)
        {
            Kick(player, reason, true, silent, null, saveSSI);
        }

        public void ForceKickAll(string reason)
        {
            foreach (TPPlayer player in tPulse.Players)
            {
                if (player != null && player.Active)
                {
                    ForceKick(player, reason, false, true);
                }
            }
        }


        public List<TPPlayer> FindPlayer(string plr)
        {
            var found = new List<TPPlayer>();
            // Avoid errors caused by null search
            if (plr == null)
                return found;

            byte plrID;
            if (byte.TryParse(plr, out plrID))
            {
                TPPlayer player = tPulse.Players[plrID];
                if (player != null && player.Active)
                {
                    return new List<TPPlayer> { player };
                }
            }

            string plrLower = plr.ToLower();
            foreach (TPPlayer player in tPulse.Players)
            {
                if (player != null)
                {
                    // Must be an EXACT match
                    if (player.Name == plr)
                        return new List<TPPlayer> { player };
                    if (player.Name.ToLower().StartsWith(plrLower))
                        found.Add(player);
                }
            }
            return found;
        }

    }
}

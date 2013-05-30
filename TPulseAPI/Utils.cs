/*
TShock, a server mod for Terraria
/*
TPulse, a server mod for Terraria, forked from TShock
Copyright (C) 2013 StiCode

This program is free software: you can redistribute it and/or modify
it under the terms of the GNU General Public License as published by
the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.

This program is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU General Public License for more details.

You should have received a copy of the GNU General Public License
along with this program.  If not, see <http://www.gnu.org/licenses/>.
*/
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;
using Terraria;

namespace TPulseAPI
{
	/// <summary>
	/// Utilities and other TShock core calls that don't fit anywhere else
	/// </summary>
	public static class Utils
	{
	    /// <summary>
	    /// The lowest id for a prefix.
	    /// </summary>
	    private const int FirstItemPrefix = 1;

	    /// <summary>
	    /// The highest id for a prefix.
	    /// </summary>
	    private const int LastItemPrefix = 83;

		public static Random Random = new Random();
		//private static List<Group> groups = new List<Group>();

		/// <summary>
		/// Provides the real IP address from a RemoteEndPoint string that contains a port and an IP
		/// </summary>
		/// <param name="mess">A string IPv4 address in IP:PORT form.</param>
		/// <returns>A string IPv4 address.</returns>
		public static string GetRealIP(string mess)
		{
			return mess.Split(':')[0];
		}

		/// <summary>
		/// It's a clamp function
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="value">Value to clamp</param>
		/// <param name="max">Maximum bounds of the clamp</param>
		/// <param name="min">Minimum bounds of the clamp</param>
		/// <returns></returns>
		public static T Clamp<T>(T value, T max, T min)
			where T : IComparable<T>
		{
			T result = value;
			if (value.CompareTo(max) > 0)
				result = max;
			if (value.CompareTo(min) < 0)
				result = min;
			return result;
		}

		/// <summary>
		/// Saves the map data
		/// </summary>
		public static void SaveWorld()
		{
			SaveManager.Instance.SaveWorld();
		}

		/// <summary>
		/// Broadcasts a message to all players
		/// </summary>
		/// <param name="msg">string message</param>
		[Obsolete("Use TSPlayer.All and send a message via that method rather than using Broadcast.")]
		public static void Broadcast(string msg)
		{
			Broadcast(msg, Color.Green);
		}

		public static void Broadcast(string msg, byte red, byte green, byte blue)
		{
			TPPlayer.All.SendMessage(msg, red, green, blue);
			TPPlayer.Server.SendMessage(msg, red, green, blue);
			Log.Info(string.Format("Broadcast: {0}", msg));
		}

		public static void Broadcast(string msg, Color color)
		{
			Broadcast(msg, color.R, color.G, color.B);
		}

        /// <summary>
        /// Broadcasts a message from a player, not TShock
        /// </summary>
        /// <param name="ply">TSPlayer ply - the player that will send the packet</param>
        /// <param name="msg">string msg - the message</param>
        /// <param name="red">r</param>
        /// <param name="green">g</param>
        /// <param name="blue">b</param>
        public static void Broadcast(int ply, string msg, byte red, byte green, byte blue)
        {
            TPPlayer.All.SendMessageFromPlayer(msg, red, green, blue, ply);
            TPPlayer.Server.SendMessage(Main.player[ply].name + ": " + msg, red, green, blue);
            Log.Info(string.Format("Broadcast: {0}", Main.player[ply].name + ": " + msg));
        }
        /*
		/// <summary>
		/// Sends message to all users with 'logs' permission.
		/// </summary>
		/// <param name="log">Message to send</param>
		/// <param name="color">Color of the message</param>
        [Obsolete("To be move")]
		public static void SendLogs(string log, Color color, TPulse tPulse)
		{
			Log.Info(log);
			TPPlayer.Server.SendMessage(log, color);
			foreach (TPPlayer player in TPulse.Players)
			{
				if (player != null && player.Active && player.Group.HasPermission(Permissions.logs) && player.DisplayLogs &&
				    tPulse.Config.DisableSpewLogs == false)
					player.SendMessage(log, color);
			}
		}
        */
		/// <summary>
		/// The number of active players on the server.
		/// </summary>
		/// <returns>int playerCount</returns>
        [Obsolete("Must put this method into TPulse")]
		public static int ActivePlayers()
		{
			return Main.player.Where(p => null != p && p.active).Count();
		}
        /*
		/// <summary>
		/// Finds a TSPlayer based on name or ID
		/// </summary>
		/// <param name="plr">Player name or ID</param>
		/// <returns></returns>
        [Obsolete("Must put this method into TPulse")]
		public static List<TPPlayer> FindPlayer(string plr)
		{
			var found = new List<TPPlayer>();
			// Avoid errors caused by null search
			if (plr == null)
				return found;

			byte plrID;
			if (byte.TryParse(plr, out plrID))
			{
				TPPlayer player = TPulse.Players[plrID];
				if (player != null && player.Active)
				{
					return new List<TPPlayer> { player };
				}
			}

			string plrLower = plr.ToLower();
			foreach (TPPlayer player in TPulse.Players)
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
        */
		/// <summary>
		/// Gets a random clear tile in range
		/// </summary>
		/// <param name="startTileX">Bound X</param>
		/// <param name="startTileY">Bound Y</param>
		/// <param name="tileXRange">Range on the X axis</param>
		/// <param name="tileYRange">Range on the Y axis</param>
		/// <param name="tileX">X location</param>
		/// <param name="tileY">Y location</param>
        [Obsolete("Need to be placed into a maptools class")]
		public static void GetRandomClearTileWithInRange(int startTileX, int startTileY, int tileXRange, int tileYRange,
		                                          out int tileX, out int tileY)
		{
			int j = 0;
			do
			{
				if (j == 100)
				{
					tileX = startTileX;
					tileY = startTileY;
					break;
				}

				tileX = startTileX + Random.Next(tileXRange*-1, tileXRange);
				tileY = startTileY + Random.Next(tileYRange*-1, tileYRange);
				j++;
			} while (TileValid(tileX, tileY) && !TileClear(tileX, tileY));
		}

		/// <summary>
		/// Determines if a tile is valid
		/// </summary>
		/// <param name="tileX">Location X</param>
		/// <param name="tileY">Location Y</param>
		/// <returns>If the tile is valid</returns>
        [Obsolete("Need to be placed into a maptools class")]
		public static bool TileValid(int tileX, int tileY)
		{
			return tileX >= 0 && tileX < Main.maxTilesX && tileY >= 0 && tileY < Main.maxTilesY;
		}

		/// <summary>
		/// Checks to see if the tile is clear.
		/// </summary>
		/// <param name="tileX">Location X</param>
		/// <param name="tileY">Location Y</param>
		/// <returns>The state of the tile</returns>
        [Obsolete("Need to be placed into a maptools class")]
		private static bool TileClear(int tileX, int tileY)
		{
			return !Main.tile[tileX, tileY].active;
		}

        /*
		/// <summary>
		/// Kicks all player from the server without checking for immunetokick permission.
		/// </summary>
		/// <param name="ply">int player</param>
		/// <param name="reason">string reason</param>
		public static void ForceKickAll(string reason)
		{
			foreach (TPPlayer player in TPulse.Players)
			{
				if (player != null && player.Active)
				{
					ForceKick(player, reason, false, true);
				}
			}
		}*/
        /*
		/// <summary>
		/// Stops the server after kicking all players with a reason message, and optionally saving the world
		/// </summary>
		/// <param name="save">bool perform a world save before stop (default: true)</param>
		/// <param name="reason">string reason (default: "Server shutting down!")</param>
		public static void StopServer(bool save = true, string reason = "Server shutting down!")
		{
			ForceKickAll(reason);
			if (save)
				SaveManager.Instance.SaveWorld();

			// Save takes a while so kick again
			ForceKickAll(reason);

			// Broadcast so console can see we are shutting down as well
			Utils.Broadcast(reason, Color.Red);

			// Disconnect after kick as that signifies server is exiting and could cause a race
			Netplay.disconnect = true;
		}*/
        /*
#if COMPAT_SIGS
		[Obsolete("This method is for signature compatibility for external code only")]
		public static void ForceKick(TPPlayer player, string reason)
		{
			Kick(player, reason, true, false, string.Empty);
		}
#endif
         * */
        /*
		/// <summary>
		/// Kicks a player from the server without checking for immunetokick permission.
		/// </summary>
		/// <param name="ply">int player</param>
		/// <param name="reason">string reason</param>
		/// <param name="silent">bool silent (default: false)</param>
		public static void ForceKick(TPPlayer player, string reason, bool silent = false, bool saveSSI = false)
		{
			Kick(player, reason, true, silent, null, saveSSI);
		}*/
        /*
#if COMPAT_SIGS
		[Obsolete("This method is for signature compatibility for external code only")]
		public static bool Kick(TPPlayer player, string reason, string adminUserName)
		{
			return Kick(player, reason, false, false, adminUserName);
		}
#endif
		/// <summary>
		/// Kicks a player from the server..
		/// </summary>
		/// <param name="ply">int player</param>
		/// <param name="reason">string reason</param>
		/// <param name="force">bool force (default: false)</param>
		/// <param name="silent">bool silent (default: false)</param>
		/// <param name="adminUserName">string adminUserName (default: null)</param>
		/// <param name="saveSSI">bool saveSSI (default: false)</param>
		public static bool Kick(TPPlayer player, string reason, bool force = false, bool silent = false, string adminUserName = null, bool saveSSI = false)
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
                        Broadcast(string.Format("{0} was {1}kicked for {2}", playerName, verb, reason.ToLower()), Color.Green);
                    else
						Broadcast(string.Format("{0} {1}kicked {2} for {3}", adminUserName, verb, playerName, reason.ToLower()), Color.Green);
                }
				return true;
			}
			return false;
		}*/

        #region to be removed

        /*
#if COMPAT_SIGS
		[Obsolete("This method is for signature compatibility for external code only")]
		public static bool Ban(TPPlayer player, string reason, string adminUserName)
		{
			return Ban(player, reason, false, adminUserName);
		}
#endif
 * */
        /*
		/// <summary>
		/// Bans and kicks a player from the server.
		/// </summary>
		/// <param name="ply">int player</param>
		/// <param name="reason">string reason</param>
		/// <param name="force">bool force (default: false)</param>
		/// <param name="adminUserName">bool silent (default: null)</param>
		public static bool Ban(TPPlayer player, string reason, bool force = false, string adminUserName = null)
		{
			if (!player.ConnectionAlive)
				return true;
			if (force || !player.Group.HasPermission(Permissions.immunetoban))
			{
				string ip = player.IP;
				string playerName = player.Name;
				TPulse.Bans.AddBan(ip, playerName, reason);
				player.Disconnect(string.Format("Banned: {0}", reason));
				Log.ConsoleInfo(string.Format("Banned {0} for : {1}", playerName, reason));
				string verb = force ? "force " : "";
				if (string.IsNullOrWhiteSpace(adminUserName))
					Broadcast(string.Format("{0} was {1}banned for {2}", playerName, verb, reason.ToLower()));
				else
					Broadcast(string.Format("{0} {1}banned {2} for {3}", adminUserName, verb, playerName, reason.ToLower()));
				return true;
			}
			return false;
		}
        */
        #endregion

        /// <summary>
		/// Shows a file to the user.
		/// </summary>
		/// <param name="ply">int player</param>
		/// <param name="file">string filename reletave to savedir</param>
        [Obsolete("Need to rework this method")]
		public static void ShowFileToUser(TPPlayer player, string file, string players)
		{
			string foo = "";
			using (var tr = new StreamReader(Path.Combine(TPulse.SavePath, file)))
			{
				while ((foo = tr.ReadLine()) != null)
				{
					foo = foo.Replace("%map%", Main.worldName);
					foo = foo.Replace("%players%", players);
					//foo = SanitizeString(foo);
					if (foo.Substring(0, 1) == "%" && foo.Substring(12, 1) == "%") //Look for a beginning color code.
					{
						string possibleColor = foo.Substring(0, 13);
						foo = foo.Remove(0, 13);
						float[] pC = {0, 0, 0};
						possibleColor = possibleColor.Replace("%", "");
						string[] pCc = possibleColor.Split(',');
						if (pCc.Length == 3)
						{
							try
							{
								player.SendMessage(foo, (byte) Convert.ToInt32(pCc[0]), (byte) Convert.ToInt32(pCc[1]),
								                   (byte) Convert.ToInt32(pCc[2]));
								continue;
							}
							catch (Exception e)
							{
								Log.Error(e.ToString());
							}
						}
					}
					player.SendMessage(foo);
				}
			}
		}

        #region to be removed
        /*/// <summary>
		/// Returns a Group from the name of the group
		/// </summary>
		/// <param name="ply">string groupName</param>
        [Obsolete("To be move")]
		public static Group GetGroup(string groupName, TPulse tPulse)
		{
			//first attempt on cached groups
			for (int i = 0; i < TPulse.Groups.groups.Count; i++)
			{
				if (TPulse.Groups.groups[i].Name.Equals(groupName))
				{
					return TPulse.Groups.groups[i];
				}
			}
			return new Group(tPulse.Config.DefaultGuestGroupName);
		}*/
        #endregion
        /// <summary>
		/// Returns an IPv4 address from a DNS query
		/// </summary>
		/// <param name="hostname">string ip</param>
		public static string GetIPv4Address(string hostname)
		{
			try
			{
				//Get the ipv4 address from GetHostAddresses, if an ip is passed it will return that ip
				var ip = Dns.GetHostAddresses(hostname).FirstOrDefault(i => i.AddressFamily == AddressFamily.InterNetwork);
				//if the dns query was successful then return it, otherwise return an empty string
				return ip != null ? ip.ToString() : "";
			}
			catch (SocketException)
			{
			}
			return "";
		}

        /// <summary>
        /// Default hashing algorithm.
        /// </summary>
        public static string HashAlgo = "sha512";

        /// <summary>
        /// A dictionary of hashing algortihms and an implementation object.
        /// </summary>
		public static readonly Dictionary<string, Func<HashAlgorithm>> HashTypes = new Dictionary<string, Func<HashAlgorithm>>
		                                                                    	{
		                                                                    		{"sha512", () => new SHA512Managed()},
		                                                                    		{"sha256", () => new SHA256Managed()},
		                                                                    		{"md5", () => new MD5Cng()},
		                                                                    		{"sha512-xp", () => SHA512.Create()},
		                                                                    		{"sha256-xp", () => SHA256.Create()},
		                                                                    		{"md5-xp", () => MD5.Create()},
		                                                                    	};

		/// <summary>
		/// Returns a Sha256 string for a given string
		/// </summary>
		/// <param name="bytes">bytes to hash</param>
		/// <returns>string sha256</returns>
		public static string HashPassword(byte[] bytes)
		{
			if (bytes == null)
				throw new NullReferenceException("bytes");
			Func<HashAlgorithm> func;
			if (!HashTypes.TryGetValue(HashAlgo.ToLower(), out func))
				throw new NotSupportedException("Hashing algorithm {0} is not supported".SFormat(HashAlgo.ToLower()));

			using (var hash = func())
			{
				var ret = hash.ComputeHash(bytes);
				return ret.Aggregate("", (s, b) => s + b.ToString("X2"));
			}
		}

		/// <summary>
		/// Returns a Sha256 string for a given string
		/// </summary>
		/// <param name="bytes">bytes to hash</param>
		/// <returns>string sha256</returns>
		public static string HashPassword(string password)
		{
			if (string.IsNullOrEmpty(password) || password == "non-existant password")
				return "non-existant password";
			return HashPassword(Encoding.UTF8.GetBytes(password));
		}

		/// <summary>
		/// Checks if the string contains any unprintable characters
		/// </summary>
		/// <param name="str">String to check</param>
		/// <returns>True if the string only contains printable characters</returns>
		public static bool ValidString(string str)
		{
			foreach (var c in str)
			{
				if (c < 0x20 || c > 0xA9)
					return false;
			}
			return true;
		}

		/// <summary>
		/// Checks if world has hit the max number of chests
		/// </summary>
		/// <returns>True if the entire chest array is used</returns>
		public static bool MaxChests()
		{
			for (int i = 0; i < Main.chest.Length; i++)
			{
				if (Main.chest[i] == null)
					return false;
			}
			return true;
		}

		/// <summary>
		/// Searches for a projectile by identity and owner
		/// </summary>
		/// <param name="identity">identity</param>
		/// <param name="owner">owner</param>
		/// <returns>projectile ID</returns>
		public static int SearchProjectile(short identity, int owner)
		{
			for (int i = 0; i < Main.maxProjectiles; i++)
			{
				if (Main.projectile[i].identity == identity && Main.projectile[i].owner == owner)
					return i;
			}
			return 1000;
		}

		/// <summary>
		/// Sanitizes input strings
		/// </summary>
		/// <param name="str">string</param>
		/// <returns>sanitized string</returns>
		public static string SanitizeString(string str)
		{
			var returnstr = str.ToCharArray();
			for (int i = 0; i < str.Length; i++)
			{
				if (!ValidString(str[i].ToString()))
					returnstr[i] = ' ';
			}
			return new string(returnstr);
		}
	}
}

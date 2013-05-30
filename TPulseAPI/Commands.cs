﻿/*
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
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using Terraria;
using TPulseAPI.DB;
using System.Reflection;

namespace TPulseAPI
{
    //making this class not static

	public delegate void CommandDelegate(CommandArgs args);

	public class Commands
	{
        //Player Login Event Handling

        private event PlayerLoginHandler _onPlayerLogin;

        public List<Command> ChatCommands { get; protected set; }

		private delegate void AddChatCommand(string permission, CommandDelegate command, params string[] names);


        public Commands()
        {
            ChatCommands = new List<Command>();
        }

        public event PlayerLoginHandler OnPlayerLogin
        {
            add
            {
                _onPlayerLogin += value;
            }
            remove
            {
                _onPlayerLogin -= value;
            }
        }

        public void HandlePlayerLogin(PlayerLoginEventArgs args)
        {
            if (_onPlayerLogin != null)
                _onPlayerLogin.Invoke(args);
        }

        //call this in the constructor maybe?
		public void InitCommands()
		{
			AddChatCommand add = (p, c, n) => ChatCommands.Add(new Command(p, c, n));
            ChatCommands.Add(new Command(AuthToken, "auth") { AllowServer = false });
            ChatCommands.Add(new Command(Permissions.canchangepassword, PasswordUser, "password") { AllowServer = false, DoLog = false });
            ChatCommands.Add(new Command(Permissions.canregister, RegisterUser, "register") { AllowServer = false, DoLog = false });
            ChatCommands.Add(new Command(Permissions.rootonly, ManageUsers, "user") { DoLog = false });
            ChatCommands.Add(new Command(Permissions.canlogin, AttemptLogin, "login") { AllowServer = true, DoLog = false });
            ChatCommands.Add(new Command(Permissions.buff, Buff, "buff") { AllowServer = false });
            ChatCommands.Add(new Command(Permissions.cfg, SetSpawn, "setspawn") { AllowServer = false });
            ChatCommands.Add(new Command(Permissions.grow, Grow, "grow") { AllowServer = false });
            ChatCommands.Add(new Command(Permissions.item, Item, "item", "i") { AllowServer = false });
            ChatCommands.Add(new Command(Permissions.tp, Home, "home") { AllowServer = false });
            ChatCommands.Add(new Command(Permissions.canpartychat, PartyChat, "p") { AllowServer = false });
            ChatCommands.Add(new Command(Permissions.tp, Spawn, "spawn") { AllowServer = false });
            ChatCommands.Add(new Command(Permissions.tp, TP, "tp") { AllowServer = false });
            ChatCommands.Add(new Command(Permissions.tp, TPHere, "tphere") { AllowServer = false });
            ChatCommands.Add(new Command(Permissions.tpallow, TPAllow, "tpallow") { AllowServer = false });
			add(Permissions.kick, Kick, "kick");
		    add(Permissions.ban, DeprecateBans, "banip", "listbans", "unban", "unbanip", "clearbans");
			add(Permissions.ban, Ban, "ban");
			add(Permissions.whitelist, Whitelist, "whitelist");
			add(Permissions.maintenance, Off, "off", "exit");
			add(Permissions.maintenance, Restart, "restart"); //Added restart command
			add(Permissions.maintenance, OffNoSave, "off-nosave", "exit-nosave");
			add(Permissions.maintenance, CheckUpdates, "checkupdates");
			add(Permissions.causeevents, DropMeteor, "dropmeteor");
			add(Permissions.causeevents, Star, "star");
			add(Permissions.causeevents, Fullmoon, "fullmoon");
			add(Permissions.causeevents, Bloodmoon, "bloodmoon");
			add(Permissions.causeevents, Invade, "invade");
            add(Permissions.spawnboss, Eater, "eater");
            add(Permissions.spawnboss, Eye, "eye");
            add(Permissions.spawnboss, King, "king");
            add(Permissions.spawnboss, Skeletron, "skeletron");
            add(Permissions.spawnboss, WoF, "wof", "wallofflesh");
            add(Permissions.spawnboss, Twins, "twins");
            add(Permissions.spawnboss, Destroyer, "destroyer");
            add(Permissions.spawnboss, SkeletronPrime, "skeletronp", "prime");
            add(Permissions.spawnboss, Hardcore, "hardcore");
            add(Permissions.spawnmob, SpawnMob, "spawnmob", "sm");
			add(Permissions.warp, Warp, "warp");
		    add(null, DeprecateWarp, "setwarp", "sendwarp", "delwarp", "sw");
			add(Permissions.managegroup, AddGroup, "addgroup");
			add(Permissions.managegroup, DeleteGroup, "delgroup");
			add(Permissions.managegroup, ModifyGroup, "modgroup");
			add(Permissions.managegroup, ViewGroups, "group");
			add(Permissions.manageitem, AddItem, "additem", "banitem");
			add(Permissions.manageitem, DeleteItem, "delitem", "unbanitem");
			add(Permissions.manageitem, ListItems, "listitems", "listbanneditems");
			add(Permissions.manageitem, AddItemGroup, "additemgroup");
			add(Permissions.manageitem, DeleteItemGroup, "delitemgroup");
            add(Permissions.manageregion, Region, "region");
            add(Permissions.manageregion, DebugRegions, "debugreg");
			add(Permissions.cfg, Reload, "reload");
			add(Permissions.cfg, ServerPassword, "serverpassword");
			add(Permissions.cfg, Save, "save");
			add(Permissions.cfg, Settle, "settle");
			add(Permissions.cfg, MaxSpawns, "maxspawns");
			add(Permissions.cfg, SpawnRate, "spawnrate");
			add(Permissions.time, Time, "time");
			add(Permissions.pvpfun, Slap, "slap");
			add(Permissions.editspawn, ToggleAntiBuild, "antibuild");
			add(Permissions.editspawn, ProtectSpawn, "protectspawn");
            add(Permissions.maintenance, GetVersion, "version");
			add(null, ListConnectedPlayers, "playing", "online", "who");
            add(null, Motd, "motd");
            add(null, Rules, "rules");
            add(null, Help, "help");
			add(Permissions.cantalkinthird, ThirdPerson, "me");
			add(Permissions.mute, Mute, "mute", "unmute");
			add(Permissions.logs, DisplayLogs, "displaylogs");
			add(Permissions.userinfo, GrabUserUserInfo, "userinfo", "ui");
			add(Permissions.rootonly, AuthVerify, "auth-verify");
			add(Permissions.cfg, Broadcast, "broadcast", "bc", "say");
			add(Permissions.whisper, Whisper, "whisper", "w", "tell");
			add(Permissions.whisper, Reply, "reply", "r");
			add(Permissions.annoy, Annoy, "annoy");
			add(Permissions.kill, Kill, "kill");
			add(Permissions.butcher, Butcher, "butcher");
			add(Permissions.item, Give, "give", "g");
			add(Permissions.clearitems, ClearItems, "clear", "clearitems");
			add(Permissions.heal, Heal, "heal");
			add(Permissions.buffplayer, GBuff, "gbuff", "buffplayer");
			add(Permissions.hardmode, StartHardMode, "hardmode");
			add(Permissions.hardmode, DisableHardMode, "stophardmode", "disablehardmode");
			add(Permissions.cfg, ServerInfo, "stats");
			add(Permissions.cfg, WorldInfo, "world");
			add(Permissions.savessi, SaveSSI, "savessi");
			add(Permissions.savessi, OverrideSSI, "overridessi", "ossi");
		    add(Permissions.xmas, ForceXmas, "forcexmas");
		    //add(null, TestCallbackCommand, "test");
		}

		public bool HandleCommand(TPPlayer player, string text)
		{
			string cmdText = text.Remove(0, 1);

			var args = ParseParameters(cmdText);
			if (args.Count < 1)
				return false;

			string cmdName = args[0].ToLower();
			args.RemoveAt(0);

			IEnumerable<Command> cmds = ChatCommands.Where(c => c.HasAlias(cmdName));

			if (cmds.Count() == 0)
			{
				if (player.AwaitingResponse.ContainsKey(cmdName))
				{
					Action<CommandArgs> call = player.AwaitingResponse[cmdName];
					player.AwaitingResponse.Remove(cmdName);
					call(new CommandArgs(cmdText, player, args));
					return true;
				}
				player.SendErrorMessage("Invalid command entered. Type /help for a list of valid commands.");
				return true;
			}
            foreach (Command cmd in cmds)
            {
                if (!cmd.CanRun(player))
                {
                    TPulse.Utils.SendLogs(string.Format("{0} tried to execute /{1}.", player.Name, cmdText), Color.Red);
                    player.SendErrorMessage("You do not have access to that command.");
                }
                else if (!cmd.AllowServer && !player.RealPlayer)
                {
                    player.SendErrorMessage("You must use this command in-game.");
                }
                else
                {
                    if (cmd.DoLog)
                        TPulse.Utils.SendLogs(string.Format("{0} executed: /{1}.", player.Name, cmdText), Color.Red);
                    cmd.Run(cmdText, player, args);
                }
            }
		    return true;
		}



#region static method
		/// <summary>
		/// Parses a string of parameters into a list. Handles quotes.
		/// </summary>
		/// <param name="str"></param>
		/// <returns></returns>
		private static List<String> ParseParameters(string str)
		{
			var ret = new List<string>();
			var sb = new StringBuilder();
			bool instr = false;
			for (int i = 0; i < str.Length; i++)
			{
				char c = str[i];

				if (instr)
				{
					if (c == '\\')
					{
						if (i + 1 >= str.Length)
							break;
						c = GetEscape(str[++i]);
					}
					else if (c == '"')
					{
						ret.Add(sb.ToString());
						sb.Clear();
						instr = false;
						continue;
					}
					sb.Append(c);
				}
				else
				{
					if (IsWhiteSpace(c))
					{
						if (sb.Length > 0)
						{
							ret.Add(sb.ToString());
							sb.Clear();
						}
					}
					else if (c == '"')
					{
						if (sb.Length > 0)
						{
							ret.Add(sb.ToString());
							sb.Clear();
						}
						instr = true;
					}
					else
					{
						sb.Append(c);
					}
				}
			}
			if (sb.Length > 0)
				ret.Add(sb.ToString());

			return ret;
		}

		private static char GetEscape(char c)
		{
			switch (c)
			{
				case '\\':
					return '\\';
				case '"':
					return '"';
				case 't':
					return '\t';
				default:
					return c;
			}
		}

		private static bool IsWhiteSpace(char c)
		{
			return c == ' ' || c == '\t' || c == '\n';
		}
#endregion
        //private static void TestCallbackCommand(CommandArgs args)
        //{
        //    Action<object> a = (s) => { ((CommandArgs)s).Player.SendSuccessMessage("This is your callack"); };
        //    args.Player.AddResponse( "yes", a);
        //    args.Player.SendInfoMessage( "Type /yes to get called back." );
        //}

		#region Account commands

		public void AttemptLogin(CommandArgs args)
		{
			if (args.Player.LoginAttempts > TPulse.Config.MaximumLoginAttempts && (TPulse.Config.MaximumLoginAttempts != -1))
			{
				Log.Warn(String.Format("{0} ({1}) had {2} or more invalid login attempts and was kicked automatically.",
					args.Player.IP, args.Player.Name, TPulse.Config.MaximumLoginAttempts));
				TPulse.Utils.Kick(args.Player, "Too many invalid login attempts.");
				return;
			}

			User user = TPulse.Users.GetUserByName(args.Player.Name);
			string encrPass = "";

			if (args.Parameters.Count == 1)
			{
				user = TPulse.Users.GetUserByName(args.Player.Name);
				encrPass = TPulse.Utils.HashPassword(args.Parameters[0]);
			}
			else if (args.Parameters.Count == 2 && TPulse.Config.AllowLoginAnyUsername)
			{
				user = TPulse.Users.GetUserByName(args.Parameters[0]);
				encrPass = TPulse.Utils.HashPassword(args.Parameters[1]);
				if (String.IsNullOrEmpty(args.Parameters[0]))
				{
					args.Player.SendErrorMessage("Bad login attempt.");
					return;
				}
			}
			else
			{
				args.Player.SendErrorMessage(String.Format("Syntax: /login{0} <password>", TPulse.Config.AllowLoginAnyUsername ? " " : " [username]"));
				args.Player.SendErrorMessage("If you forgot your password, there is no way to recover it.");
				return;
			}
			try
			{
				if (user == null)
				{
					args.Player.SendErrorMessage("A user by that name does not exist.");
				}
				else if (user.Password.ToUpper() == encrPass.ToUpper())
				{
					args.Player.PlayerData = TPulse.InventoryDB.GetPlayerData(args.Player, TPulse.Users.GetUserID(user.Name));

					var group = TPulse.Utils.GetGroup(user.Group);

					if (TPulse.Config.ServerSideInventory)
					{
						if (group.HasPermission(Permissions.bypassinventorychecks))
						{
							args.Player.IgnoreActionsForClearingTrashCan = false;
						}
						else if (!TPulse.CheckInventory(args.Player))
						{
							args.Player.SendErrorMessage("Login failed. Please fix the above errors then /login again.");
							args.Player.IgnoreActionsForClearingTrashCan = true;
							return;
						}
					}

					if (group.HasPermission(Permissions.ignorestackhackdetection))
						args.Player.IgnoreActionsForCheating = "none";

					if (group.HasPermission(Permissions.usebanneditem))
						args.Player.IgnoreActionsForDisabledArmor = "none";

					args.Player.Group = group;
					args.Player.UserAccountName = user.Name;
					args.Player.UserID = TPulse.Users.GetUserID(args.Player.UserAccountName);
					args.Player.IsLoggedIn = true;
					args.Player.IgnoreActionsForInventory = "none";

					if (!args.Player.IgnoreActionsForClearingTrashCan)
					{
						args.Player.PlayerData.CopyInventory(args.Player);
						TPulse.InventoryDB.InsertPlayerData(args.Player);
					}
					args.Player.SendSuccessMessage("Authenticated as " + user.Name + " successfully.");

					Log.ConsoleInfo(args.Player.Name + " authenticated successfully as user: " + user.Name + ".");
					if ((args.Player.LoginHarassed) && (TPulse.Config.RememberLeavePos))
					{
						if (TPulse.RememberedPos.GetLeavePos(args.Player.Name, args.Player.IP) != Vector2.Zero)
						{
							Vector2 pos = TPulse.RememberedPos.GetLeavePos(args.Player.Name, args.Player.IP);
							args.Player.Teleport((int)pos.X, (int)pos.Y + 3);
						}
						args.Player.LoginHarassed = false;

					}

				    //Hooks.PlayerLoginEvent.OnPlayerLogin(args.Player);
                    HandlePlayerLogin(new PlayerLoginEventArgs(args.Player));
				}
				else
				{
					args.Player.SendErrorMessage("Incorrect password.");
					Log.Warn(args.Player.IP + " failed to authenticate as user: " + user.Name + ".");
					args.Player.LoginAttempts++;
				}
			}
			catch (Exception ex)
			{
				args.Player.SendErrorMessage("There was an error processing your request.");
				Log.Error(ex.ToString());
			}
		}

		private void PasswordUser(CommandArgs args)
		{
			try
			{
				if (args.Player.IsLoggedIn && args.Parameters.Count == 2)
				{
					var user = TPulse.Users.GetUserByName(args.Player.UserAccountName);
					string encrPass = TPulse.Utils.HashPassword(args.Parameters[0]);
					if (user.Password.ToUpper() == encrPass.ToUpper())
					{
						args.Player.SendSuccessMessage("You changed your password to " + args.Parameters[1] + "!");
						TPulse.Users.SetUserPassword(user, args.Parameters[1]); // SetUserPassword will hash it for you.
						Log.ConsoleInfo(args.Player.IP + " named " + args.Player.Name + " changed the password of account " + user.Name + ".");
					}
					else
					{
						args.Player.SendErrorMessage("You failed to change your password!");
						Log.ConsoleError(args.Player.IP + " named " + args.Player.Name + " failed to change password for account: " +
										 user.Name + ".");
					}
				}
				else
				{
					args.Player.SendErrorMessage("Not logged in or invalid syntax! Syntax: /password <oldpassword> <newpassword>");
				}
			}
			catch (UserManagerException ex)
			{
				args.Player.SendErrorMessage("Sorry, an error occured: " + ex.Message + ".");
				Log.ConsoleError("PasswordUser returned an error: " + ex);
			}
		}

		private void RegisterUser(CommandArgs args)
		{
			try
			{
				var user = new User();

				if (args.Parameters.Count == 1)
				{
					user.Name = args.Player.Name;
					user.Password = args.Parameters[0];
				}
				else if (args.Parameters.Count == 2 && TPulse.Config.AllowRegisterAnyUsername)
				{
					user.Name = args.Parameters[0];
					user.Password = args.Parameters[1];
				}
				else
				{
					args.Player.SendErrorMessage("Invalid syntax! Proper syntax: /register <password>");
					return;
				}

				user.Group = TPulse.Config.DefaultRegistrationGroupName; // FIXME -- we should get this from the DB. --Why?

				if (TPulse.Users.GetUserByName(user.Name) == null) // Cheap way of checking for existance of a user
				{
					args.Player.SendSuccessMessage("Account " + user.Name + " has been registered.");
					args.Player.SendSuccessMessage("Your password is " + user.Password);
					TPulse.Users.AddUser(user);
					Log.ConsoleInfo(args.Player.Name + " registered an account: " + user.Name + ".");
				}
				else
				{
					args.Player.SendErrorMessage("Account " + user.Name + " has already been registered.");
					Log.ConsoleInfo(args.Player.Name + " failed to register an existing account: " + user.Name);
				}
			}
			catch (UserManagerException ex)
			{
				args.Player.SendErrorMessage("Sorry, an error occured: " + ex.Message + ".");
				Log.ConsoleError("RegisterUser returned an error: " + ex);
			}
		}

		private void ManageUsers(CommandArgs args)
		{
			// This guy needs to be here so that people don't get exceptions when they type /user
			if (args.Parameters.Count < 1)
			{
				args.Player.SendErrorMessage("Invalid user syntax. Try /user help.");
				return;
			}

			string subcmd = args.Parameters[0];

			// Add requires a username:password pair/ip address and a group specified.
			if (subcmd == "add")
			{
				var namepass = args.Parameters[1].Split(':');
				var user = new User();

				try
				{
					if (args.Parameters.Count > 2)
					{
						if (namepass.Length == 2)
						{
							user.Name = namepass[0];
							user.Password = namepass[1];
							user.Group = args.Parameters[2];
						}
						else if (namepass.Length == 1)
						{
							user.Address = namepass[0];
							user.Group = args.Parameters[2];
							user.Name = user.Address;
						}
						if (!string.IsNullOrEmpty(user.Address))
						{
							args.Player.SendSuccessMessage("IP address admin added. If they're logged in, tell them to rejoin.");
							args.Player.SendSuccessMessage("WARNING: This is insecure! It would be better to use a user account instead.");
							TPulse.Users.AddUser(user);
							Log.ConsoleInfo(args.Player.Name + " added IP " + user.Address + " to group " + user.Group);
						}
						else
						{
							args.Player.SendSuccessMessage("Account " + user.Name + " has been added to group " + user.Group + "!");
							TPulse.Users.AddUser(user);
							Log.ConsoleInfo(args.Player.Name + " added Account " + user.Name + " to group " + user.Group);
						}
					}
					else
					{
						args.Player.SendErrorMessage("Invalid syntax. Try /user help.");
					}
				}
				catch (UserManagerException ex)
				{
					args.Player.SendErrorMessage(ex.Message);
					Log.ConsoleError(ex.ToString());
				}
			}
				// User deletion requires a username
			else if (subcmd == "del" && args.Parameters.Count == 2)
			{
				var user = new User();
				if (args.Parameters[1].Split('.').Count() ==4)

					//              changed to support dot character in usernames
					//				if (args.Parameters[1].Contains("."))
					user.Address = args.Parameters[1];
				else
					user.Name = args.Parameters[1];

				try
				{
					TPulse.Users.RemoveUser(user);
					args.Player.SendSuccessMessage("Account removed successfully.");
					Log.ConsoleInfo(args.Player.Name + " successfully deleted account: " + args.Parameters[1] + ".");
				}
				catch (UserManagerException ex)
				{
					args.Player.SendMessage(ex.Message, Color.Red);
					Log.ConsoleError(ex.ToString());
				}
			}
				// Password changing requires a username, and a new password to set
			else if (subcmd == "password")
			{
				var user = new User();
				user.Name = args.Parameters[1];

				try
				{
					if (args.Parameters.Count == 3)
					{
						args.Player.SendSuccessMessage("Password change succeeded for " + user.Name + ".");
						TPulse.Users.SetUserPassword(user, args.Parameters[2]);
						Log.ConsoleInfo(args.Player.Name + " changed the password of account " + user.Name);
					}
					else
					{
						args.Player.SendErrorMessage("Invalid user password syntax. Try /user help.");
					}
				}
				catch (UserManagerException ex)
				{
					args.Player.SendErrorMessage(ex.Message);
					Log.ConsoleError(ex.ToString());
				}
			}
				// Group changing requires a username or IP address, and a new group to set
			else if (subcmd == "group")
			{
				var user = new User();
				if (args.Parameters[1].Split('.').Count()==4)

				//changed to support dot character in usernames
				//if (args.Parameters[1].Contains("."))

					user.Address = args.Parameters[1];
				else
					user.Name = args.Parameters[1];

				try
				{
					if (args.Parameters.Count == 3)
					{
						if (!string.IsNullOrEmpty(user.Address))
						{
							args.Player.SendSuccessMessage("IP address " + user.Address + " has been changed to group " + args.Parameters[2] + "!");
							TPulse.Users.SetUserGroup(user, args.Parameters[2]);
							Log.ConsoleInfo(args.Player.Name + " changed IP address " + user.Address + " to group " + args.Parameters[2] + ".");
						}
						else
						{
							args.Player.SendSuccessMessage("Account " + user.Name + " has been changed to group " + args.Parameters[2] + "!");
							TPulse.Users.SetUserGroup(user, args.Parameters[2]);
							Log.ConsoleInfo(args.Player.Name + " changed account " + user.Name + " to group " + args.Parameters[2] + ".");
						}
					}
					else
					{
						args.Player.SendErrorMessage("Invalid user group syntax. Try /user help.");
					}
				}
				catch (UserManagerException ex)
				{
					args.Player.SendMessage(ex.Message, Color.Green);
					Log.ConsoleError(ex.ToString());
				}
			}
			else if (subcmd == "help")
			{
				args.Player.SendInfoMessage("Use command help:");
				args.Player.SendInfoMessage("/user add username:password group   -- Adds a specified user");
				args.Player.SendInfoMessage("/user del username                  -- Removes a specified user");
				args.Player.SendInfoMessage("/user password username newpassword -- Changes a user's password");
				args.Player.SendInfoMessage("/user group username newgroup       -- Changes a user's group");
			}
			else
			{
				args.Player.SendErrorMessage("Invalid user syntax. Try /user help.");
			}
		}

		#endregion

		#region Stupid commands

		public void ServerInfo(CommandArgs args)
		{
			args.Player.SendInfoMessage("Memory usage: " + Process.GetCurrentProcess().WorkingSet64);
			args.Player.SendInfoMessage("Allocated memory: " + Process.GetCurrentProcess().VirtualMemorySize64);
			args.Player.SendInfoMessage("Total processor time: " + Process.GetCurrentProcess().TotalProcessorTime);
			args.Player.SendInfoMessage("WinVer: " + Environment.OSVersion);
			args.Player.SendInfoMessage("Proc count: " + Environment.ProcessorCount);
			args.Player.SendInfoMessage("Machine name: " + Environment.MachineName);
		}

		public void WorldInfo(CommandArgs args)
		{
			args.Player.SendInfoMessage("World name: " + Main.worldName);
			args.Player.SendInfoMessage("World ID: " + Main.worldID);
		}

		#endregion

		#region Player Management Commands

		private void GrabUserUserInfo(CommandArgs args)
		{
			if (args.Parameters.Count < 1)
			{
				args.Player.SendErrorMessage("Invalid syntax! Proper syntax: /userinfo <player>");
				return;
			}

			var players = TPulse.Utils.FindPlayer(args.Parameters[0]);
			if (players.Count > 1)
			{
				var plrMatches = "";
				foreach (TPPlayer plr in players)
				{
					if (plrMatches.Length != 0)
					{
						plrMatches += ", " + plr.Name;
					}
					else
					{
						plrMatches += plr.Name;
					}
				}
				args.Player.SendErrorMessage("More than one player matched! Matches: " + plrMatches);
				return;
			}
			try
			{
				args.Player.SendSuccessMessage("IP Address: " + players[0].IP + " Logged in as: " + players[0].UserAccountName + " group: " + players[0].Group.Name);
			}
			catch (Exception)
			{
				args.Player.SendErrorMessage("Invalid player.");
			}
		}

		private void Kick(CommandArgs args)
		{
			if (args.Parameters.Count < 1)
			{
				args.Player.SendErrorMessage("Invalid syntax! Proper syntax: /kick <player> [reason]");
				return;
			}
			if (args.Parameters[0].Length == 0)
			{
				args.Player.SendErrorMessage("Missing player name.");
				return;
			}

			string plStr = args.Parameters[0];
			var players = TPulse.Utils.FindPlayer(plStr);
			if (players.Count == 0)
			{
				args.Player.SendErrorMessage("Invalid player!");
			}
			else if (players.Count > 1)
			{
				var plrMatches = "";
				foreach (TPPlayer plr in players)
				{
					if (plrMatches.Length != 0)
					{
						plrMatches += ", " + plr.Name;
					}
					else
					{
						plrMatches += plr.Name;
					}
				}
				args.Player.SendErrorMessage("More than one player matched! Matches: " + plrMatches);
			}
			else
			{
				string reason = args.Parameters.Count > 1
									? String.Join(" ", args.Parameters.GetRange(1, args.Parameters.Count - 1))
									: "Misbehaviour.";
				if (!TPulse.Utils.Kick(players[0], reason, !args.Player.RealPlayer, false, args.Player.Name))
				{
					args.Player.SendErrorMessage("You can't kick another admin!");
				}
			}
		}

        private void DeprecateBans(CommandArgs args)
        {
            args.Player.SendInfoMessage("Syntax: /ban [option] [arguments]");
            args.Player.SendInfoMessage("Options: list, listip, clear, add, addip, del, delip");
            args.Player.SendInfoMessage("Arguments: list, listip, clear [code], add [name], addip [ip], del [name], delip [name]");
            args.Player.SendInfoMessage("In addition, a reason may be provided for all new bans after the arguments.");
            return;
        }

		private void Ban(CommandArgs args)
		{
			if (args.Parameters.Count == 0 || args.Parameters[0].ToLower() == "help")
			{
				args.Player.SendInfoMessage("Syntax: /ban [option] [arguments]");
				args.Player.SendInfoMessage("Options: list, listip, clear, add, addip, del, delip");
				args.Player.SendInfoMessage("Arguments: list, listip, clear [code], add [name], addip [ip], del [name], delip [name]");
				args.Player.SendInfoMessage("In addition, a reason may be provided for all new bans after the arguments.");
				return;
			}
			if (args.Parameters[0].ToLower() == "list")
			{
				#region List bans
				if (TPulse.Bans.GetBans().Count == 0)
				{
					args.Player.SendErrorMessage("There are currently no players banned.");
					return;
				}

				string banString = "";
				foreach (Ban b in TPulse.Bans.GetBans())
				{

					if (b.Name.Trim() == "")
					{
						continue;
					}

					if (banString.Length == 0)
					{
						banString = b.Name;
					}
					else
					{
						int length = banString.Length;
						while (length > 60)
						{
							length = length - 60;
						}
						if (length + b.Name.Length >= 60)
						{
							banString += "|, " + b.Name;
						}
						else
						{
							banString += ", " + b.Name;
						}
					}
				}

				String[] banStrings = banString.Split('|');

				if (banStrings.Length == 0)
				{
					args.Player.SendErrorMessage("There are currently no players with valid names banned.");
					return;
				}

				if (banStrings[0].Trim() == "")
				{
					args.Player.SendErrorMessage("There are currently no bans with valid names found.");
					return;
				}

				args.Player.SendInfoMessage("List of banned players:");
				foreach (string s in banStrings)
				{
					args.Player.SendInfoMessage(s);
				}
				return;
				#endregion List bans
			}

			if (args.Parameters[0].ToLower() == "listip")
			{
				#region List ip bans
				if (TPulse.Bans.GetBans().Count == 0)
				{
					args.Player.SendWarningMessage("There are currently no players banned.");
					return;
				}

				string banString = "";
				foreach (Ban b in TPulse.Bans.GetBans())
				{

					if (b.IP.Trim() == "")
					{
						continue;
					}

					if (banString.Length == 0)
					{
						banString = b.IP;
					}
					else
					{
						int length = banString.Length;
						while (length > 60)
						{
							length = length - 60;
						}
						if (length + b.Name.Length >= 60)
						{
							banString += "|, " + b.IP;
						}
						else
						{
							banString += ", " + b.IP;
						}
					}
				}

				String[] banStrings = banString.Split('|');

				if (banStrings.Length == 0)
				{
					args.Player.SendErrorMessage("There are currently no players with valid IPs banned.");
					return;
				}

				if (banStrings[0].Trim() == "")
				{
					args.Player.SendErrorMessage("There are currently no bans with valid IPs found.");
					return;
				}

				args.Player.SendInfoMessage("List of IP banned players:");
				foreach (string s in banStrings)
				{
					args.Player.SendInfoMessage(s);
				}
				return;
				#endregion List ip bans
			}

			if (args.Parameters.Count >= 2)
			{
				if (args.Parameters[0].ToLower() == "add")
				{
					#region Add ban
					string plStr = args.Parameters[1];
					var players = TPulse.Utils.FindPlayer(plStr);
					if (players.Count == 0)
					{
						args.Player.SendErrorMessage("Invalid player!");
					}
					else if (players.Count > 1)
					{
						var plrMatches = "";
						foreach (TPPlayer plr in players)
						{
							if (plrMatches.Length != 0)
							{
								plrMatches += ", " + plr.Name;
							}
							else
							{
								plrMatches += plr.Name;
							}
						}
						args.Player.SendErrorMessage("More than one player matched! Matches: " + plrMatches);
					}
					else
					{
						string reason = args.Parameters.Count > 2
											? String.Join(" ", args.Parameters.GetRange(2, args.Parameters.Count - 2))
											: "Misbehavior.";
						if (!TPulse.Utils.Ban(players[0], reason, !args.Player.RealPlayer, args.Player.Name))
						{
							args.Player.SendErrorMessage("You can't ban another admin!");
						}
					}
					return;
					#endregion Add ban
				}
				else if (args.Parameters[0].ToLower() == "addip")
				{
					#region Add ip ban
					string ip = args.Parameters[1];
					string reason = args.Parameters.Count > 2
										? String.Join(" ", args.Parameters.GetRange(2, args.Parameters.Count - 2))
										: "Manually added IP address ban.";
					TPulse.Bans.AddBan(ip, "", reason);
					args.Player.SendSuccessMessage(ip + " banned.");
					return;
					#endregion Add ip ban
				}
				else if (args.Parameters[0].ToLower() == "delip")
				{
					#region Delete ip ban
					var ip = args.Parameters[1];
					var ban = TPulse.Bans.GetBanByIp(ip);
					if (ban != null)
					{
						if (TPulse.Bans.RemoveBan(ban.IP))
							args.Player.SendSuccessMessage(string.Format("Unbanned {0} ({1})!", ban.Name, ban.IP));
						else
							args.Player.SendErrorMessage(string.Format("Failed to unban {0} ({1})!", ban.Name, ban.IP));
					}
					else
					{
						args.Player.SendErrorMessage(string.Format("No bans for ip {0} exist", ip));
					}
					return;
					#endregion Delete ip ban
				}
				else if (args.Parameters[0].ToLower() == "del")
				{
					#region Delete ban
					string plStr = args.Parameters[1];
					var ban = TPulse.Bans.GetBanByName(plStr, false);
					if (ban != null)
					{
						if (TPulse.Bans.RemoveBan(ban.Name, true))
							args.Player.SendSuccessMessage(string.Format("Unbanned {0} ({1})!", ban.Name, ban.IP));
						else
							args.Player.SendErrorMessage(string.Format("Failed to unban {0} ({1})!", ban.Name, ban.IP));
					}
					else
					{
						args.Player.SendErrorMessage(string.Format("No bans for player {0} exist", plStr));
					}
					return;
					#endregion Delete ban
				}

				#region Clear bans
				if (args.Parameters[0].ToLower() == "clear")
				{
					if (args.Parameters.Count < 1 && ClearBansCode == -1)
					{
						ClearBansCode = new Random().Next(0, short.MaxValue);
						args.Player.SendInfoMessage("ClearBans Code: " + ClearBansCode);
						return;
					}
					if (args.Parameters.Count < 1)
					{
						args.Player.SendErrorMessage("Invalid syntax! Proper syntax: /ban clear <code>");
						return;
					}

					int num;
					if (!int.TryParse(args.Parameters[1], out num))
					{
						args.Player.SendErrorMessage("Invalid syntax! Expected a number, didn't get one.");
						return;
					}

					if (num == ClearBansCode)
					{
						ClearBansCode = -1;
						if (TPulse.Bans.ClearBans())
						{
							Log.ConsoleInfo("Bans cleared.");
							args.Player.SendSuccessMessage("Bans cleared.");
						}
						else
						{
							args.Player.SendErrorMessage("Failed to clear bans.");
						}
					}
					else
					{
						args.Player.SendErrorMessage("Incorrect clear code.");
					}
				}
				return;
				#endregion Clear bans
			}
			args.Player.SendErrorMessage("Invalid syntax or old command provided.");
			args.Player.SendErrorMessage("Type /ban help for more information.");
		}

		private int ClearBansCode = -1;

		public void Whitelist(CommandArgs args)
		{
			if (args.Parameters.Count == 1)
			{
				using (var tw = new StreamWriter(FileTools.WhitelistPath, true))
				{
					tw.WriteLine(args.Parameters[0]);
				}
				args.Player.SendSuccessMessage("Added " + args.Parameters[0] + " to the whitelist.");
			}
		}

		public void DisplayLogs(CommandArgs args)
		{
			args.Player.DisplayLogs = (!args.Player.DisplayLogs);
			args.Player.SendSuccessMessage("You will " + (args.Player.DisplayLogs ? "now" : "no longer") + " receive logs.");
		}

		public void SaveSSI(CommandArgs args )
		{
			if (TPulse.Config.ServerSideInventory)
			{
				args.Player.SendSuccessMessage("SSI has been saved.");
				foreach (TPPlayer player in TPulse.Players)
				{
					if (player != null && player.IsLoggedIn && !player.IgnoreActionsForClearingTrashCan)
					{
						TPulse.InventoryDB.InsertPlayerData(player);
					}
				}
			}
		}

		public void OverrideSSI( CommandArgs args )
		{
			if( args.Parameters.Count < 1 )
			{
				args.Player.SendErrorMessage("Correct usage: /overridessi(/ossi) <player name>");
				return;
			}

			var players = TPulse.Utils.FindPlayer(args.Parameters[0]);
			if( players.Count < 1 )
			{
				args.Player.SendErrorMessage("No players match " + args.Parameters[0] + "!");
			}
			else if( players.Count > 1 )
			{
				args.Player.SendErrorMessage( players.Count + " players matched " + args.Parameters[0] + "!");
			}
			else if (TPulse.Config.ServerSideInventory)
			{
				if( players[0] != null && players[0].IsLoggedIn && !players[0].IgnoreActionsForClearingTrashCan)
				{
					args.Player.SendSuccessMessage( players[0].Name + " has been exempted and updated.");
					TPulse.InventoryDB.InsertPlayerData(players[0]);
				}
			}
		}

        private void ForceXmas(CommandArgs args)
        {
            if(args.Parameters.Count == 0)
            {
                args.Player.SendErrorMessage("Usage: /forcexmas [true/false]");
                args.Player.SendInfoMessage(
                    String.Format("The server is currently {0} force Christmas mode.",
                                (TPulse.Config.ForceXmas ? "in" : "not in")));
                return;
            }

            if(args.Parameters[0].ToLower() == "true")
            {
                TPulse.Config.ForceXmas = true;
                Main.checkXMas();
            }
            else if(args.Parameters[0].ToLower() == "false")
            {
                TPulse.Config.ForceXmas = false;
                Main.checkXMas();
            }
            else
            {
                args.Player.SendErrorMessage("Usage: /forcexmas [true/false]");
                return;
            }

            args.Player.SendInfoMessage(
                    String.Format("The server is currently {0} force Christmas mode.",
                                (TPulse.Config.ForceXmas ? "in" : "not in")));
        }

		#endregion Player Management Commands

		#region Server Maintenence Commands

		private void Broadcast(CommandArgs args)
		{
			string message = "";

			for (int i = 0; i < args.Parameters.Count; i++)
			{
				message += " " + args.Parameters[i];
			}

			TPulse.Utils.Broadcast("(Server Broadcast)" + message, Color.Red);
			return;
		}

		private void Off(CommandArgs args)
		{

			if (TPulse.Config.ServerSideInventory)
			{
				foreach (TPPlayer player in TPulse.Players)
				{
					if (player != null && player.IsLoggedIn && !player.IgnoreActionsForClearingTrashCan)
					{
						player.SaveServerInventory();
					}
				}
			}

			string reason = ((args.Parameters.Count > 0) ? "Server shutting down: " + String.Join(" ", args.Parameters) : "Server shutting down!");
			TPulse.Utils.StopServer(true, reason);
		}
		//Added restart command
		private void Restart(CommandArgs args)
		{
			if (Main.runningMono)
			{
				Log.ConsoleInfo("Sorry, this command has not yet been implemented in Mono.");
			}
			else
			{
				if (TPulse.Config.ServerSideInventory)
				{
					foreach (TPPlayer player in TPulse.Players)
					{
						if (player != null && player.IsLoggedIn && !player.IgnoreActionsForClearingTrashCan)
						{
							TPulse.InventoryDB.InsertPlayerData(player);
						}
					}
				}

				string reason = ((args.Parameters.Count > 0) ? "Server shutting down: " + String.Join(" ", args.Parameters) : "Server shutting down!");
				TPulse.Utils.StopServer(true, reason);
				System.Diagnostics.Process.Start(System.Reflection.Assembly.GetExecutingAssembly().GetName().CodeBase);
				Environment.Exit(0);
			}
		}

		private void OffNoSave(CommandArgs args)
		{
			string reason = ((args.Parameters.Count > 0) ? "Server shutting down: " + String.Join(" ", args.Parameters) : "Server shutting down!");
			TPulse.Utils.StopServer(false, reason);
		}

		private void CheckUpdates(CommandArgs args)
		{
            args.Player.SendInfoMessage("An update check has been queued.");
			//ThreadPool.QueueUserWorkItem(UpdateManager.CheckUpdate);
		}

		#endregion Server Maintenence Commands

        #region Cause Events and Spawn Monsters Commands

        private void DropMeteor(CommandArgs args)
		{
			WorldGen.spawnMeteor = false;
			WorldGen.dropMeteor();
            args.Player.SendInfoMessage("A meteor has been triggered.");
		}

		private void Star(CommandArgs args)
		{
			int penis56 = 12;
			int penis57 = Main.rand.Next(Main.maxTilesX - 50) + 100;
			penis57 *= 0x10;
			int penis58 = Main.rand.Next((int) (Main.maxTilesY*0.05))*0x10;
			Vector2 vector = new Vector2(penis57, penis58);
			float speedX = Main.rand.Next(-100, 0x65);
			float speedY = Main.rand.Next(200) + 100;
			float penis61 = (float) Math.Sqrt(((speedX*speedX) + (speedY*speedY)));
			penis61 = (penis56)/penis61;
			speedX *= penis61;
			speedY *= penis61;
			Projectile.NewProjectile(vector.X, vector.Y, speedX, speedY, 12, 0x3e8, 10f, Main.myPlayer);
            args.Player.SendInfoMessage("An attempt has been made to spawn a star.");
		}

		private void Fullmoon(CommandArgs args)
		{
			TPPlayer.Server.SetFullMoon(true);
			TPulse.Utils.Broadcast(string.Format("{0} turned on the full moon.", args.Player.Name), Color.Green);
		}

		private void Bloodmoon(CommandArgs args)
		{
			TPPlayer.Server.SetBloodMoon(true);
			TPulse.Utils.Broadcast(string.Format("{0} turned on the blood moon.", args.Player.Name), Color.Green);
		}

		private void Invade(CommandArgs args)
		{
			if (Main.invasionSize <= 0)
			{
				TPPlayer.All.SendInfoMessage(string.Format("{0} has started a goblin army invasion.", args.Player.Name));
				TPulse.StartInvasion();
			}
			else
			{
                TPPlayer.All.SendInfoMessage(string.Format("{0} has ended a goblin army invasion.", args.Player.Name));
				Main.invasionSize = 0;
			}
		}

        private void StartHardMode(CommandArgs args)
        {
            if (!TPulse.Config.DisableHardmode)
                WorldGen.StartHardmode();
            else
                args.Player.SendMessage("Hardmode is disabled via config.", Color.Red);
        }

        private void DisableHardMode(CommandArgs args)
        {
            Main.hardMode = false;
            args.Player.SendMessage("Hardmode is now disabled.", Color.Green);
        }

        [Obsolete("This specific command for spawning mobs will replaced soon.")]
        private void Eater(CommandArgs args)
        {
            if (args.Parameters.Count > 1)
            {
                args.Player.SendMessage("Invalid syntax! Proper syntax: /eater [amount]", Color.Red);
                return;
            }
            int amount = 1;
            if (args.Parameters.Count == 1 && !int.TryParse(args.Parameters[0], out amount))
            {
                args.Player.SendMessage("Invalid syntax! Proper syntax: /eater [amount]", Color.Red);
                return;
            }
            amount = Math.Min(amount, Main.maxNPCs);
            NPC eater = TPulse.Utils.GetNPCById(13);
            TPPlayer.Server.SpawnNPC(eater.type, eater.name, amount, args.Player.TileX, args.Player.TileY);
            TPulse.Utils.Broadcast(string.Format("{0} has spawned eater of worlds {1} times!", args.Player.Name, amount));
        }

        [Obsolete("This specific command for spawning mobs will replaced soon.")]
        private void Eye(CommandArgs args)
        {
            if (args.Parameters.Count > 1)
            {
                args.Player.SendMessage("Invalid syntax! Proper syntax: /eye [amount]", Color.Red);
                return;
            }
            int amount = 1;
            if (args.Parameters.Count == 1 && !int.TryParse(args.Parameters[0], out amount))
            {
                args.Player.SendMessage("Invalid syntax! Proper syntax: /eye [amount]", Color.Red);
                return;
            }
            amount = Math.Min(amount, Main.maxNPCs);
            NPC eye = TPulse.Utils.GetNPCById(4);
            TPPlayer.Server.SetTime(false, 0.0);
            TPPlayer.Server.SpawnNPC(eye.type, eye.name, amount, args.Player.TileX, args.Player.TileY);
            TPulse.Utils.Broadcast(string.Format("{0} has spawned eye {1} times!", args.Player.Name, amount));
        }

        [Obsolete("This specific command for spawning mobs will replaced soon.")]
        private void King(CommandArgs args)
        {
            if (args.Parameters.Count > 1)
            {
                args.Player.SendMessage("Invalid syntax! Proper syntax: /king [amount]", Color.Red);
                return;
            }
            int amount = 1;
            if (args.Parameters.Count == 1 && !int.TryParse(args.Parameters[0], out amount))
            {
                args.Player.SendMessage("Invalid syntax! Proper syntax: /king [amount]", Color.Red);
                return;
            }
            amount = Math.Min(amount, Main.maxNPCs);
            NPC king = TPulse.Utils.GetNPCById(50);
            TPPlayer.Server.SpawnNPC(king.type, king.name, amount, args.Player.TileX, args.Player.TileY);
            TPulse.Utils.Broadcast(string.Format("{0} has spawned king slime {1} times!", args.Player.Name, amount));
        }

        [Obsolete("This specific command for spawning mobs will replaced soon.")]
        private void Skeletron(CommandArgs args)
        {
            if (args.Parameters.Count > 1)
            {
                args.Player.SendMessage("Invalid syntax! Proper syntax: /skeletron [amount]", Color.Red);
                return;
            }
            int amount = 1;
            if (args.Parameters.Count == 1 && !int.TryParse(args.Parameters[0], out amount))
            {
                args.Player.SendMessage("Invalid syntax! Proper syntax: /skeletron [amount]", Color.Red);
                return;
            }
            amount = Math.Min(amount, Main.maxNPCs);
            NPC skeletron = TPulse.Utils.GetNPCById(35);
            TPPlayer.Server.SetTime(false, 0.0);
            TPPlayer.Server.SpawnNPC(skeletron.type, skeletron.name, amount, args.Player.TileX, args.Player.TileY);
            TPulse.Utils.Broadcast(string.Format("{0} has spawned skeletron {1} times!", args.Player.Name, amount));
        }

        [Obsolete("This specific command for spawning mobs will replaced soon.")]
        private void WoF(CommandArgs args)
        {
            if (Main.wof >= 0 || (args.Player.Y / 16f < (Main.maxTilesY - 205)))
            {
                args.Player.SendMessage("Can't spawn Wall of Flesh!", Color.Red);
                return;
            }
            NPC.SpawnWOF(new Vector2(args.Player.X, args.Player.Y));
            TPulse.Utils.Broadcast(string.Format("{0} has spawned Wall of Flesh!", args.Player.Name));
        }

        [Obsolete("This specific command for spawning mobs will replaced soon.")]
        private void Twins(CommandArgs args)
        {
            if (args.Parameters.Count > 1)
            {
                args.Player.SendMessage("Invalid syntax! Proper syntax: /twins [amount]", Color.Red);
                return;
            }
            int amount = 1;
            if (args.Parameters.Count == 1 && !int.TryParse(args.Parameters[0], out amount))
            {
                args.Player.SendMessage("Invalid syntax! Proper syntax: /twins [amount]", Color.Red);
                return;
            }
            amount = Math.Min(amount, Main.maxNPCs);
            NPC retinazer = TPulse.Utils.GetNPCById(125);
            NPC spaz = TPulse.Utils.GetNPCById(126);
            TPPlayer.Server.SetTime(false, 0.0);
            TPPlayer.Server.SpawnNPC(retinazer.type, retinazer.name, amount, args.Player.TileX, args.Player.TileY);
            TPPlayer.Server.SpawnNPC(spaz.type, spaz.name, amount, args.Player.TileX, args.Player.TileY);
            TPulse.Utils.Broadcast(string.Format("{0} has spawned the twins {1} times!", args.Player.Name, amount));
        }

        [Obsolete("This specific command for spawning mobs will replaced soon.")]
        private void Destroyer(CommandArgs args)
        {
            if (args.Parameters.Count > 1)
            {
                args.Player.SendMessage("Invalid syntax! Proper syntax: /destroyer [amount]", Color.Red);
                return;
            }
            int amount = 1;
            if (args.Parameters.Count == 1 && !int.TryParse(args.Parameters[0], out amount))
            {
                args.Player.SendMessage("Invalid syntax! Proper syntax: /destroyer [amount]", Color.Red);
                return;
            }
            amount = Math.Min(amount, Main.maxNPCs);
            NPC destroyer = TPulse.Utils.GetNPCById(134);
            TPPlayer.Server.SetTime(false, 0.0);
            TPPlayer.Server.SpawnNPC(destroyer.type, destroyer.name, amount, args.Player.TileX, args.Player.TileY);
            TPulse.Utils.Broadcast(string.Format("{0} has spawned the destroyer {1} times!", args.Player.Name, amount));
        }

        [Obsolete("This specific command for spawning mobs will replaced soon.")]
        private void SkeletronPrime(CommandArgs args)
        {
            if (args.Parameters.Count > 1)
            {
                args.Player.SendMessage("Invalid syntax! Proper syntax: /prime [amount]", Color.Red);
                return;
            }
            int amount = 1;
            if (args.Parameters.Count == 1 && !int.TryParse(args.Parameters[0], out amount))
            {
                args.Player.SendMessage("Invalid syntax! Proper syntax: /prime [amount]", Color.Red);
                return;
            }
            amount = Math.Min(amount, Main.maxNPCs);
            NPC prime = TPulse.Utils.GetNPCById(127);
            TPPlayer.Server.SetTime(false, 0.0);
            TPPlayer.Server.SpawnNPC(prime.type, prime.name, amount, args.Player.TileX, args.Player.TileY);
            TPulse.Utils.Broadcast(string.Format("{0} has spawned skeletron prime {1} times!", args.Player.Name, amount));
        }

        [Obsolete("This specific command for spawning mobs will replaced soon.")]
        private void Hardcore(CommandArgs args) // TODO: Add all 8 bosses
        {
            if (args.Parameters.Count > 1)
            {
                args.Player.SendMessage("Invalid syntax! Proper syntax: /hardcore [amount]", Color.Red);
                return;
            }
            int amount = 1;
            if (args.Parameters.Count == 1 && !int.TryParse(args.Parameters[0], out amount))
            {
                args.Player.SendMessage("Invalid syntax! Proper syntax: /hardcore [amount]", Color.Red);
                return;
            }
            amount = Math.Min(amount, Main.maxNPCs / 4);
            NPC retinazer = TPulse.Utils.GetNPCById(125);
            NPC spaz = TPulse.Utils.GetNPCById(126);
            NPC destroyer = TPulse.Utils.GetNPCById(134);
            NPC prime = TPulse.Utils.GetNPCById(127);
            NPC eater = TPulse.Utils.GetNPCById(13);
            NPC eye = TPulse.Utils.GetNPCById(4);
            NPC king = TPulse.Utils.GetNPCById(50);
            NPC skeletron = TPulse.Utils.GetNPCById(35);
            TPPlayer.Server.SetTime(false, 0.0);
            TPPlayer.Server.SpawnNPC(retinazer.type, retinazer.name, amount, args.Player.TileX, args.Player.TileY);
            TPPlayer.Server.SpawnNPC(spaz.type, spaz.name, amount, args.Player.TileX, args.Player.TileY);
            TPPlayer.Server.SpawnNPC(destroyer.type, destroyer.name, amount, args.Player.TileX, args.Player.TileY);
            TPPlayer.Server.SpawnNPC(prime.type, prime.name, amount, args.Player.TileX, args.Player.TileY);
            TPPlayer.Server.SpawnNPC(eater.type, eater.name, amount, args.Player.TileX, args.Player.TileY);
            TPPlayer.Server.SpawnNPC(eye.type, eye.name, amount, args.Player.TileX, args.Player.TileY);
            TPPlayer.Server.SpawnNPC(king.type, king.name, amount, args.Player.TileX, args.Player.TileY);
            TPPlayer.Server.SpawnNPC(skeletron.type, skeletron.name, amount, args.Player.TileX, args.Player.TileY);
            TPulse.Utils.Broadcast(string.Format("{0} has spawned all bosses {1} times!", args.Player.Name, amount));
        }

        private void SpawnMob(CommandArgs args)
        {
            if (args.Parameters.Count < 1 || args.Parameters.Count > 2)
            {
                args.Player.SendMessage("Invalid syntax! Proper syntax: /spawnmob <mob name/id> [amount]", Color.Red);
                return;
            }
            if (args.Parameters[0].Length == 0)
            {
                args.Player.SendMessage("Missing mob name/id", Color.Red);
                return;
            }
            int amount = 1;
            if (args.Parameters.Count == 2 && !int.TryParse(args.Parameters[1], out amount))
            {
                args.Player.SendMessage("Invalid syntax! Proper syntax: /spawnmob <mob name/id> [amount]", Color.Red);
                return;
            }

            amount = Math.Min(amount, Main.maxNPCs);

            var npcs = TPulse.Utils.GetNPCByIdOrName(args.Parameters[0]);
            if (npcs.Count == 0)
            {
                args.Player.SendMessage("Invalid mob type!", Color.Red);
            }
            else if (npcs.Count > 1)
            {
                args.Player.SendMessage(string.Format("More than one ({0}) mob matched!", npcs.Count), Color.Red);
            }
            else
            {
                var npc = npcs[0];
                if (npc.type >= 1 && npc.type < Main.maxNPCTypes && npc.type != 113)
                //Do not allow WoF to spawn, in certain conditions may cause loops in client
                {
                    TPPlayer.Server.SpawnNPC(npc.type, npc.name, amount, args.Player.TileX, args.Player.TileY, 50, 20);
                    TPPlayer.All.SendSuccessMessage(string.Format("{0} was spawned {1} time(s).", npc.name, amount));
                }
                else if (npc.type == 113)
                    args.Player.SendErrorMessage("Sorry, you can't spawn Wall of Flesh! Try /wof instead.");
                // Maybe perhaps do something with WorldGen.SpawnWoF?
                else
                    args.Player.SendMessage("Invalid mob type!", Color.Red);
            }
        }

		#endregion Cause Events and Spawn Monsters Commands

		#region Teleport Commands

		private void Home(CommandArgs args)
		{
			args.Player.Spawn();
			args.Player.SendSuccessMessage("Teleported to your spawnpoint.");
		}

		private void Spawn(CommandArgs args)
		{
			if (args.Player.Teleport(Main.spawnTileX, Main.spawnTileY))
				args.Player.SendSuccessMessage("Teleported to the map's spawnpoint.");
		}

		private void TP(CommandArgs args)
		{
			if (args.Parameters.Count < 1)
			{
				args.Player.SendErrorMessage("Invalid syntax! Proper syntax: /tp <player> ");
				return;
			}

			string plStr = String.Join(" ", args.Parameters);
			var players = TPulse.Utils.FindPlayer(plStr);
			if (players.Count == 0)
				args.Player.SendErrorMessage("Invalid player!");
			else if (players.Count > 1)
				args.Player.SendErrorMessage("More than one player matched!");
			else if (!players[0].TPAllow && !args.Player.Group.HasPermission(Permissions.tpall))
			{
				var plr = players[0];
				args.Player.SendErrorMessage(plr.Name + " has prevented users from teleporting to them.");
				plr.SendInfoMessage(args.Player.Name + " attempted to teleport to you.");
			}
			else
			{
				var plr = players[0];
				if (args.Player.Teleport(plr.TileX, plr.TileY + 3))
				{
					args.Player.SendSuccessMessage(string.Format("Teleported to {0}.", plr.Name));
					if (!args.Player.Group.HasPermission(Permissions.tphide))
						plr.SendInfoMessage(args.Player.Name + " teleported to you.");
				}
			}
		}

		private void TPHere(CommandArgs args)
		{
			if (args.Parameters.Count < 1)
			{
				args.Player.SendErrorMessage("Invalid syntax! Proper syntax: /tphere <player> ");
				return;
			}

			string plStr = String.Join(" ", args.Parameters);

			if (plStr == "all" || plStr == "*")
			{
				args.Player.SendInfoMessage(string.Format("You brought all players here."));
				for (int i = 0; i < Main.maxPlayers; i++)
				{
					if (Main.player[i].active && (Main.player[i] != args.TPlayer))
					{
						if (TPulse.Players[i].Teleport(args.Player.TileX, args.Player.TileY + 3))
							TPulse.Players[i].SendSuccessMessage(string.Format("You were teleported to {0}.", args.Player.Name) + ".");
					}
				}
				return;
			}

			var players = TPulse.Utils.FindPlayer(plStr);
			if (players.Count == 0)
			{
				args.Player.SendErrorMessage("Invalid player!");
			}
			else if (players.Count > 1)
			{
				args.Player.SendErrorMessage("More than one player matched!");
			}
			else
			{
				var plr = players[0];
				if (plr.Teleport(args.Player.TileX, args.Player.TileY + 3))
				{
					plr.SendInfoMessage(string.Format("You were teleported to {0}.", args.Player.Name));
					args.Player.SendSuccessMessage(string.Format("You brought {0} here.", plr.Name));
				}
			}
		}

		private void TPAllow(CommandArgs args)
		{
			if (!args.Player.TPAllow)
				args.Player.SendSuccessMessage("You have removed your teleportation protection.");
			if (args.Player.TPAllow)
                args.Player.SendSuccessMessage("You have enabled teleportation protection.");
			args.Player.TPAllow = !args.Player.TPAllow;
		}

        private void DeprecateWarp(CommandArgs args)
        {
            if (args.Player.Group.HasPermission(Permissions.managewarp))
            {
                args.Player.SendInfoMessage("Invalid syntax. Syntax: /warp [command] [arguments]");
                args.Player.SendInfoMessage("Commands: add, del, hide, list, send, [warpname]");
                args.Player.SendInfoMessage("Arguments: add [warp name], del [warp name], list [page]");
                args.Player.SendInfoMessage("Arguments: send [player] [warp name], hide [warp name] [Enable(true/false)]");
                args.Player.SendInfoMessage("Examples: /warp add foobar, /warp hide foobar true, /warp foobar");
            }
            else
            {
                args.Player.SendErrorMessage("Invalid syntax. Syntax: /warp [name] or /warp list <page>");
                args.Player.SendErrorMessage("Previous warps with spaces should be wrapped in single quotes.");
            }
        }

		private void Warp(CommandArgs args)
		{
		    bool hasManageWarpPermission = args.Player.Group.HasPermission(Permissions.managewarp);
            if (args.Parameters.Count < 1)
            {
                if (hasManageWarpPermission)
                {
                    args.Player.SendInfoMessage("Invalid syntax. Syntax: /warp [command] [arguments]");
                    args.Player.SendInfoMessage("Commands: add, del, hide, list, send, [warpname]");
                    args.Player.SendInfoMessage("Arguments: add [warp name], del [warp name], list [page]");
                    args.Player.SendInfoMessage("Arguments: send [player] [warp name], hide [warp name] [Enable(true/false)]");
                    args.Player.SendInfoMessage("Examples: /warp add foobar, /warp hide foobar true, /warp foobar");
                    return;
                }
                else
                {
                    args.Player.SendErrorMessage("Invalid syntax. Syntax: /warp [name] or /warp list <page>");
                    args.Player.SendErrorMessage("Previous warps with spaces should be wrapped in single quotes.");

                    return;
                }
            }

			if (args.Parameters[0].Equals("list"))
            {
                #region
                //How many warps per page
				const int pagelimit = 15;
				//How many warps per line
				const int perline = 5;
				//Pages start at 0 but are displayed and parsed at 1
				int page = 0;


				if (args.Parameters.Count > 1)
				{
					if (!int.TryParse(args.Parameters[1], out page) || page < 1)
					{
						args.Player.SendErrorMessage(string.Format("Invalid page number ({0})", page));
						return;
					}
					page--; //Substract 1 as pages are parsed starting at 1 and not 0
				}

				var warps = TPulse.Warps.ListAllPublicWarps(Main.worldID.ToString());

				//Check if they are trying to access a page that doesn't exist.
				int pagecount = warps.Count/pagelimit;
				if (page > pagecount)
				{
					args.Player.SendErrorMessage(string.Format("Page number exceeds pages ({0}/{1}).", page + 1, pagecount + 1));
					return;
				}

				//Display the current page and the number of pages.
				args.Player.SendSuccessMessage(string.Format("Current warps ({0}/{1}):", page + 1, pagecount + 1));

				//Add up to pagelimit names to a list
				var nameslist = new List<string>();
				for (int i = (page*pagelimit); (i < ((page*pagelimit) + pagelimit)) && i < warps.Count; i++)
				{
					nameslist.Add(warps[i].WarpName);
				}

				//convert the list to an array for joining
				var names = nameslist.ToArray();
				for (int i = 0; i < names.Length; i += perline)
				{
					args.Player.SendInfoMessage(string.Join(", ", names, i, Math.Min(names.Length - i, perline)));
				}

				if (page < pagecount)
				{
					args.Player.SendInfoMessage(string.Format("Type /warp list {0} for more warps.", (page + 2)));
                }
                #endregion
            }
            else if (args.Parameters[0].ToLower() == "add" && hasManageWarpPermission)
            {
                #region Add warp
                if (args.Parameters.Count == 2)
                {
                    string warpName = args.Parameters[1];
                    if (warpName == "list" || warpName == "hide" || warpName == "del" || warpName == "add")
                    {
                        args.Player.SendErrorMessage("Name reserved, use a different name.");
                    }
                    else if (TPulse.Warps.AddWarp(args.Player.TileX, args.Player.TileY, warpName, Main.worldID.ToString()))
                    {
                        args.Player.SendSuccessMessage("Warp added: " + warpName);
                    }
                    else
                    {
                        args.Player.SendErrorMessage("Warp " + warpName + " already exists.");
                    }
                }
                else
                    args.Player.SendErrorMessage("Invalid syntax! Proper syntax: /warp add [name]");
                #endregion

            }
            else if (args.Parameters[0].ToLower() == "del" && hasManageWarpPermission)
            {
                #region Del warp
                if (args.Parameters.Count == 2)
                {
                    string warpName = args.Parameters[1];
                    if (TPulse.Warps.RemoveWarp(warpName))
                        args.Player.SendSuccessMessage("Warp deleted: " + warpName);
                    else
                        args.Player.SendErrorMessage("Could not find the specified warp.");
                }
                else
                    args.Player.SendErrorMessage("Invalid syntax! Proper syntax: /warp del [name]");
                #endregion

            }
            else if (args.Parameters[0].ToLower() == "hide" && hasManageWarpPermission)
            {
                #region Hide warp
                if (args.Parameters.Count == 3)
                {
                    string warpName = args.Parameters[1];
                    bool state = false;
                    if (Boolean.TryParse(args.Parameters[2], out state))
                    {
                        if (TPulse.Warps.HideWarp(args.Parameters[1], state))
                        {
                            if (state)
                                args.Player.SendSuccessMessage("Warp " + warpName + " is now private.");
                            else
                                args.Player.SendSuccessMessage("Warp " + warpName + " is now public.");
                        }
                        else
                            args.Player.SendErrorMessage("Could not find specified warp.");
                    }
                    else
                        args.Player.SendErrorMessage("Invalid syntax! Proper syntax: /warp hide [name] <true/false>");
                }
                else
                    args.Player.SendErrorMessage("Invalid syntax! Proper syntax: /warp hide [name] <true/false>");
                #endregion
            }
            else if (args.Parameters[0].ToLower() == "send" && args.Player.Group.HasPermission(Permissions.tphere))
            {
                #region Warp send
                if (args.Parameters.Count < 3)
                {
                    args.Player.SendErrorMessage("Invalid syntax! Proper syntax: /warp send [player] [warpname]");
                    return;
                }

                var foundplr = TPulse.Utils.FindPlayer(args.Parameters[1]);
                if (foundplr.Count == 0)
                {
                    args.Player.SendErrorMessage("Invalid player!");
                    return;
                }
                else if (foundplr.Count > 1)
                {
                    args.Player.SendErrorMessage(string.Format("More than one ({0}) player matched!", args.Parameters.Count));
                    return;
                }
                string warpName = args.Parameters[2];
                var warp = TPulse.Warps.FindWarp(warpName);
                var plr = foundplr[0];
                if (warp.WarpPos != Vector2.Zero)
                {
                    if (plr.Teleport((int)warp.WarpPos.X, (int)warp.WarpPos.Y + 3))
                    {
                        plr.SendSuccessMessage(string.Format("{0} warped you to {1}.", args.Player.Name, warpName));
                        args.Player.SendSuccessMessage(string.Format("You warped {0} to {1}.", plr.Name, warpName));
                    }
                }
                else
                {
                    args.Player.SendErrorMessage("Specified warp not found.");
                }
                #endregion

            }
            else
            {
                string warpName = String.Join(" ", args.Parameters);
                var warp = TPulse.Warps.FindWarp(warpName);
                if (warp.WarpPos != Vector2.Zero)
                {
                    if (args.Player.Teleport((int)warp.WarpPos.X, (int)warp.WarpPos.Y + 3))
                        args.Player.SendSuccessMessage("Warped to " + warpName + ".");
                }
                else
                {
                    args.Player.SendErrorMessage("The specified warp was not found.");
                }
            }
		}

		#endregion Teleport Commands

		#region Group Management

		private void AddGroup(CommandArgs args)
		{
			if (args.Parameters.Count > 0)
			{
				String groupname = args.Parameters[0];
				args.Parameters.RemoveAt(0);
				String permissions = String.Join(",", args.Parameters);

				String response = TPulse.Groups.AddGroup(groupname, permissions);
				if (response.Length > 0)
					args.Player.SendSuccessMessage(response);
			}
			else
			{
				args.Player.SendErrorMessage("Incorrect format: /addgroup <group name> [optional permissions]");
			}
		}

		private void DeleteGroup(CommandArgs args)
		{
			if (args.Parameters.Count > 0)
			{
				String groupname = args.Parameters[0];

				String response = TPulse.Groups.DeleteGroup(groupname);
				if (response.Length > 0)
					args.Player.SendSuccessMessage(response);
			}
			else
			{
				args.Player.SendErrorMessage("Incorrect format: /delgroup <group name>");
			}
		}

		private void ModifyGroup(CommandArgs args)
		{
			if (args.Parameters.Count > 2)
			{
				String com = args.Parameters[0];
				args.Parameters.RemoveAt(0);

				String groupname = args.Parameters[0];
				args.Parameters.RemoveAt(0);

				string response = "";
				if (com.Equals("add"))
				{
					if( groupname == "*" )
					{
						int count = 0;
						foreach( Group g in TPulse.Groups )
						{
							response = TPulse.Groups.AddPermissions(g.Name, args.Parameters);
							if (!response.StartsWith("Error:"))
								count++;
						}
						args.Player.SendSuccessMessage(String.Format("{0} groups were modified.", count ));
						return;
					}
					response = TPulse.Groups.AddPermissions(groupname, args.Parameters);
					if (response.Length > 0)
						args.Player.SendSuccessMessage(response);
					return;
				}
				
				if (com.Equals("del") || com.Equals("delete"))
				{
					if (groupname == "*")
					{
						int count = 0;
						foreach (Group g in TPulse.Groups)
						{
							response = TPulse.Groups.DeletePermissions(g.Name, args.Parameters);
							if (!response.StartsWith("Error:"))
								count++;
						}
						args.Player.SendSuccessMessage(String.Format("{0} groups were modified.", count));
						return;
					}
					response = TPulse.Groups.DeletePermissions(groupname, args.Parameters);
					if (response.Length > 0)
						args.Player.SendSuccessMessage(response);
					return;
				}
			}
			args.Player.SendErrorMessage("Incorrect format: /modgroup add|del <group name> <permission to add or remove>");
		}

		private void ViewGroups(CommandArgs args)
		{
			if (args.Parameters.Count > 0)
			{
				String com = args.Parameters[0];

				if( com == "list" )
				{
					string ret = "Groups: ";
					foreach( Group g in TPulse.Groups.groups )
					{
						if (ret.Length > 50)
						{
							args.Player.SendSuccessMessage(ret);
							ret = "";
						}

						if( ret != "" )
						{
							ret += ", ";
						}
						
						ret += g.Name;
					}

					if (ret.Length > 0)
					{
						args.Player.SendSuccessMessage(ret);
					}
					return;
				}
				else if( com == "perm")
				{
					if (args.Parameters.Count > 1)
					{
						String groupname = args.Parameters[1];

						if( TPulse.Groups.GroupExists( groupname ) )
						{
							string ret = String.Format("Permissions for {0}: ", groupname);
							foreach (string p in TPulse.Utils.GetGroup( groupname ).permissions)
							{
								if (ret.Length > 50)
								{
									args.Player.SendSuccessMessage(ret);
									ret = "";
								}

								if (ret != "")
								{
									ret += ", ";
								}

								ret += p;
							}
							if (ret.Length > 0)
							{
								args.Player.SendSuccessMessage(ret);
							}

							return;
						}
						else
						{
							args.Player.SendErrorMessage("Group does not exist.");
							return;
						}
					}
				}
			}
			args.Player.SendErrorMessage("Incorrect format: /group list");
			args.Player.SendErrorMessage("                  /group perm <group name>");
		}

		#endregion Group Management

		#region Item Management

		private void AddItem(CommandArgs args)
		{
			if (args.Parameters.Count == 1)
			{
				var items = TPulse.Utils.GetItemByIdOrName(args.Parameters[0]);
				if (items.Count == 0)
				{
					args.Player.SendErrorMessage("Invalid item type!");
				}
				else if (items.Count > 1)
				{
					args.Player.SendErrorMessage(string.Format("More than one ({0}) item matched!", items.Count));
				}
				else
				{
					var item = items[0];
					if (item.type >= 1)
					{
						TPulse.Itembans.AddNewBan(item.name);
						args.Player.SendErrorMessage(item.name + " has been banned.");
					}
					else
					{
						args.Player.SendErrorMessage("Invalid item type!");
					}
				}
			}
			else
			{
				args.Player.SendErrorMessage("Invalid use: /additem \"item name\" or /additem ##.");
			}
		}

		private void DeleteItem(CommandArgs args)
		{
			if (args.Parameters.Count == 1)
			{
				var items = TPulse.Utils.GetItemByIdOrName(args.Parameters[0]);
				if (items.Count == 0)
				{
					args.Player.SendErrorMessage("Invalid item type!");
				}
				else if (items.Count > 1)
				{
					args.Player.SendErrorMessage(string.Format("More than one ({0}) item matched!", items.Count));
				}
				else
				{
					var item = items[0];
					if (item.type >= 1)
					{
						TPulse.Itembans.RemoveBan(item.name);
						args.Player.SendSuccessMessage(item.name + " has been unbanned.");
					}
					else
					{
						args.Player.SendErrorMessage("Invalid item type!");
					}
				}
			}
			else
			{
				args.Player.SendErrorMessage("Invalid use: /delitem \"item name\" or /delitem ##");
			}
		}
		
		private void ListItems(CommandArgs args)
		{
			args.Player.SendInfoMessage("The banned items are: " + String.Join(",", TPulse.Itembans.ItemBans) + ".");
		}
		
		private void AddItemGroup(CommandArgs args)
		{
			if (args.Parameters.Count == 2)
			{
				var items = TPulse.Utils.GetItemByIdOrName(args.Parameters[0]);
				if (items.Count == 0)
				{
					args.Player.SendErrorMessage("Invalid item type!");
				}
				else if (items.Count > 1)
				{
					args.Player.SendErrorMessage(string.Format("More than one ({0}) item matched!", items.Count));
				}
				else
				{
					var item = items[0];
					if (item.type >= 1)
					{
						if(TPulse.Groups.GroupExists(args.Parameters[1]))
						{
							ItemBan ban = TPulse.Itembans.GetItemBanByName(item.name);
							
							if(!ban.AllowedGroups.Contains(args.Parameters[1]))
							{
								TPulse.Itembans.AllowGroup(item.name, args.Parameters[1]);
								args.Player.SendSuccessMessage("Banned item " + item.name + " has been allowed for group " + args.Parameters[1] + ".");
							}
							else
							{
								args.Player.SendWarningMessage("Banned item " + item.name + " is already allowed for group " + args.Parameters[1] + "!");	
							}
						}
						else
						{
							args.Player.SendErrorMessage("Group " + args.Parameters[1] + " not found!");
						}
					}
					else
					{
						args.Player.SendErrorMessage("Invalid item type!");
					}
				}
			}
			else
			{
				args.Player.SendErrorMessage("Invalid use: /additemgroup \"item name\" \"group name\"");
			}
		}		

		private void DeleteItemGroup(CommandArgs args)
		{
			if (args.Parameters.Count == 2)
			{
				var items = TPulse.Utils.GetItemByIdOrName(args.Parameters[0]);
				if (items.Count == 0)
				{
					args.Player.SendErrorMessage("Invalid item type!");
				}
				else if (items.Count > 1)
				{
					args.Player.SendErrorMessage(string.Format("More than one ({0}) item matched!", items.Count));
				}
				else
				{
					var item = items[0];
					if (item.type >= 1)
					{
						if(TPulse.Groups.GroupExists(args.Parameters[1]))
						{
							ItemBan ban = TPulse.Itembans.GetItemBanByName(item.name);
							
							if(ban.AllowedGroups.Contains(args.Parameters[1]))
							{
								TPulse.Itembans.RemoveGroup(item.name, args.Parameters[1]);
								args.Player.SendSuccessMessage("Removed access for group " + args.Parameters[1] + " to banned item " + item.name + ".");
							}
							else
							{
								args.Player.SendWarningMessage("Group " + args.Parameters[1] + " did not have access to banned item " + item.name + "!");	
							}
						}
						else
						{
							args.Player.SendErrorMessage("Group " + args.Parameters[1] + " not found!");
						}
					}
					else
					{
						args.Player.SendErrorMessage("Invalid item type!");
					}
				}
			}
			else
			{
				args.Player.SendErrorMessage("Invalid use: /delitemgroup \"item name\" \"group name\"");
			}
		}
		
		#endregion Item Management

		#region Server Config Commands

		private void SetSpawn(CommandArgs args)
		{
			Main.spawnTileX = args.Player.TileX + 1;
			Main.spawnTileY = args.Player.TileY + 3;
			SaveManager.Instance.SaveWorld(false);
			args.Player.SendSuccessMessage("Spawn has now been set at your location.");
		}

		private void Reload(CommandArgs args)
		{
			FileTools.SetupConfig();
			TPulse.HandleCommandLinePostConfigLoad(Environment.GetCommandLineArgs());
			TPulse.Groups.LoadPermisions();
			//todo: Create an event for reloads to propegate to plugins.
            TPulse.Regions.ReloadAllRegions();
			args.Player.SendSuccessMessage(
				"Configuration, permissions, and regions reload complete. Some changes may require a server restart.");
		}

		private void ServerPassword(CommandArgs args)
		{
			if (args.Parameters.Count != 1)
			{
				args.Player.SendErrorMessage("Invalid syntax! Proper syntax: /password \"<new password>\"");
				return;
			}
			string passwd = args.Parameters[0];
			TPulse.Config.ServerPassword = passwd;
			args.Player.SendSuccessMessage(string.Format("Server password has been changed to: {0}.", passwd));
		}

		private void Save(CommandArgs args)
		{
			SaveManager.Instance.SaveWorld(false);
			foreach (TPPlayer tsply in TPulse.Players.Where(tsply => tsply != null))
			{
				tsply.SaveServerInventory();
			}
			args.Player.SendSuccessMessage("Save succeeded.");
		}

		private void Settle(CommandArgs args)
		{
			if (Liquid.panicMode)
			{
				args.Player.SendWarningMessage("Liquids are already settling!");
				return;
			}
			Liquid.StartPanic();
			args.Player.SendInfoMessage("Settling liquids.");
		}

		private void MaxSpawns(CommandArgs args)
		{
			if (args.Parameters.Count != 1)
			{
				args.Player.SendErrorMessage("Invalid syntax! Proper syntax: /maxspawns <maxspawns>");
				args.Player.SendErrorMessage("Proper syntax: /maxspawns show");
				args.Player.SendErrorMessage("Proper syntax: /maxspawns default");
				return;
			}

			if (args.Parameters[0] == "show")
			{
				args.Player.SendInfoMessage("Current maximum spawns is " + TPulse.Config.DefaultMaximumSpawns + ".");
				return;
			}
			
			if(args.Parameters[0]=="default"){
				TPulse.Config.DefaultMaximumSpawns = 5;
				NPC.defaultMaxSpawns = 5;
				TPPlayer.All.SendInfoMessage(string.Format("{0} changed the maximum spawns to 5.", args.Player.Name));
				return;
			}

			int amount = Convert.ToInt32(args.Parameters[0]);
			int.TryParse(args.Parameters[0], out amount);
			NPC.defaultMaxSpawns = amount;
			TPulse.Config.DefaultMaximumSpawns = amount;
			TPPlayer.All.SendInfoMessage(string.Format("{0} changed the maximum spawns to {1}.", args.Player.Name, amount));
		}

		private void SpawnRate(CommandArgs args)
		{
			if (args.Parameters.Count != 1)
			{
				args.Player.SendErrorMessage("Invalid syntax! Proper syntax: /spawnrate <spawnrate>");
				args.Player.SendErrorMessage("/spawnrate show");
				args.Player.SendErrorMessage("/spawnrate default");
				return;
			}

			if (args.Parameters[0] == "show")
			{
				args.Player.SendInfoMessage("Current spawn rate is " + TPulse.Config.DefaultSpawnRate + ".");
				return;
			}

			if (args.Parameters[0] == "default")
			{
				TPulse.Config.DefaultSpawnRate = 600;
				NPC.defaultSpawnRate = 600;
				TPPlayer.All.SendInfoMessage(string.Format("{0} changed the spawn rate to 600.", args.Player.Name));
				return;
			}

			int amount = -1;
			if (!int.TryParse(args.Parameters[0], out amount))
			{
				args.Player.SendWarningMessage(string.Format("Invalid spawnrate ({0})", args.Parameters[0]));
				return;
			}

			if (amount < 0)
			{
				args.Player.SendWarningMessage("Spawnrate cannot be negative!");
				return;
			}

			NPC.defaultSpawnRate = amount;
			TPulse.Config.DefaultSpawnRate = amount;
			TPPlayer.All.SendInfoMessage(string.Format("{0} changed the spawn rate to {1}.", args.Player.Name, amount));
		}

		#endregion Server Config Commands

		#region Time/PvpFun Commands

		private void Time(CommandArgs args)
		{
			if (args.Parameters.Count != 1)
			{
				args.Player.SendErrorMessage("Invalid syntax! Proper syntax: /time <day/night/dusk/noon/midnight>");
				return;
			}

			switch (args.Parameters[0])
			{
				case "day":
					TPPlayer.Server.SetTime(true, 150.0);
					TPPlayer.All.SendInfoMessage(string.Format("{0} set the time to day.", args.Player.Name));
					break;
				case "night":
					TPPlayer.Server.SetTime(false, 0.0);
					TPPlayer.All.SendInfoMessage(string.Format("{0} set the time to night.", args.Player.Name));
					break;
				case "dusk":
					TPPlayer.Server.SetTime(false, 0.0);
					TPPlayer.All.SendInfoMessage(string.Format("{0} set the time to dusk.", args.Player.Name));
					break;
				case "noon":
					TPPlayer.Server.SetTime(true, 27000.0);
					TPPlayer.All.SendInfoMessage(string.Format("{0} set the time to noon.", args.Player.Name));
					break;
				case "midnight":
					TPPlayer.Server.SetTime(false, 16200.0);
					TPPlayer.All.SendInfoMessage(string.Format("{0} set the time to midnight.", args.Player.Name));
					break;
				default:
					args.Player.SendErrorMessage("Invalid syntax! Proper syntax: /time <day/night/dusk/noon/midnight>");
					break;
			}
		}

        //TODO: Come back here

		private void Slap(CommandArgs args)
		{
			if (args.Parameters.Count < 1 || args.Parameters.Count > 2)
			{
				args.Player.SendErrorMessage("Invalid syntax! Proper syntax: /slap <player> [dmg]");
				return;
			}
			if (args.Parameters[0].Length == 0)
			{
				args.Player.SendErrorMessage("Missing player name.");
				return;
			}

			string plStr = args.Parameters[0];
			var players = TPulse.Utils.FindPlayer(plStr);
			if (players.Count == 0)
			{
				args.Player.SendErrorMessage("Invalid player!");
			}
			else if (players.Count > 1)
			{
				args.Player.SendErrorMessage("More than one player matched!");
			}
			else
			{
				var plr = players[0];
				int damage = 5;
				if (args.Parameters.Count == 2)
				{
					int.TryParse(args.Parameters[1], out damage);
				}
				if (!args.Player.Group.HasPermission(Permissions.kill))
				{
					damage = TPulse.Utils.Clamp(damage, 15, 0);
				}
				plr.DamagePlayer(damage);
				TPPlayer.All.SendSuccessMessage(string.Format("{0} slapped {1} for {2} damage.",
													 args.Player.Name, plr.Name, damage));
				Log.Info(args.Player.Name + " slapped " + plr.Name + " for " + damage + " damage.");
			}
		}

		#endregion Time/PvpFun Commands

        #region Region Commands

        private void DebugRegions(CommandArgs args)
        {
            foreach (Region r in TPulse.Regions.Regions)
            {
                args.Player.SendMessage(r.Name + ": P: " + r.DisableBuild + " X: " + r.Area.X + " Y: " + r.Area.Y + " W: " +
                                        r.Area.Width + " H: " + r.Area.Height);
                foreach (int s in r.AllowedIDs)
                {
                    args.Player.SendMessage(r.Name + ": " + s);
                }
            }
        }

        private void Region(CommandArgs args)
        {
            string cmd = "help";
            if (args.Parameters.Count > 0)
            {
                cmd = args.Parameters[0].ToLower();
            }
            switch (cmd)
            {
                case "name":
                    {
                        {
                            args.Player.SendMessage("Hit a block to get the name of the region", Color.Yellow);
                            args.Player.AwaitingName = true;
                        }
                        break;
                    }
                case "set":
                    {
                        int choice = 0;
                        if (args.Parameters.Count == 2 &&
                            int.TryParse(args.Parameters[1], out choice) &&
                            choice >= 1 && choice <= 2)
                        {
                            args.Player.SendMessage("Hit a block to Set Point " + choice, Color.Yellow);
                            args.Player.AwaitingTempPoint = choice;
                        }
                        else
                        {
                            args.Player.SendMessage("Invalid syntax! Proper syntax: /region set [1/2]", Color.Red);
                        }
                        break;
                    }
                case "define":
                    {
                        if (args.Parameters.Count > 1)
                        {
                            if (!args.Player.TempPoints.Any(p => p == Point.Zero))
                            {
                                string regionName = String.Join(" ", args.Parameters.GetRange(1, args.Parameters.Count - 1));
                                var x = Math.Min(args.Player.TempPoints[0].X, args.Player.TempPoints[1].X);
                                var y = Math.Min(args.Player.TempPoints[0].Y, args.Player.TempPoints[1].Y);
                                var width = Math.Abs(args.Player.TempPoints[0].X - args.Player.TempPoints[1].X);
                                var height = Math.Abs(args.Player.TempPoints[0].Y - args.Player.TempPoints[1].Y);

                                if (TPulse.Regions.AddRegion(x, y, width, height, regionName, args.Player.UserAccountName,
                                                             Main.worldID.ToString()))
                                {
                                    args.Player.TempPoints[0] = Point.Zero;
                                    args.Player.TempPoints[1] = Point.Zero;
                                    args.Player.SendMessage("Set region " + regionName, Color.Yellow);
                                }
                                else
                                {
                                    args.Player.SendMessage("Region " + regionName + " already exists", Color.Red);
                                }
                            }
                            else
                            {
                                args.Player.SendMessage("Points not set up yet", Color.Red);
                            }
                        }
                        else
                            args.Player.SendMessage("Invalid syntax! Proper syntax: /region define [name]", Color.Red);
                        break;
                    }
                case "protect":
                    {
                        if (args.Parameters.Count == 3)
                        {
                            string regionName = args.Parameters[1];
                            if (args.Parameters[2].ToLower() == "true")
                            {
                                if (TPulse.Regions.SetRegionState(regionName, true))
                                    args.Player.SendMessage("Protected region " + regionName, Color.Yellow);
                                else
                                    args.Player.SendMessage("Could not find specified region", Color.Red);
                            }
                            else if (args.Parameters[2].ToLower() == "false")
                            {
                                if (TPulse.Regions.SetRegionState(regionName, false))
                                    args.Player.SendMessage("Unprotected region " + regionName, Color.Yellow);
                                else
                                    args.Player.SendMessage("Could not find specified region", Color.Red);
                            }
                            else
                                args.Player.SendMessage("Invalid syntax! Proper syntax: /region protect [name] [true/false]", Color.Red);
                        }
                        else
                            args.Player.SendMessage("Invalid syntax! Proper syntax: /region protect [name] [true/false]", Color.Red);
                        break;
                    }
                case "delete":
                    {
                        if (args.Parameters.Count > 1)
                        {
                            string regionName = String.Join(" ", args.Parameters.GetRange(1, args.Parameters.Count - 1));
                            if (TPulse.Regions.DeleteRegion(regionName))
                                args.Player.SendMessage("Deleted region " + regionName, Color.Yellow);
                            else
                                args.Player.SendMessage("Could not find specified region", Color.Red);
                        }
                        else
                            args.Player.SendMessage("Invalid syntax! Proper syntax: /region delete [name]", Color.Red);
                        break;
                    }
                case "clear":
                    {
                        args.Player.TempPoints[0] = Point.Zero;
                        args.Player.TempPoints[1] = Point.Zero;
                        args.Player.SendMessage("Cleared temp area", Color.Yellow);
                        args.Player.AwaitingTempPoint = 0;
                        break;
                    }
                case "allow":
                    {
                        if (args.Parameters.Count > 2)
                        {
                            string playerName = args.Parameters[1];
                            string regionName = "";

                            for (int i = 2; i < args.Parameters.Count; i++)
                            {
                                if (regionName == "")
                                {
                                    regionName = args.Parameters[2];
                                }
                                else
                                {
                                    regionName = regionName + " " + args.Parameters[i];
                                }
                            }
                            if (TPulse.Users.GetUserByName(playerName) != null)
                            {
                                if (TPulse.Regions.AddNewUser(regionName, playerName))
                                {
                                    args.Player.SendMessage("Added user " + playerName + " to " + regionName, Color.Yellow);
                                }
                                else
                                    args.Player.SendMessage("Region " + regionName + " not found", Color.Red);
                            }
                            else
                            {
                                args.Player.SendMessage("Player " + playerName + " not found", Color.Red);
                            }
                        }
                        else
                            args.Player.SendMessage("Invalid syntax! Proper syntax: /region allow [name] [region]", Color.Red);
                        break;
                    }
                case "remove":
                    if (args.Parameters.Count > 2)
                    {
                        string playerName = args.Parameters[1];
                        string regionName = "";

                        for (int i = 2; i < args.Parameters.Count; i++)
                        {
                            if (regionName == "")
                            {
                                regionName = args.Parameters[2];
                            }
                            else
                            {
                                regionName = regionName + " " + args.Parameters[i];
                            }
                        }
                        if (TPulse.Users.GetUserByName(playerName) != null)
                        {
                            if (TPulse.Regions.RemoveUser(regionName, playerName))
                            {
                                args.Player.SendMessage("Removed user " + playerName + " from " + regionName, Color.Yellow);
                            }
                            else
                                args.Player.SendMessage("Region " + regionName + " not found", Color.Red);
                        }
                        else
                        {
                            args.Player.SendMessage("Player " + playerName + " not found", Color.Red);
                        }
                    }
                    else
                        args.Player.SendMessage("Invalid syntax! Proper syntax: /region remove [name] [region]", Color.Red);
                    break;
                case "allowg":
                    {
                        if (args.Parameters.Count > 2)
                        {
                            string group = args.Parameters[1];
                            string regionName = "";

                            for (int i = 2; i < args.Parameters.Count; i++)
                            {
                                if (regionName == "")
                                {
                                    regionName = args.Parameters[2];
                                }
                                else
                                {
                                    regionName = regionName + " " + args.Parameters[i];
                                }
                            }
                            if (TPulse.Groups.GroupExists(group))
                            {
                                if (TPulse.Regions.AllowGroup(regionName, group))
                                {
                                    args.Player.SendMessage("Added group " + group + " to " + regionName, Color.Yellow);
                                }
                                else
                                    args.Player.SendMessage("Region " + regionName + " not found", Color.Red);
                            }
                            else
                            {
                                args.Player.SendMessage("Group " + group + " not found", Color.Red);
                            }
                        }
                        else
                            args.Player.SendMessage("Invalid syntax! Proper syntax: /region allow [group] [region]", Color.Red);
                        break;
                    }
                case "removeg":
                    if (args.Parameters.Count > 2)
                    {
                        string group = args.Parameters[1];
                        string regionName = "";

                        for (int i = 2; i < args.Parameters.Count; i++)
                        {
                            if (regionName == "")
                            {
                                regionName = args.Parameters[2];
                            }
                            else
                            {
                                regionName = regionName + " " + args.Parameters[i];
                            }
                        }
                        if (TPulse.Groups.GroupExists(group))
                        {
                            if (TPulse.Regions.RemoveGroup(regionName, group))
                            {
                                args.Player.SendMessage("Removed group " + group + " from " + regionName, Color.Yellow);
                            }
                            else
                                args.Player.SendMessage("Region " + regionName + " not found", Color.Red);
                        }
                        else
                        {
                            args.Player.SendMessage("Group " + group + " not found", Color.Red);
                        }
                    }
                    else
                        args.Player.SendMessage("Invalid syntax! Proper syntax: /region removeg [group] [region]", Color.Red);
                    break;
                case "list":
                    {
                        //How many regions per page
                        const int pagelimit = 15;
                        //How many regions per line
                        const int perline = 5;
                        //Pages start at 0 but are displayed and parsed at 1
                        int page = 0;


                        if (args.Parameters.Count > 1)
                        {
                            if (!int.TryParse(args.Parameters[1], out page) || page < 1)
                            {
                                args.Player.SendMessage(string.Format("Invalid page number ({0})", page), Color.Red);
                                return;
                            }
                            page--; //Substract 1 as pages are parsed starting at 1 and not 0
                        }

                        var regions = TPulse.Regions.ListAllRegions(Main.worldID.ToString());

                        // Are there even any regions to display?
                        if (regions.Count == 0)
                        {
                            args.Player.SendMessage("There are currently no regions defined.", Color.Red);
                            return;
                        }

                        //Check if they are trying to access a page that doesn't exist.
                        int pagecount = regions.Count / pagelimit;
                        if (page > pagecount)
                        {
                            args.Player.SendMessage(string.Format("Page number exceeds pages ({0}/{1})", page + 1, pagecount + 1), Color.Red);
                            return;
                        }

                        //Display the current page and the number of pages.
                        args.Player.SendMessage(string.Format("Current Regions ({0}/{1}):", page + 1, pagecount + 1), Color.Green);

                        //Add up to pagelimit names to a list
                        var nameslist = new List<string>();
                        for (int i = (page * pagelimit); (i < ((page * pagelimit) + pagelimit)) && i < regions.Count; i++)
                        {
                            nameslist.Add(regions[i].Name);
                        }

                        //convert the list to an array for joining
                        var names = nameslist.ToArray();
                        for (int i = 0; i < names.Length; i += perline)
                        {
                            args.Player.SendMessage(string.Join(", ", names, i, Math.Min(names.Length - i, perline)), Color.Yellow);
                        }

                        if (page < pagecount)
                        {
                            args.Player.SendMessage(string.Format("Type /region list {0} for more regions.", (page + 2)), Color.Yellow);
                        }

                        break;
                    }
                case "info":
                    {
                        if (args.Parameters.Count > 1)
                        {
                            string regionName = String.Join(" ", args.Parameters.GetRange(1, args.Parameters.Count - 1));
                            Region r = TPulse.Regions.GetRegionByName(regionName);

                            if (r == null)
                            {
                                args.Player.SendMessage("Region {0} does not exist");
                                break;
                            }

                            args.Player.SendMessage(r.Name + ": P: " + r.DisableBuild + " X: " + r.Area.X + " Y: " + r.Area.Y + " W: " +
                                                    r.Area.Width + " H: " + r.Area.Height);
                            foreach (int s in r.AllowedIDs)
                            {
                                var user = TPulse.Users.GetUserByID(s);
                                args.Player.SendMessage(r.Name + ": " + (user != null ? user.Name : "Unknown"));
                            }
                        }
                        else
                        {
                            args.Player.SendMessage("Invalid syntax! Proper syntax: /region info [name]", Color.Red);
                        }

                        break;
                    }
                case "z":
                    {
                        if (args.Parameters.Count == 3)
                        {
                            string regionName = args.Parameters[1];
                            int z = 0;
                            if (int.TryParse(args.Parameters[2], out z))
                            {
                                if (TPulse.Regions.SetZ(regionName, z))
                                    args.Player.SendMessage("Region's z is now " + z, Color.Yellow);
                                else
                                    args.Player.SendMessage("Could not find specified region", Color.Red);
                            }
                            else
                                args.Player.SendMessage("Invalid syntax! Proper syntax: /region z [name] [#]", Color.Red);
                        }
                        else
                            args.Player.SendMessage("Invalid syntax! Proper syntax: /region z [name] [#]", Color.Red);
                        break;
                    }
                case "resize":
                case "expand":
                    {
                        if (args.Parameters.Count == 4)
                        {
                            int direction;
                            switch (args.Parameters[2])
                            {
                                case "u":
                                case "up":
                                    {
                                        direction = 0;
                                        break;
                                    }
                                case "r":
                                case "right":
                                    {
                                        direction = 1;
                                        break;
                                    }
                                case "d":
                                case "down":
                                    {
                                        direction = 2;
                                        break;
                                    }
                                case "l":
                                case "left":
                                    {
                                        direction = 3;
                                        break;
                                    }
                                default:
                                    {
                                        direction = -1;
                                        break;
                                    }
                            }
                            int addAmount;
                            int.TryParse(args.Parameters[3], out addAmount);
                            if (TPulse.Regions.resizeRegion(args.Parameters[1], addAmount, direction))
                            {
                                args.Player.SendMessage("Region Resized Successfully!", Color.Yellow);
                                TPulse.Regions.ReloadAllRegions();
                            }
                            else
                            {
                                args.Player.SendMessage("Invalid syntax! Proper syntax: /region resize [regionname] [u/d/l/r] [amount]",
                                                        Color.Red);
                            }
                        }
                        else
                        {
                            args.Player.SendMessage("Invalid syntax! Proper syntax: /region resize [regionname] [u/d/l/r] [amount]1",
                                                    Color.Red);
                        }
                        break;
                    }
                case "help":
                default:
                    {
                        args.Player.SendMessage("Avialable region commands:", Color.Green);
                        args.Player.SendMessage("/region set [1/2] /region define [name] /region protect [name] [true/false]",
                                                Color.Yellow);
                        args.Player.SendMessage("/region name (provides region name)", Color.Yellow);
                        args.Player.SendMessage("/region delete [name] /region clear (temporary region)", Color.Yellow);
                        args.Player.SendMessage("/region allow [name] [regionname]", Color.Yellow);
                        args.Player.SendMessage("/region resize [regionname] [u/d/l/r] [amount]", Color.Yellow);
                        break;
                    }
            }
        }

        #endregion Region Commands

        #region World Protection Commands

        private void ToggleAntiBuild(CommandArgs args)
		{
			TPulse.Config.DisableBuild = (TPulse.Config.DisableBuild == false);
			TPPlayer.All.SendSuccessMessage(string.Format("Anti-build is now {0}.", (TPulse.Config.DisableBuild ? "on" : "off")));
		}

		private void ProtectSpawn(CommandArgs args)
		{
			TPulse.Config.SpawnProtection = (TPulse.Config.SpawnProtection == false);
			TPPlayer.All.SendSuccessMessage(string.Format("Spawn is now {0}.", (TPulse.Config.SpawnProtection ? "protected" : "open")));
		}

		#endregion World Protection Commands

		#region General Commands

		private void Help(CommandArgs args)
		{
			args.Player.SendInfoMessage("TPulse Commands:");
			int page = 1;
			if (args.Parameters.Count > 0)
				int.TryParse(args.Parameters[0], out page);
			var cmdlist = new List<Command>();
			for (int j = 0; j < ChatCommands.Count; j++)
			{
				if (ChatCommands[j].CanRun(args.Player))
				{
					cmdlist.Add(ChatCommands[j]);
				}
			}
			var sb = new StringBuilder();
			if (cmdlist.Count > (15*(page - 1)))
			{
				for (int j = (15*(page - 1)); j < (15*page); j++)
				{
					if (sb.Length != 0)
						sb.Append(", ");
					sb.Append("/").Append(cmdlist[j].Name);
					if (j == cmdlist.Count - 1)
					{
						args.Player.SendInfoMessage(sb.ToString());
						break;
					}
					if ((j + 1)%5 == 0)
					{
						args.Player.SendInfoMessage(sb.ToString());
						sb.Clear();
					}
				}
			}
			if (cmdlist.Count > (15*page))
			{
				args.Player.SendInfoMessage(string.Format("Type /help {0} for more commands.", (page + 1)));
			}
		}

		private void GetVersion(CommandArgs args)
		{
			args.Player.SendInfoMessage(string.Format("TPulse: {0} ({1}): ({2}/{3})", TPulse.VersionNum, TPulse.VersionCodename,
												  TPulse.Utils.ActivePlayers(), TPulse.Config.MaxSlots));
		}

		private void ListConnectedPlayers(CommandArgs args)
		{
			//How many players per page
			const int pagelimit = 15;
			//How many players per line
			const int perline = 5;
			//Pages start at 0 but are displayed and parsed at 1
			int page = 0;


			if (args.Parameters.Count > 0)
			{
				if (!int.TryParse(args.Parameters[0], out page) || page < 1)
				{
					args.Player.SendErrorMessage(string.Format("Invalid page number ({0})", page));
					return;
				}
				page--; //Substract 1 as pages are parsed starting at 1 and not 0
			}

			var playerList = args.Player.Group.HasPermission(Permissions.seeids)
								 ? TPulse.Utils.GetPlayers(true)
								 : TPulse.Utils.GetPlayers(false);

			//Check if they are trying to access a page that doesn't exist.
			int pagecount = playerList.Count / pagelimit;
			if (page > pagecount)
			{
				args.Player.SendErrorMessage(string.Format("Page number exceeds pages ({0}/{1})", page + 1, pagecount + 1));
				return;
			}

			//Display the current page and the number of pages.
			args.Player.SendSuccessMessage(string.Format("Players: {0}/{1}",
												  TPulse.Utils.ActivePlayers(), TPulse.Config.MaxSlots));
			args.Player.SendSuccessMessage(string.Format("Current players page {0}/{1}:", page + 1, pagecount + 1));

			//Add up to pagelimit names to a list
			var nameslist = new List<string>();
			for (int i = (page * pagelimit); (i < ((page * pagelimit) + pagelimit)) && i < playerList.Count; i++)
			{
				nameslist.Add(playerList[i]);
			}

			//convert the list to an array for joining
			var names = nameslist.ToArray();
			for (int i = 0; i < names.Length; i += perline)
			{
				args.Player.SendInfoMessage(string.Join(", ", names, i, Math.Min(names.Length - i, perline)));
			}

			if (page < pagecount)
			{
				args.Player.SendInfoMessage(string.Format("Type /who {0} for more players.", (page + 2)));
			}
		}

		private void AuthToken(CommandArgs args)
		{
			if (TPulse.AuthToken == 0)
			{
				args.Player.SendWarningMessage("Auth is disabled. This incident has been logged.");
				Log.Warn(args.Player.IP + " attempted to use /auth even though it's disabled.");
				return;
			}
			int givenCode = Convert.ToInt32(args.Parameters[0]);
			if (givenCode == TPulse.AuthToken && args.Player.Group.Name != "superadmin")
			{
				try
				{
					TPulse.Users.AddUser(new User(args.Player.IP, "", "", "superadmin"));
					args.Player.Group = TPulse.Utils.GetGroup("superadmin");
					args.Player.SendInfoMessage("This IP address is now superadmin. Please perform the following command:");
					args.Player.SendInfoMessage("/user add <username>:<password> superadmin");
					args.Player.SendInfoMessage("Creates: <username> with the password <password> as part of the superadmin group.");
					args.Player.SendInfoMessage("Please use /login <username> <password> to login from now on.");
					args.Player.SendInfoMessage("If you understand, please /login <username> <password> now, and type /auth-verify.");
				}
				catch (UserManagerException ex)
				{
					Log.ConsoleError(ex.ToString());
					args.Player.SendErrorMessage(ex.Message);
				}
				return;
			}

			if (args.Player.Group.Name == "superadmin")
			{
				args.Player.SendInfoMessage("Please disable the auth system!");
				args.Player.SendInfoMessage("This IP address is now superadmin. Please perform the following command:");
				args.Player.SendInfoMessage("/user add <username>:<password> superadmin");
				args.Player.SendInfoMessage("Creates: <username> with the password <password> as part of the superadmin group.");
				args.Player.SendInfoMessage("Please use /login <username> <password> to login from now on.");
				args.Player.SendInfoMessage("If you understand, please /login <username> <password> now, and type /auth-verify.");
				return;
			}

			args.Player.SendErrorMessage("Incorrect auth code. This incident has been logged.");
			Log.Warn(args.Player.IP + " attempted to use an incorrect auth code.");
		}

		private void AuthVerify(CommandArgs args)
		{
			if (TPulse.AuthToken == 0)
			{
				args.Player.SendWarningMessage("It appears that you have already turned off the auth token.");
				args.Player.SendWarningMessage("If this is a mistake, delete auth.lck.");
				return;
			}

			if (!args.Player.IsLoggedIn)
			{
				args.Player.SendWarningMessage("You must be logged in to disable the auth system.");
				args.Player.SendWarningMessage("This is a security measure designed to prevent insecure administration setups.");
				args.Player.SendWarningMessage("Please re-run /auth and read the instructions!");
				return;
			}

			args.Player.SendSuccessMessage("Your new account has been verified, and the /auth system has been turned off.");
			args.Player.SendSuccessMessage("You can always use the /user command to manage players. Don't just delete the auth.lck.");
			args.Player.SendSuccessMessage("Thank you for using TPulse!");
			FileTools.CreateFile(Path.Combine(TPulse.SavePath, "auth.lck"));
			File.Delete(Path.Combine(TPulse.SavePath, "authcode.txt"));
			TPulse.AuthToken = 0;
		}

		private void ThirdPerson(CommandArgs args)
		{
			if (args.Parameters.Count == 0)
			{
				args.Player.SendErrorMessage("Invalid syntax! Proper syntax: /me <text>");
				return;
			}
			if (args.Player.mute)
				args.Player.SendErrorMessage("You are muted.");
			else
				TPPlayer.All.SendMessage(string.Format("*{0} {1}", args.Player.Name, String.Join(" ", args.Parameters)), 205, 133, 63);
		}

		private void PartyChat(CommandArgs args)
		{
			if (args.Parameters.Count == 0)
			{
				args.Player.SendErrorMessage("Invalid syntax! Proper syntax: /p <team chat text>");
				return;
			}
			int playerTeam = args.Player.Team;

			if (args.Player.mute)
				args.Player.SendErrorMessage("You are muted.");
			else if (playerTeam != 0)
			{
				string msg = string.Format("<{0}> {1}", args.Player.Name, String.Join(" ", args.Parameters));
				foreach (TPPlayer player in TPulse.Players)
				{
					if (player != null && player.Active && player.Team == playerTeam)
						player.SendMessage(msg, Main.teamColor[playerTeam].R, Main.teamColor[playerTeam].G, Main.teamColor[playerTeam].B);
				}
			}
			else
				args.Player.SendErrorMessage("You are not in a party!");
		}

		private void Mute(CommandArgs args)
		{
			if (args.Parameters.Count < 1)
			{
				args.Player.SendErrorMessage("Invalid syntax! Proper syntax: /mute <player> [reason]");
				return;
			}

			var players = TPulse.Utils.FindPlayer(args.Parameters[0]);
			if (players.Count == 0)
				args.Player.SendErrorMessage("Invalid player!");
			else if (players.Count > 1)
				args.Player.SendErrorMessage("More than one player matched!");
			else if (players[0].Group.HasPermission(Permissions.mute))
			{
				args.Player.SendErrorMessage("You cannot mute this player.");
			}
			else if (players[0].mute)
			{
				var plr = players[0];
				plr.mute = false;
				TPPlayer.All.SendInfoMessage(String.Format("{0} has been unmuted by {1}.", plr.Name, args.Player.Name));
			}
			else
			{
				string reason = "misbehavior";
				if (args.Parameters.Count > 1)
					reason = String.Join(" ", args.Parameters.ToArray(), 1, args.Parameters.Count - 1);
				var plr = players[0];
				plr.mute = true;
				TPPlayer.All.SendInfoMessage(String.Format("{0} has been muted by {1} for {2}.", plr.Name, args.Player.Name, reason));
			}
		}

		private void Motd(CommandArgs args)
		{
            //change this
			TPulse.Utils.ShowFileToUser(args.Player, "motd.txt");
		}

		private void Rules(CommandArgs args)
		{
            //same here, to be changed
			TPulse.Utils.ShowFileToUser(args.Player, "rules.txt");
		}

		private void Whisper(CommandArgs args)
		{
			if (args.Parameters.Count < 2)
			{
				args.Player.SendErrorMessage("Invalid syntax! Proper syntax: /whisper <player> <text>");
				return;
			}

			var players = TPulse.Utils.FindPlayer(args.Parameters[0]);
			if (players.Count == 0)
			{
				args.Player.SendErrorMessage("Invalid player!");
			}
			else if (players.Count > 1)
			{
				args.Player.SendErrorMessage("More than one player matched!");
			}
			else if (args.Player.mute)
				args.Player.SendErrorMessage("You are muted.");
			else
			{
				var plr = players[0];
				var msg = string.Join(" ", args.Parameters.ToArray(), 1, args.Parameters.Count - 1);
				plr.SendMessage("(Whisper From)" + "<" + args.Player.Name + ">" + msg, Color.MediumPurple);
				args.Player.SendMessage("(Whisper To)" + "<" + plr.Name + ">" + msg, Color.MediumPurple);
				plr.LastWhisper = args.Player;
				args.Player.LastWhisper = plr;
			}
		}

		private void Reply(CommandArgs args)
		{
			if (args.Player.mute)
				args.Player.SendErrorMessage("You are muted.");
			else if (args.Player.LastWhisper != null)
			{
				var msg = string.Join(" ", args.Parameters);
				args.Player.LastWhisper.SendMessage("(Whisper From)" + "<" + args.Player.Name + ">" + msg, Color.MediumPurple);
				args.Player.SendMessage("(Whisper To)" + "<" + args.Player.LastWhisper.Name + ">" + msg, Color.MediumPurple);
			}
			else
				args.Player.SendErrorMessage(
					"You haven't previously received any whispers. Please use /whisper to whisper to other people.");
		}

		private void Annoy(CommandArgs args)
		{
			if (args.Parameters.Count != 2)
			{
				args.Player.SendErrorMessage("Invalid syntax! Proper syntax: /annoy <player> <seconds to annoy>");
				return;
			}
			int annoy = 5;
			int.TryParse(args.Parameters[1], out annoy);

			var players = TPulse.Utils.FindPlayer(args.Parameters[0]);
			if (players.Count == 0)
				args.Player.SendErrorMessage("Invalid player!");
			else if (players.Count > 1)
				args.Player.SendErrorMessage("More than one player matched!");
			else
			{
				var ply = players[0];
				args.Player.SendSuccessMessage("Annoying " + ply.Name + " for " + annoy + " seconds.");
				(new Thread(ply.Whoopie)).Start(annoy);
			}
		}

		#endregion General Commands

		#region Cheat Commands

		private void Kill(CommandArgs args)
		{
			if (args.Parameters.Count < 1)
			{
				args.Player.SendErrorMessage("Invalid syntax! Proper syntax: /kill <player>");
				return;
			}

			string plStr = String.Join(" ", args.Parameters);
			var players = TPulse.Utils.FindPlayer(plStr);
			if (players.Count == 0)
			{
				args.Player.SendErrorMessage("Invalid player!");
			}
			else if (players.Count > 1)
			{
				args.Player.SendErrorMessage("More than one player matched!");
			}
			else
			{
				var plr = players[0];
				plr.DamagePlayer(999999);
				args.Player.SendSuccessMessage(string.Format("You just killed {0}!", plr.Name));
				plr.SendErrorMessage(string.Format("{0} just killed you!", args.Player.Name));
			}
		}

		private void Butcher(CommandArgs args)
		{
			if (args.Parameters.Count > 1)
			{
				args.Player.SendErrorMessage("Invalid syntax! Proper syntax: /butcher [killTownNPCs(true/false)]");
				return;
			}

			bool killTownNPCs = false;
			if (args.Parameters.Count == 1)
				bool.TryParse(args.Parameters[0], out killTownNPCs);

			int killcount = 0;
			for (int i = 0; i < Main.npc.Length; i++)
			{
				if (Main.npc[i].active && Main.npc[i].type != 0 && (!Main.npc[i].townNPC || killTownNPCs))
				{
					TPPlayer.Server.StrikeNPC(i, 99999, 90f, 1);
					killcount++;
				}
			}
			TPPlayer.All.SendSuccessMessage(string.Format("Killed {0} NPCs.", killcount));
		}
		
		private void Item(CommandArgs args)
		{
			if (args.Parameters.Count < 1)
			{
				args.Player.SendErrorMessage("Invalid syntax! Proper syntax: /item <item name/id> [item amount] [prefix id/name]");
				return;
			}
			if (args.Parameters[0].Length == 0)
			{
				args.Player.SendErrorMessage("Missing an item name/id.");
				return;
			}
			int itemAmount = 0;
			int prefix = 0;
			if (args.Parameters.Count == 2)
				int.TryParse(args.Parameters[1], out itemAmount);
			else if (args.Parameters.Count == 3)
			{
				int.TryParse(args.Parameters[1], out itemAmount);
				var found = TPulse.Utils.GetPrefixByIdOrName(args.Parameters[2]);
				if (found.Count == 1)
					prefix = found[0];
			}
			var items = TPulse.Utils.GetItemByIdOrName(args.Parameters[0]);
			if (items.Count == 0)
			{
				args.Player.SendErrorMessage("Invalid item type!");
			}
			else if (items.Count > 1)
			{
				args.Player.SendErrorMessage(string.Format("More than one ({0}) item matched!", items.Count));
			}
			else
			{
				var item = items[0];
				if (item.type >= 1 && item.type < Main.maxItemTypes)
				{
					if (args.Player.InventorySlotAvailable || item.name.Contains("Coin"))
					{
						if (itemAmount == 0 || itemAmount > item.maxStack)
							itemAmount = item.maxStack;
						if (args.Player.GiveItemCheck(item.type, item.name, item.width, item.height, itemAmount, prefix))
						{
							args.Player.SendSuccessMessage(string.Format("Gave {0} {1}(s).", itemAmount, item.name));
						}
						else
						{
							args.Player.SendErrorMessage("The item is banned and the config prevents you from spawning banned items.");
						}
					}
					else
					{
						args.Player.SendErrorMessage("You don't have free slots!");
					}
				}
				else
				{
					args.Player.SendErrorMessage("Invalid item type!");
				}
			}
		}

		private void Give(CommandArgs args)
		{
			if (args.Parameters.Count < 2)
			{
				args.Player.SendErrorMessage(
					"Invalid syntax! Proper syntax: /give <item type/id> <player> [item amount] [prefix id/name]");
				return;
			}
			if (args.Parameters[0].Length == 0)
			{
				args.Player.SendErrorMessage("Missing item name/id.");
				return;
			}
			if (args.Parameters[1].Length == 0)
			{
				args.Player.SendErrorMessage("Missing player name.");
				return;
			}
			int itemAmount = 0;
			int prefix = 0;
			var items = TPulse.Utils.GetItemByIdOrName(args.Parameters[0]);
			args.Parameters.RemoveAt(0);
			string plStr = args.Parameters[0];
			args.Parameters.RemoveAt(0);
			if (args.Parameters.Count == 1)
				int.TryParse(args.Parameters[0], out itemAmount);
			else if (args.Parameters.Count == 2)
			{
				int.TryParse(args.Parameters[0], out itemAmount);
				var found = TPulse.Utils.GetPrefixByIdOrName(args.Parameters[1]);
				if (found.Count == 1)
					prefix = found[0];
			}

			if (items.Count == 0)
			{
				args.Player.SendErrorMessage("Invalid item type!");
			}
			else if (items.Count > 1)
			{
				args.Player.SendErrorMessage(string.Format("More than one ({0}) item matched!", items.Count));
			}
			else
			{
				var item = items[0];
				if (item.type >= 1 && item.type < Main.maxItemTypes)
				{
					var players = TPulse.Utils.FindPlayer(plStr);
					if (players.Count == 0)
					{
						args.Player.SendErrorMessage("Invalid player!");
					}
					else if (players.Count > 1)
					{
						args.Player.SendErrorMessage("More than one player matched!");
					}
					else
					{
						var plr = players[0];
						if (plr.InventorySlotAvailable || item.name.Contains("Coin"))
						{
							if (itemAmount == 0 || itemAmount > item.maxStack)
								itemAmount = item.maxStack;
							if (plr.GiveItemCheck(item.type, item.name, item.width, item.height, itemAmount, prefix))
							{
								args.Player.SendSuccessMessage(string.Format("Gave {0} {1} {2}(s).", plr.Name, itemAmount, item.name));
								plr.SendSuccessMessage(string.Format("{0} gave you {1} {2}(s).", args.Player.Name, itemAmount, item.name));
							}
							else
							{
								args.Player.SendErrorMessage("The item is banned and the config prevents spawning banned items.");
							}
							
						}
						else
						{
							args.Player.SendErrorMessage("Player does not have free slots!");
						}
					}
				}
				else
				{
					args.Player.SendErrorMessage("Invalid item type!");
				}
			}
		}

		public void ClearItems(CommandArgs args)
		{
			int radius = 50;
			if (args.Parameters.Count > 0)
			{
				if (args.Parameters[0].ToLower() == "all")
				{
					radius = Int32.MaxValue/16;
				}
				else
				{
					try
					{
						radius = Convert.ToInt32(args.Parameters[0]);
					}
					catch (Exception)
					{
						args.Player.SendErrorMessage(
							"Please either enter the keyword \"all\", or the block radius you wish to delete all items from.");
						return;
					}
				}
			}
			int count = 0;
			for (int i = 0; i < 200; i++)
			{
				if (
					(Math.Sqrt(Math.Pow(Main.item[i].position.X - args.Player.X, 2) +
							   Math.Pow(Main.item[i].position.Y - args.Player.Y, 2)) < radius*16) && (Main.item[i].active))
				{
					Main.item[i].active = false;
					NetMessage.SendData(0x15, -1, -1, "", i, 0f, 0f, 0f, 0);
					count++;
				}
			}
			args.Player.SendSuccessMessage("All " + count + " items within a radius of " + radius + " have been deleted.");
		}

		private void Heal(CommandArgs args)
		{
			TPPlayer playerToHeal;
			if (args.Parameters.Count > 0)
			{
				string plStr = String.Join(" ", args.Parameters);
				var players = TPulse.Utils.FindPlayer(plStr);
				if (players.Count == 0)
				{
					args.Player.SendErrorMessage("Invalid player!");
					return;
				}
				else if (players.Count > 1)
				{
					args.Player.SendErrorMessage("More than one player matched!");
					return;
				}
				else
				{
					playerToHeal = players[0];
				}
			}
			else if (!args.Player.RealPlayer)
			{
				args.Player.SendErrorMessage("You cant heal yourself!");
				return;
			}
			else
			{
				playerToHeal = args.Player;
			}

			Item heart = TPulse.Utils.GetItemById(58);
			Item star = TPulse.Utils.GetItemById(184);
			for (int i = 0; i < 20; i++)
				playerToHeal.GiveItem(heart.type, heart.name, heart.width, heart.height, heart.maxStack);
			for (int i = 0; i < 10; i++)
				playerToHeal.GiveItem(star.type, star.name, star.width, star.height, star.maxStack);
			if (playerToHeal == args.Player)
			{
				args.Player.SendSuccessMessage("You just got healed!");
			}
			else
			{
				args.Player.SendSuccessMessage(string.Format("You just healed {0}", playerToHeal.Name));
				playerToHeal.SendSuccessMessage(string.Format("{0} just healed you!", args.Player.Name));
			}
		}

		private void Buff(CommandArgs args)
		{
			if (args.Parameters.Count < 1 || args.Parameters.Count > 2)
			{
				args.Player.SendErrorMessage("Invalid syntax! Proper syntax: /buff <buff id/name> [time(seconds)]");
				return;
			}
			int id = 0;
			int time = 60;
			if (!int.TryParse(args.Parameters[0], out id))
			{
				var found = TPulse.Utils.GetBuffByName(args.Parameters[0]);
				if (found.Count == 0)
				{
					args.Player.SendErrorMessage("Invalid buff name!");
					return;
				}
				else if (found.Count > 1)
				{
					args.Player.SendErrorMessage(string.Format("More than one ({0}) buff matched!", found.Count));
					return;
				}
				id = found[0];
			}
			if (args.Parameters.Count == 2)
				int.TryParse(args.Parameters[1], out time);
			if (id > 0 && id < Main.maxBuffs)
			{
				if (time < 0 || time > short.MaxValue)
					time = 60;
				args.Player.SetBuff(id, time*60);
				args.Player.SendSuccessMessage(string.Format("You have buffed yourself with {0}({1}) for {2} seconds!",
													  TPulse.Utils.GetBuffName(id), TPulse.Utils.GetBuffDescription(id), (time)));
			}
			else
				args.Player.SendErrorMessage("Invalid buff ID!");
		}

		private void GBuff(CommandArgs args)
		{
			if (args.Parameters.Count < 2 || args.Parameters.Count > 3)
			{
				args.Player.SendErrorMessage("Invalid syntax! Proper syntax: /gbuff <player> <buff id/name> [time(seconds)]");
				return;
			}
			int id = 0;
			int time = 60;
			var foundplr = TPulse.Utils.FindPlayer(args.Parameters[0]);
			if (foundplr.Count == 0)
			{
				args.Player.SendErrorMessage("Invalid player!");
				return;
			}
			else if (foundplr.Count > 1)
			{
				args.Player.SendErrorMessage(string.Format("More than one ({0}) player matched!", args.Parameters.Count));
				return;
			}
			else
			{
				if (!int.TryParse(args.Parameters[1], out id))
				{
					var found = TPulse.Utils.GetBuffByName(args.Parameters[1]);
					if (found.Count == 0)
					{
						args.Player.SendErrorMessage("Invalid buff name!");
						return;
					}
					else if (found.Count > 1)
					{
						args.Player.SendErrorMessage(string.Format("More than one ({0}) buff matched!", found.Count));
						return;
					}
					id = found[0];
				}
				if (args.Parameters.Count == 3)
					int.TryParse(args.Parameters[2], out time);
				if (id > 0 && id < Main.maxBuffs)
				{
					if (time < 0 || time > short.MaxValue)
						time = 60;
					foundplr[0].SetBuff(id, time*60);
					args.Player.SendSuccessMessage(string.Format("You have buffed {0} with {1}({2}) for {3} seconds!",
														  foundplr[0].Name, TPulse.Utils.GetBuffName(id),
														  TPulse.Utils.GetBuffDescription(id), (time)));
					foundplr[0].SendSuccessMessage(string.Format("{0} has buffed you with {1}({2}) for {3} seconds!",
														  args.Player.Name, TPulse.Utils.GetBuffName(id),
														  TPulse.Utils.GetBuffDescription(id), (time)));
				}
				else
					args.Player.SendErrorMessage("Invalid buff ID!");
			}
		}

		private void Grow(CommandArgs args)
		{
			if (args.Parameters.Count != 1)
			{
				args.Player.SendErrorMessage("Invalid syntax! Proper syntax: /grow [tree/epictree/mushroom/cactus/herb]");
				return;
			}
			var name = "Fail";
			var x = args.Player.TileX;
			var y = args.Player.TileY + 3;
			switch (args.Parameters[0].ToLower())
			{
				case "tree":
					for (int i = x - 1; i < x + 2; i++)
					{
						Main.tile[i, y].active = true;
						Main.tile[i, y].type = 2;
						Main.tile[i, y].wall = 0;
					}
					Main.tile[x, y - 1].wall = 0;
					WorldGen.GrowTree(x, y);
					name = "Tree";
					break;
				case "epictree":
					for (int i = x - 1; i < x + 2; i++)
					{
						Main.tile[i, y].active = true;
						Main.tile[i, y].type = 2;
						Main.tile[i, y].wall = 0;
					}
					Main.tile[x, y - 1].wall = 0;
					Main.tile[x, y - 1].liquid = 0;
					Main.tile[x, y - 1].active = true;
					WorldGen.GrowEpicTree(x, y);
					name = "Epic Tree";
					break;
				case "mushroom":
					for (int i = x - 1; i < x + 2; i++)
					{
						Main.tile[i, y].active = true;
						Main.tile[i, y].type = 70;
						Main.tile[i, y].wall = 0;
					}
					Main.tile[x, y - 1].wall = 0;
					WorldGen.GrowShroom(x, y);
					name = "Mushroom";
					break;
				case "cactus":
					Main.tile[x, y].type = 53;
					WorldGen.GrowCactus(x, y);
					name = "Cactus";
					break;
				case "herb":
					Main.tile[x, y].active = true;
					Main.tile[x, y].frameX = 36;
					Main.tile[x, y].type = 83;
					WorldGen.GrowAlch(x, y);
					name = "Herb";
					break;
				default:
					args.Player.SendErrorMessage("Unknown plant!");
					return;
			}
			args.Player.SendTileSquare(x, y);
			args.Player.SendSuccessMessage("Tried to grow a " + name + ".");
		}

		#endregion Cheat Comamnds
	}
}

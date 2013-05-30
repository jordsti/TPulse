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
using System.Text;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Net;
using System.Reflection;
using System.Threading;
using Hooks;
using MaxMind;
using Mono.Data.Sqlite;
using MySql.Data.MySqlClient;
using Rests;
using Terraria;
using TPulseAPI.DB;
using TPulseAPI.Net;
using TPulseAPI.Events;
using TPulseAPI.Server;

namespace TPulseAPI
{
	[APIVersion(1, 12)]
	public class TPulse : TerrariaPlugin
	{

        //Save Manager Event
        //err don't like to do this that way
        public event WorldSavedHandler OnWorldSaved
        {
            add
            {
                SaveManager.Instance.OnWorldSaved += value;
            }
            remove
            {
                SaveManager.Instance.OnWorldSaved -= value;
            }
        }

        //Player Connection Event
        private event PlayerConnectionHandler _onPlayerJoin;
        private event PlayerConnectionHandler _onPlayerLeave;

        public event PlayerConnectionHandler OnPlayerJoin
        {
            add
            {
                _onPlayerJoin += value;
            }

            remove
            {
                _onPlayerJoin -= value;
            }
        }


        public event PlayerConnectionHandler OnPlayerLeave
        {
            add
            {
                _onPlayerLeave += value;
            }

            remove
            {
                _onPlayerLeave -= value;
            }
        }

        private void onPlayerJoin(TPPlayer player)
        {
            if (_onPlayerJoin != null)
                _onPlayerJoin.Invoke(new PlayerConnectionEventArgs(player, PlayerConnectionAction.Join));
        }

        private void onPlayerLeave(TPPlayer player)
        {
            if (_onPlayerLeave != null)
                _onPlayerLeave.Invoke(new PlayerConnectionEventArgs(player, PlayerConnectionAction.Leave));
        }

        public Commands Commands { get; set; }

        private const string LogFormatDefault = "yyyy-MM-dd_HH-mm-ss";
		private string LogFormat = LogFormatDefault;
		private bool LogClear = false;
		public static readonly Version VersionNum = Assembly.GetExecutingAssembly().GetName().Version;
		public static readonly string VersionCodename = "Get forked!";

		public static string SavePath = TPulsePaths.GetPath(TPulsePath.SavePath);
        //why so many static field urgggg

        public ServerHandler ServerHandle { get; protected set; }
		public TPPlayer[] Players {get; protected set; }
        public BanManager Bans { get; protected set; }
        public WarpManager Warps { get; protected set; }
        public RegionManager Regions { get; protected set; }
        public BackupManager Backups { get; protected set; }
        public GroupManager Groups { get; protected set; }
        public UserManager Users { get; protected set; }
        public ItemManager Itembans { get; protected set; }
        public RememberedPosManager RememberedPos { get; protected set; }
        public InventoryManager InventoryDB { get; protected set; }
		public ConfigFile Config { get; set; }
        public IDbConnection DBConnection { get; protected set; }
		public bool OverridePort;
        public PacketBufferer PacketBuffer { get; protected set; }
        public GeoIPCountry Geo { get; protected set; }
		public SecureRest RestApi;
		public RestManager RestManager;
		//public static Utils Utils = Utils.Instance;
		//public static StatTracker StatTracker = new StatTracker();
		/// <summary>
		/// Used for implementing REST Tokens prior to the REST system starting up.
		/// </summary>
		public static Dictionary<string, string> RESTStartupTokens = new Dictionary<string, string>();

		/// <summary>
		/// Called after TShock is initialized. Useful for plugins that needs hooks before tshock but also depend on tshock being loaded.
		/// </summary>
		public static event Action Initialized;

		public override Version Version
		{
			get { return VersionNum; }
		}

		public override string Name
		{
			get { return "TPulse"; }
		}

		public override string Author
		{
			get { return "The Nyx Team, Forked by jordsti"; }
		}

		public override string Description
		{
			get { return "The administration modification of the future."; }
		}

		public TPulse(Main game)
			: base(game)
		{
			Config = new ConfigFile();
			Order = 0;
            Players = new TPPlayer[Main.maxPlayers];

            PlugInHandler.AddPlugIn(this);
            
            Commands = new Commands(this);

            ServerHandle = new ServerHandler(this);
		}

        #region PlayerHandling
        public bool IsUserOnline(int userId)
        {
            for (int i = 0; i < Players.Length; i++)
            {
                if (Players[i] != null)
                {
                    if (Players[i].UserID == userId)
                        return true;
                }
            }

            return false;
        }

        public TPPlayer GetOnlinePlayerByUserId(int userId)
        {
            for (int i = 0; i < Players.Length; i++)
            {
                if (Players[i] != null)
                {
                    TPPlayer p = Players[i];
                    if (p.UserID == userId)
                        return p;
                }
            }

            return null;
        }
        #endregion

        #region methods from utils
        [Obsolete("Put this into a PlayerManager class")]
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
        [Obsolete("Put this into a PlayerManager class")]
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

        [Obsolete("Put this into a PlayerManager class")]
        public void ForceKick(TPPlayer player, string reason, bool silent = false, bool saveSSI = false)
        {
            Kick(player, reason, true, silent, null, saveSSI);
        }

        [Obsolete("Put this into a PlayerManager class")]
        public void ForceKickAll(string reason)
        {
            foreach (TPPlayer player in Players)
            {
                if (player != null && player.Active)
                {
                    ForceKick(player, reason, false, true);
                }
            }
        }

        [Obsolete("Put this into a PlayerManager class")]
        public List<TPPlayer> FindPlayer(string plr)
        {
            var found = new List<TPPlayer>();
            // Avoid errors caused by null search
            if (plr == null)
                return found;

            byte plrID;
            if (byte.TryParse(plr, out plrID))
            {
                TPPlayer player = Players[plrID];
                if (player != null && player.Active)
                {
                    return new List<TPPlayer> { player };
                }
            }

            string plrLower = plr.ToLower();
            foreach (TPPlayer player in Players)
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

        //put this into a logtools maybe ?
        public void SendLogs(string log, Color color, TPulse tPulse)
        {
            Log.Info(log);
            TPPlayer.Server.SendMessage(log, color);
            foreach (TPPlayer player in Players)
            {
                if (player != null && player.Active && player.Group.HasPermission(Permissions.logs) && player.DisplayLogs &&
                    tPulse.Config.DisableSpewLogs == false)
                    player.SendMessage(log, color);
            }
        }

        [Obsolete("Put this into a PlayerManager class")]
        public string GetPlayerIP(string playername)
        {
            foreach (TPPlayer player in Players)
            {
                if (player != null && player.Active)
                {
                    if (playername.ToLower() == player.Name.ToLower())
                    {
                        return player.IP;
                    }
                }
            }
            return null;
        }

        [Obsolete("Put this into a PlayerManager class")]
        public List<string> GetPlayers(bool includeIDs)
        {
            var players = new List<string>();

            foreach (TPPlayer ply in Players)
            {
                if (ply != null && ply.Active)
                {
                    if (includeIDs)
                    {
                        players.Add(ply.Name + " (IX: " + ply.Index + ", ID: " + ply.UserID + ")");
                    }
                    else
                    {
                        players.Add(ply.Name);
                    }
                }
            }

            return players;
        }

        [Obsolete("Put this into a PlayerManager class")]
        public bool Ban(TPPlayer player, string reason, string adminUserName)
        {
            return Ban(player, reason, false, adminUserName);
        }

        [Obsolete("Put this into a PlayerManager class")]
        public bool Ban(TPPlayer player, string reason, bool force = false, string adminUserName = null)
        {
            if (!player.ConnectionAlive)
                return true;
            if (force || !player.Group.HasPermission(Permissions.immunetoban))
            {
                string ip = player.IP;
                string playerName = player.Name;
                Bans.AddBan(ip, playerName, reason);
                player.Disconnect(string.Format("Banned: {0}", reason));
                Log.ConsoleInfo(string.Format("Banned {0} for : {1}", playerName, reason));
                string verb = force ? "force " : "";
                if (string.IsNullOrWhiteSpace(adminUserName))
                    Utils.Broadcast(string.Format("{0} was {1}banned for {2}", playerName, verb, reason.ToLower()));
                else
                    Utils.Broadcast(string.Format("{0} {1}banned {2} for {3}", adminUserName, verb, playerName, reason.ToLower()));
                return true;
            }
            return false;
        }

        [Obsolete("Put this into a PlayerManager class")]
        public string GetPlayersWithIds()
        {
            var sb = new StringBuilder();
            foreach (TPPlayer player in Players)
            {
                if (player != null && player.Active)
                {
                    if (sb.Length != 0)
                    {
                        sb.Append(", ");
                    }
                    sb.Append(player.Name);
                    string id = "(ID: " + Convert.ToString(Users.GetUserID(player.UserAccountName)) + ", IX:" + player.Index + ")";
                    sb.Append(id);
                }
            }
            return sb.ToString();
        }


        public Group GetGroup(string groupName)
        {
            //first attempt on cached groups
            for (int i = 0; i < Groups.groups.Count; i++)
            {
                if (Groups.groups[i].Name.Equals(groupName))
                {
                    return Groups.groups[i];
                }
            }
            return new Group(Config.DefaultGuestGroupName);
        }

        #endregion

        [SuppressMessage("Microsoft.Security", "CA2122:DoNotIndirectlyExposeMethodsWithLinkDemands")]
		public override void Initialize()
		{
			HandleCommandLine(Environment.GetCommandLineArgs());
            //need to replace SavePath and use TPulsePath
            //HERE
			if (!Directory.Exists(SavePath))
				Directory.CreateDirectory(SavePath);

			DateTime now = DateTime.Now;
			string logFilename;
			try
			{
                //same has above
				logFilename = Path.Combine(SavePath, now.ToString(LogFormat)+".log");
			}
			catch(Exception)
			{
				// Problem with the log format use the default
				logFilename = Path.Combine(SavePath, now.ToString(LogFormatDefault) + ".log");
			}
#if DEBUG
			Log.Initialize(logFilename, LogLevel.All, false);
#else
			Log.Initialize(logFilename, LogLevel.All & ~LogLevel.Debug, LogClear);
#endif
			AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;

            //wtf is this condition
            //if (Version.Major >= 4)
            //{
                showTPulseWelcome();                
            //}

			try
			{
                //need to put pid file into constants
				if (File.Exists(TPulsePaths.GetPath(TPulsePath.ProcessFile)))
				{
					Log.ConsoleInfo(
						"TPulse was improperly shut down. Please use the exit command in the future to prevent this.");
                    File.Delete(TPulsePaths.GetPath(TPulsePath.ProcessFile));
				}
                File.WriteAllText(TPulsePaths.GetPath(TPulsePath.ProcessFile), Process.GetCurrentProcess().Id.ToString(CultureInfo.InvariantCulture));

				ConfigFile.ConfigRead += OnConfigRead;
				FileTools.SetupConfig(this);

				HandleCommandLinePostConfigLoad(Environment.GetCommandLineArgs());

				if (Config.StorageType.ToLower() == "sqlite")
				{
					string sql = TPulsePaths.GetPath(TPulsePath.SqliteFile);
					DBConnection = new SqliteConnection(string.Format("uri=file://{0},Version=3", sql));
				}
				else if (Config.StorageType.ToLower() == "mysql")
				{
					try
					{
						var hostport = Config.MySqlHost.Split(':');
						DBConnection = new MySqlConnection();
						DBConnection.ConnectionString =
							String.Format("Server={0}; Port={1}; Database={2}; Uid={3}; Pwd={4};",
										  hostport[0],
										  hostport.Length > 1 ? hostport[1] : "3306",
										  Config.MySqlDbName,
										  Config.MySqlUsername,
										  Config.MySqlPassword
								);
					}
					catch (MySqlException ex)
					{
						Log.Error(ex.ToString());
						throw new Exception("MySql not setup correctly");
					}
				}
				else
				{
					throw new Exception("Invalid storage type");
				}

				Backups = new BackupManager(TPulsePaths.GetPath(TPulsePath.BackupPath));
				Backups.KeepFor = Config.BackupKeepFor;
				Backups.Interval = Config.BackupInterval;
				Bans = new BanManager(DBConnection);
				Warps = new WarpManager(DBConnection);
                Regions = new RegionManager(DBConnection, this);
				Users = new UserManager(DBConnection, this);
				Groups = new GroupManager(DBConnection, this);
				Itembans = new ItemManager(DBConnection);
				RememberedPos = new RememberedPosManager(DBConnection);
				InventoryDB = new InventoryManager(DBConnection);
				RestApi = new SecureRest(Netplay.serverListenIP, Config.RestApiPort);
				RestApi.Verify += RestApi_Verify;
				RestApi.Port = Config.RestApiPort;
				RestManager = new RestManager(RestApi, this);
				RestManager.RegisterRestfulCommands();

				var geoippath = TPulsePaths.GetPath(TPulsePath.GeoIPFile);
				if (Config.EnableGeoIP && File.Exists(geoippath))
					Geo = new GeoIPCountry(geoippath);

				Log.ConsoleInfo(string.Format("TPulse Version {0} ({1}) now running.", Version, VersionCodename));

				GameHooks.PostInitialize += OnPostInit;
				GameHooks.Update += OnUpdate;
                GameHooks.HardUpdate += OnHardUpdate;
                GameHooks.StatueSpawn += OnStatueSpawn;
				ServerHooks.Connect += OnConnect;
				ServerHooks.Join += OnJoin;
				ServerHooks.Leave += OnLeave;
				ServerHooks.Chat += OnChat;
				ServerHooks.Command += ServerHooks_OnCommand;
				NetHooks.GetData += OnGetData;
				NetHooks.SendData += NetHooks_SendData;
				NetHooks.GreetPlayer += OnGreetPlayer;
				NpcHooks.StrikeNpc += NpcHooks_OnStrikeNpc;
			    NpcHooks.SetDefaultsInt += OnNpcSetDefaults;
				ProjectileHooks.SetDefaults += OnProjectileSetDefaults;
				WorldHooks.StartHardMode += OnStartHardMode;
				WorldHooks.SaveWorld += SaveManager.Instance.OnSaveWorld;
			    WorldHooks.ChristmasCheck += OnXmasCheck;
                NetHooks.NameCollision += NetHooks_NameCollision;

				GetDataHandlers.InitGetDataHandler();
				Commands.InitCommands();
				//RconHandler.StartThread();

				if (Config.RestApiEnabled)
					RestApi.Start();

				if (Config.BufferPackets)
					PacketBuffer = new PacketBufferer();

				Log.ConsoleInfo("AutoSave " + (Config.AutoSave ? "Enabled" : "Disabled"));
				Log.ConsoleInfo("Backups " + (Backups.Interval > 0 ? "Enabled" : "Disabled"));

				if (Initialized != null)
					Initialized();
			}
			catch (Exception ex)
			{
				Log.Error("Fatal Startup Exception");
				Log.Error(ex.ToString());
				Environment.Exit(1);
			}
		}

	    private static void showTPulseWelcome()
	    {
            

            Console.WriteLine("TPulse  Copyright (C) 2013  JordSti");
            Console.WriteLine("This program comes with ABSOLUTELY NO WARRANTY;");
            Console.WriteLine("This is free software, and you are welcome to redistribute it");
            Console.WriteLine("under certain conditions;");


            Console.WriteLine("........................................................");
            Console.WriteLine(".########.########..##.....##.##........######..########");
            Console.WriteLine("....##....##.....##.##.....##.##.......##....##.##......");
            Console.WriteLine("....##....##.....##.##.....##.##.......##.......##......");
            Console.WriteLine("....##....########..##.....##.##........######..######..");
            Console.WriteLine("....##....##........##.....##.##.............##.##......");
            Console.WriteLine("....##....##........##.....##.##.......##....##.##......");
            Console.WriteLine("....##....##.........#######..########..######..########");
            Console.WriteLine("........................................................");
            Console.WriteLine("TPulse is a recently forked version of TShock");
	    }

	    private RestObject RestApi_Verify(string username, string password)
		{
			var userAccount = Users.GetUserByName(username);
			if (userAccount == null)
			{
				return new RestObject("401")
						{Error = "Invalid username/password combination provided. Please re-submit your query with a correct pair."};
			}

			if (Utils.HashPassword(password).ToUpper() != userAccount.Password.ToUpper())
			{
				return new RestObject("401")
						{Error = "Invalid username/password combination provided. Please re-submit your query with a correct pair."};
			}

			if (!GetGroup(userAccount.Group).HasPermission(Permissions.restapi) && userAccount.Group != "superadmin")
			{
				return new RestObject("403")
						{
							Error =
								"Although your account was successfully found and identified, your account lacks the permission required to use the API. (api)"
						};
			}

			return new RestObject("200") {Response = "Successful login"}; //Maybe return some user info too?
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				// NOTE: order is important here
				if (Geo != null)
				{
					Geo.Dispose();
				}
				SaveManager.Instance.Dispose();

				GameHooks.PostInitialize -= OnPostInit;
				GameHooks.Update -= OnUpdate;
			    GameHooks.HardUpdate -= OnHardUpdate;
			    GameHooks.StatueSpawn -= OnStatueSpawn;
                ServerHooks.Connect -= OnConnect;
				ServerHooks.Join -= OnJoin;
				ServerHooks.Leave -= OnLeave;
				ServerHooks.Chat -= OnChat;
				ServerHooks.Command -= ServerHooks_OnCommand;
				NetHooks.GetData -= OnGetData;
				NetHooks.SendData -= NetHooks_SendData;
				NetHooks.GreetPlayer -= OnGreetPlayer;
				NpcHooks.StrikeNpc -= NpcHooks_OnStrikeNpc;
                NpcHooks.SetDefaultsInt -= OnNpcSetDefaults;
				ProjectileHooks.SetDefaults -= OnProjectileSetDefaults;
                WorldHooks.StartHardMode -= OnStartHardMode;
				WorldHooks.SaveWorld -= SaveManager.Instance.OnSaveWorld;
                WorldHooks.ChristmasCheck -= OnXmasCheck;
                NetHooks.NameCollision -= NetHooks_NameCollision;

				if (File.Exists(TPulsePaths.GetPath(TPulsePath.ProcessFile)))
				{
                    File.Delete(TPulsePaths.GetPath(TPulsePath.ProcessFile));
				}

				RestApi.Dispose();
				Log.Dispose();
			}
			base.Dispose(disposing);
		}

        void NetHooks_NameCollision(int who, string name, HandledEventArgs e)
        {
            string ip = Utils.GetRealIP(Netplay.serverSock[who].tcpClient.Client.RemoteEndPoint.ToString());
            foreach (TPPlayer ply in Players)
            {
                if (ply == null)
                {
                    continue;
                }
                if (ply.Name == name && ply.Index != who)
                {
                    if (ply.IP == ip)
                    {
                        if (ply.State < 2)
                        {
                            ForceKick(ply, "Name collision and this client has no world data.", true, false);
                            e.Handled = true;
                            return;
                        }
                        else
                        {
                            e.Handled = false;
                            return;
                        }
                    }
                }
            }
            e.Handled = false;
            return;
        }

        void OnXmasCheck(ChristmasCheckEventArgs args)
        {
            if (args.Handled)
                return;

            if(Config.ForceXmas)
            {
                args.Xmas = true;
                args.Handled = true;
            }
        }
		/// <summary>
		/// Handles exceptions that we didn't catch or that Red fucked up
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
		{
			Log.Error(e.ExceptionObject.ToString());

			if (e.ExceptionObject.ToString().Contains("Terraria.Netplay.ListenForClients") ||
				e.ExceptionObject.ToString().Contains("Terraria.Netplay.ServerLoop"))
			{
				var sb = new List<string>();
				for (int i = 0; i < Netplay.serverSock.Length; i++)
				{
					if (Netplay.serverSock[i] == null)
					{
						sb.Add("Sock[" + i + "]");
					}
					else if (Netplay.serverSock[i].tcpClient == null)
					{
						sb.Add("Tcp[" + i + "]");
					}
				}
				Log.Error(string.Join(", ", sb));
			}

			if (e.IsTerminating)
			{
				if (Main.worldPathName != null && Config.SaveWorldOnCrash)
				{
					Main.worldPathName += ".crash";
					SaveManager.Instance.SaveWorld();
				}
			}
		}

		private void HandleCommandLine(string[] parms)
		{
			string path;
			for (int i = 0; i < parms.Length; i++)
			{
				switch(parms[i].ToLower())
				{
					case "-configpath":
						path = parms[++i];
						if (path.IndexOfAny(Path.GetInvalidPathChars()) == -1)
						{
							SavePath = path;
							Log.ConsoleInfo("Config path has been set to " + path);
						}
						break;

					case "-worldpath":
						path = parms[++i];
						if (path.IndexOfAny(Path.GetInvalidPathChars()) == -1)
						{
							Main.WorldPath = path;
							Log.ConsoleInfo("World path has been set to " + path);
						}
						break;

					case "-dump":
						ConfigFile.DumpDescriptions();
						Permissions.DumpDescriptions();
						break;

					case "-logformat":
						LogFormat = parms[++i];
						break;

					case "-logclear":
						bool.TryParse(parms[++i], out LogClear);
						break;
				}
			}
		}

		public void HandleCommandLinePostConfigLoad(string[] parms)
		{
			for (int i = 0; i < parms.Length; i++)
			{
				switch(parms[i].ToLower())
				{
					case "-port":
						int port = Convert.ToInt32(parms[++i]);
						Netplay.serverPort = port;
						Config.ServerPort = port;
						OverridePort = true;
						Log.ConsoleInfo("Port overridden by startup argument. Set to " + port);
						break;
					case "-rest-token":
						string token = Convert.ToString(parms[++i]);
						RESTStartupTokens.Add(token, "null");
						Console.WriteLine("Startup parameter overrode REST token.");
						break;
					case "-rest-enabled":
						Config.RestApiEnabled = Convert.ToBoolean(parms[++i]);
						Console.WriteLine("Startup parameter overrode REST enable.");
						break;
					case "-rest-port":
						Config.RestApiPort = Convert.ToInt32(parms[++i]);
						Console.WriteLine("Startup parameter overrode REST port.");
						break;
					case "-maxplayers":
					case "-players":
						Config.MaxSlots = Convert.ToInt32(parms[++i]);
						Console.WriteLine("Startup parameter overrode maximum player slot configuration value.");
						break;
				}
			}
		}

		/*
		 * Hooks:
		 * 
		 */

		public static int AuthToken = -1;

		private void OnPostInit()
		{
			SetConsoleTitle();
			if (!File.Exists(TPulsePaths.GetPath(TPulsePath.AuthLockFile)) && !File.Exists(TPulsePaths.GetPath(TPulsePath.AuthCodeFile)))
			{
				var r = new Random((int) DateTime.Now.ToBinary());
				AuthToken = r.Next(100000, 10000000);
				Console.ForegroundColor = ConsoleColor.Yellow;
				Console.WriteLine("TPulse Notice: To become SuperAdmin, join the game and type /auth " + AuthToken);
				Console.WriteLine("This token will display until disabled by verification. (/auth-verify)");
				Console.ForegroundColor = ConsoleColor.Gray;
                FileTools.CreateFile(TPulsePaths.GetPath(TPulsePath.AuthCodeFile));
                using (var tw = new StreamWriter(TPulsePaths.GetPath(TPulsePath.AuthCodeFile)))
				{
					tw.WriteLine(AuthToken);
				}
			}
            else if (File.Exists(TPulsePaths.GetPath(TPulsePath.AuthCodeFile)))
			{
                using (var tr = new StreamReader(TPulsePaths.GetPath(TPulsePath.AuthCodeFile)))
				{
					AuthToken = Convert.ToInt32(tr.ReadLine());
				}
				Console.ForegroundColor = ConsoleColor.Yellow;
				Console.WriteLine(
					"TPulse Notice: authcode.txt is still present, and the AuthToken located in that file will be used.");
				Console.WriteLine("To become superadmin, join the game and type /auth " + AuthToken);
				Console.WriteLine("This token will display until disabled by verification. (/auth-verify)");
				Console.ForegroundColor = ConsoleColor.Gray;
			}
			else
			{
				AuthToken = 0;
			}

            Regions.ReloadAllRegions();

			//StatTracker.CheckIn();
			FixChestStacks();

            
		}

		private void FixChestStacks()
		{
			foreach (Chest chest in Main.chest)
			{
				if (chest != null)
				{
					foreach (Item item in chest.item)
					{
						if (item != null && item.stack > item.maxStack)
							item.stack = item.maxStack;
					}
				}
			}
		}

		private DateTime LastCheck = DateTime.UtcNow;
		private DateTime LastSave = DateTime.UtcNow;

		private void OnUpdate()
		{
			//UpdateManager.UpdateProcedureCheck();
			//StatTracker.CheckIn();
			if (Backups.IsBackupTime)
				Backups.Backup();
			//call these every second, not every update
			if ((DateTime.UtcNow - LastCheck).TotalSeconds >= 1)
			{
				OnSecondUpdate();
				LastCheck = DateTime.UtcNow;
			}

			if ((DateTime.UtcNow - LastSave).TotalMinutes >= Config.ServerSideInventorySave)
			{
				foreach (TPPlayer player in Players)
				{
					// prevent null point exceptions
					if (player != null && player.IsLoggedIn && !player.IgnoreActionsForClearingTrashCan)
					{

						InventoryDB.InsertPlayerData(player);
					}
				}
				LastSave = DateTime.UtcNow;
			}
		}

		private void OnSecondUpdate()
		{
			if (Config.ForceTime != "normal")
			{
				switch (Config.ForceTime)
				{
					case "day":
						TPPlayer.Server.SetTime(true, 27000.0);
						break;
					case "night":
						TPPlayer.Server.SetTime(false, 16200.0);
						break;
				}
			}
			int count = 0;
			foreach (TPPlayer player in Players)
			{
				if (player != null && player.Active)
				{
					count++;
					if (player.TilesDestroyed != null)
					{
						if (player.TileKillThreshold >= Config.TileKillThreshold)
						{
							player.Disable("Reached TileKill threshold.");
							TPPlayer.Server.RevertTiles(player.TilesDestroyed);
							player.TilesDestroyed.Clear();
						}
					}
					if (player.TileKillThreshold > 0)
					{
						player.TileKillThreshold = 0;
						//We don't want to revert the entire map in case of a disable.
						player.TilesDestroyed.Clear();
					}
					if (player.TilesCreated != null)
					{
						if (player.TilePlaceThreshold >= Config.TilePlaceThreshold)
						{
							player.Disable("Reached TilePlace threshold.");
							TPPlayer.Server.RevertTiles(player.TilesCreated);
							player.TilesCreated.Clear();
						}
					}
					if (player.TilePlaceThreshold > 0)
					{
						player.TilePlaceThreshold = 0;
					}
					if (player.TileLiquidThreshold >= Config.TileLiquidThreshold)
					{
						player.Disable("Reached TileLiquid threshold.");
					}
					if (player.TileLiquidThreshold > 0)
					{
						player.TileLiquidThreshold = 0;
					}
					if (player.ProjectileThreshold >= Config.ProjectileThreshold)
					{
						player.Disable("Reached projectile threshold.");
					}
					if (player.ProjectileThreshold > 0)
					{
						player.ProjectileThreshold = 0;
					}
					if (player.Dead && (DateTime.Now - player.LastDeath).Seconds >= 3 && player.Difficulty != 2)
					{
						player.Spawn();
					}
					string check = "none";
					foreach (Item item in player.TPlayer.inventory)
					{
						if (!player.Group.HasPermission(Permissions.ignorestackhackdetection) && item.stack > item.maxStack &&
							item.type != 0)
						{
							check = "Remove item " + item.name + " (" + item.stack + ") exceeds max stack of " + item.maxStack;
						}
					}
					player.IgnoreActionsForCheating = check;
					check = "none";
					foreach (Item item in player.TPlayer.armor)
					{
						if (!player.Group.HasPermission(Permissions.usebanneditem) && Itembans.ItemIsBanned(item.name, player))
						{
							player.SetBuff(30, 120); //Bleeding
							player.SetBuff(36, 120); //Broken Armor
							check = "Remove armor/accessory " + item.name;
						}
					}
					player.IgnoreActionsForDisabledArmor = check;
					if (CheckIgnores(player))
					{
						player.SetBuff(33, 120); //Weak
						player.SetBuff(32, 120); //Slow
						player.SetBuff(23, 120); //Cursed
					}
					else if (!player.Group.HasPermission(Permissions.usebanneditem) &&
							 Itembans.ItemIsBanned(player.TPlayer.inventory[player.TPlayer.selectedItem].name, player))
					{
						player.SetBuff(23, 120); //Cursed
					}
				}
			}
			SetConsoleTitle();
		}

		private void SetConsoleTitle()
		{
		    Console.Title = string.Format("{0}{1}/{2} @ {3}:{4} (TPulse v{5})",
		                                  !string.IsNullOrWhiteSpace(Config.ServerName) ? Config.ServerName + " - " : "",
		                                  Utils.ActivePlayers(),
		                                  Config.MaxSlots, Netplay.serverListenIP, Netplay.serverPort, Version);
		}

        private void OnHardUpdate( HardUpdateEventArgs args )
        {
            if (args.Handled)
                return;

            if (!Config.AllowCorruptionCreep && ( args.Type == 23 || args.Type == 25 || args.Type == 0 ||
                args.Type == 112 || args.Type == 23 || args.Type == 32 ) )
            {
                args.Handled = true;
                return;
            }

            if (!Config.AllowHallowCreep && (args.Type == 109 || args.Type == 117 || args.Type == 116 ) )
            {
                args.Handled = true;
            }
        }

        private void OnStatueSpawn( StatueSpawnEventArgs args )
        {
            if( args.Within200 < Config.StatueSpawn200 && args.Within600 < Config.StatueSpawn600 && args.WorldWide < Config.StatueSpawnWorld )
            {
                args.Handled = true;
            }
            else
            {
                args.Handled = false;
            }
        }

		private void OnConnect(int ply, HandledEventArgs handler)
		{
			var player = new TPPlayer(ply);
			if (Config.EnableDNSHostResolution)
			{
				player.Group = Users.GetGroupForIPExpensive(player.IP);
			}
			else
			{
				player.Group = Users.GetGroupForIP(player.IP);
			}

			if (Utils.ActivePlayers() + 1 > Config.MaxSlots + 20)
			{
				ForceKick(player, Config.ServerFullNoReservedReason, true, false);
				handler.Handled = true;
				return;
			}

			var ipban = Bans.GetBanByIp(player.IP);
			Ban ban = null;
			if (ipban != null && Config.EnableIPBans)
				ban = ipban;

			if (ban != null)
			{
				ForceKick(player, string.Format("You are banned: {0}", ban.Reason), true, false);
				handler.Handled = true;
				return;
			}

			if (!FileTools.OnWhitelist(player.IP, this))
			{
				ForceKick(player, Config.WhitelistKickReason, true, false);
				handler.Handled = true;
				return;
			}

			if (Geo != null)
			{
				var code = Geo.TryGetCountryCode(IPAddress.Parse(player.IP));
				player.Country = code == null ? "N/A" : GeoIPCountry.GetCountryNameByCode(code);
				if (code == "A1")
				{
					if (Config.KickProxyUsers)
					{
						ForceKick(player, "Proxies are not allowed.", true, false);
						handler.Handled = true;
						return;
					}
				}
			}
		    Players[ply] = player;
		}

		private void OnJoin(int ply, HandledEventArgs handler)
		{
			var player = Players[ply];
			if (player == null)
			{
				handler.Handled = true;
				return;
			}

			Ban ban = null;
			if (Config.EnableBanOnUsernames)
			{
				var newban = Bans.GetBanByName(player.Name);
				if (null != newban)
					ban = newban;
			}

			if (Config.EnableIPBans && null == ban)
			{
				ban = Bans.GetBanByIp(player.IP);
			}

			if (ban != null)
			{
				ForceKick(player, string.Format("You are banned: {0}", ban.Reason), true, false);
				handler.Handled = true;
				return;
			}
            
            //All is good
            onPlayerJoin(player);
		}

		private void OnLeave(int ply)
		{

			var tsplr = Players[ply];
			Players[ply] = null;

			if (tsplr != null && tsplr.ReceivedInfo)
			{
				if (!tsplr.SilentKickInProgress || tsplr.State > 1)
				{
					if (tsplr.State >= 2)
					{
                        Utils.Broadcast(tsplr.Name + " left", Color.Yellow);    
					}
				}
				Log.Info(string.Format("{0} disconnected.", tsplr.Name));

				if (tsplr.IsLoggedIn && !tsplr.IgnoreActionsForClearingTrashCan)
				{
					tsplr.PlayerData.CopyInventory(tsplr);
					InventoryDB.InsertPlayerData(tsplr);
				}

				if ((Config.RememberLeavePos) &&(!tsplr.LoginHarassed))
				{
					RememberedPos.InsertLeavePos(tsplr.Name, tsplr.IP, (int) (tsplr.X/16), (int) (tsplr.Y/16));
				}

                onPlayerLeave(tsplr);
			}
		}

		private void OnChat(messageBuffer msg, int ply, string text, HandledEventArgs e)
		{
			if (e.Handled)
				return;

			var tsplr = Players[msg.whoAmI];
			if (tsplr == null)
			{
				e.Handled = true;
				return;
			}

			/*if (!Utils.ValidString(text))
			{
				e.Handled = true;
				return;
			}*/

			if (text.StartsWith("/"))
			{
				try
				{
					e.Handled = Commands.HandleCommand(tsplr, text);
				}
				catch (Exception ex)
				{
					Log.ConsoleError("Command exception");
					Log.Error(ex.ToString());
				}
			}
			else if (!tsplr.mute && !Config.EnableChatAboveHeads)
			{
				Utils.Broadcast(
					String.Format(Config.ChatFormat, tsplr.Group.Name, tsplr.Group.Prefix, tsplr.Name, tsplr.Group.Suffix, text),
					tsplr.Group.R, tsplr.Group.G, tsplr.Group.B);
				e.Handled = true;
			} else if (!tsplr.mute && Config.EnableChatAboveHeads)
			{
			    Utils.Broadcast(ply, String.Format(Config.ChatAboveHeadsFormat, tsplr.Group.Name, tsplr.Group.Prefix, tsplr.Name, tsplr.Group.Suffix, text), tsplr.Group.R, tsplr.Group.G, tsplr.Group.B);
			    e.Handled = true;
			}
			else if (tsplr.mute)
			{
				tsplr.SendErrorMessage("You are muted!");
				e.Handled = true;
			}
		}

		/// <summary>
		/// When a server command is run.
		/// </summary>
		/// <param name="cmd"></param>
		/// <param name="e"></param>
		private void ServerHooks_OnCommand(string text, HandledEventArgs e)
		{
			if (e.Handled)
				return;

			// Damn you ThreadStatic and Redigit
			if (Main.rand == null)
			{
				Main.rand = new Random();
			}
			if (WorldGen.genRand == null)
			{
				WorldGen.genRand = new Random();
			}

			if (text.StartsWith("playing") || text.StartsWith("/playing"))
			{
				int count = 0;
				foreach (TPPlayer player in Players)
				{
					if (player != null && player.Active)
					{
						count++;
						TPPlayer.Server.SendInfoMessage(string.Format("{0} ({1}) [{2}] <{3}>", player.Name, player.IP,
																  player.Group.Name, player.UserAccountName));
					}
				}
				TPPlayer.Server.SendInfoMessage(string.Format("{0} players connected.", count));
			}
			else if (text == "autosave")
			{
				Main.autoSave = Config.AutoSave = !Config.AutoSave;
				Log.ConsoleInfo("AutoSave " + (Config.AutoSave ? "Enabled" : "Disabled"));
			}
			else if (text.StartsWith("/"))
			{
				Commands.HandleCommand(TPPlayer.Server, text);
			}
			else
			{
				Commands.HandleCommand(TPPlayer.Server, "/" + text);
			}
			e.Handled = true;
		}

		private void OnGetData(GetDataEventArgs e)
		{
			if (e.Handled)
				return;

			PacketTypes type = e.MsgID;

			Debug.WriteLine("Recv: {0:X}: {2} ({1:XX})", e.Msg.whoAmI, (byte) type, type);

			var player = Players[e.Msg.whoAmI];
			if (player == null)
			{
				e.Handled = true;
				return;
			}

			if (!player.ConnectionAlive)
			{
				e.Handled = true;
				return;
			}

			if (player.RequiresPassword && type != PacketTypes.PasswordSend)
			{
				e.Handled = true;
				return;
			}

			if ((player.State < 10 || player.Dead) && (int) type > 12 && (int) type != 16 && (int) type != 42 && (int) type != 50 &&
				(int) type != 38 && (int) type != 21)
			{
				e.Handled = true;
				return;
			}

			using (var data = new MemoryStream(e.Msg.readBuffer, e.Index, e.Length))
			{
				try
				{
					if (GetDataHandlers.HandlerGetData(type, player, data))
						e.Handled = true;
				}
				catch (Exception ex)
				{
					Log.Error(ex.ToString());
				}
			}
		}


        public string GetPlayersList()
        {
            string list = "";

            foreach (TPPlayer p in Players)
            {
                if (p != null && p.Active)
                {
                    list += p.Name + ", ";
                }
            }

            return list.TrimEnd(new char[] { ' ', ',' });
        }


		private void OnGreetPlayer(int who, HandledEventArgs e)
		{
			var player = Players[who];
			if (player == null)
			{
				e.Handled = true;
				return;
			}
			player.LoginMS= DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond;
			//some work to do on this method
			Utils.ShowFileToUser(player, "motd.txt", GetPlayersList());

			if (Config.PvPMode == "always" && !player.TPlayer.hostile)
			{
				player.SendMessage("PvP is forced! Enable PvP else you can't do anything!", Color.Red);
			}

			if (!player.IsLoggedIn)
			{
				if (Config.ServerSideInventory)
				{
					player.SendMessage(
						player.IgnoreActionsForInventory = "Server side inventory is enabled! Please /register or /login to play!",
						Color.Red);
						player.LoginHarassed = true;
				}
				else if (Config.RequireLogin)
				{
					player.SendMessage("Please /register or /login to play!", Color.Red);
					player.LoginHarassed = true;
				}
			}

			if (player.Group.HasPermission(Permissions.causeevents) && Config.InfiniteInvasion)
			{
				StartInvasion();
			}

			player.LastNetPosition = new Vector2(Main.spawnTileX*16f, Main.spawnTileY*16f);

			if (Config.RememberLeavePos)
			{
			if (RememberedPos.GetLeavePos(player.Name, player.IP) != Vector2.Zero){
				var pos = RememberedPos.GetLeavePos(player.Name, player.IP);

				player.Teleport((int) pos.X, (int) pos.Y + 3);
			}}

			e.Handled = true;
		}

		private void NpcHooks_OnStrikeNpc(NpcStrikeEventArgs e)
		{
			if (Config.InfiniteInvasion)
			{
				IncrementKills();
				if (Main.invasionSize < 10)
				{
					Main.invasionSize = 20000000;
				}
			}
		}

		private void OnProjectileSetDefaults(SetDefaultsEventArgs<Projectile, int> e)
		{
			if (e.Info == 43)
				if (Config.DisableTombstones)
					e.Object.SetDefaults(0);
			if (e.Info == 75)
				if (Config.DisableClownBombs)
					e.Object.SetDefaults(0);
			if (e.Info == 109)
				if (Config.DisableSnowBalls)
					e.Object.SetDefaults(0);
		}

		private void OnNpcSetDefaults(SetDefaultsEventArgs<NPC, int> e)
		{
			if (Itembans.ItemIsBanned(e.Object.name, null))
			{
				e.Object.SetDefaults(0);
			}
		}

		/// <summary>
		/// Send bytes to client using packetbuffering if available
		/// </summary>
		/// <param name="client">socket to send to</param>
		/// <param name="bytes">bytes to send</param>
		/// <returns>False on exception</returns>
		public bool SendBytes(ServerSock client, byte[] bytes)
		{
			if (PacketBuffer != null)
			{
				PacketBuffer.BufferBytes(client, bytes);
				return true;
			}

			return SendBytesBufferless(client, bytes);
		}

		/// <summary>
		/// Send bytes to a client ignoring the packet buffer
		/// </summary>
		/// <param name="client">socket to send to</param>
		/// <param name="bytes">bytes to send</param>
		/// <returns>False on exception</returns>
		public static bool SendBytesBufferless(ServerSock client, byte[] bytes)
		{
			try
			{
				if (client.tcpClient.Connected)
					client.networkStream.Write(bytes, 0, bytes.Length);
				return true;
			}
			catch (Exception ex)
			{
				Log.Warn("This is a normal exception");
				Log.Warn(ex.ToString());
			}
			return false;
		}

		private void NetHooks_SendData(SendDataEventArgs e)
		{
			if (e.MsgID == PacketTypes.Disconnect)
			{
				Action<ServerSock, string> senddisconnect = (sock, str) =>
																{
																	if (sock == null || !sock.active)
																		return;
																	sock.kill = true;
																	using (var ms = new MemoryStream())
																	{
																		new DisconnectMsg {Reason = str}.PackFull(ms);
																		SendBytesBufferless(sock, ms.ToArray());
																	}
																};

				if (e.remoteClient != -1)
				{
					senddisconnect(Netplay.serverSock[e.remoteClient], e.text);
				}
				else
				{
					for (int i = 0; i < Netplay.serverSock.Length; i++)
					{
						if (e.ignoreClient != -1 && e.ignoreClient == i)
							continue;

						senddisconnect(Netplay.serverSock[i], e.text);
					}
				}
				e.Handled = true;
			}
            if (e.MsgID == PacketTypes.WorldInfo)
            {
                if (e.remoteClient == -1) return;
                var player = Players[e.remoteClient];
                if (player == null) return;
                if (Config.UseServerName)
                {
                    using (var ms = new MemoryStream())
                    {
                        var msg = new WorldInfoMsg
                        {
                            Time = (int)Main.time,
                            DayTime = Main.dayTime,
                            MoonPhase = (byte)Main.moonPhase,
                            BloodMoon = Main.bloodMoon,
                            MaxTilesX = Main.maxTilesX,
                            MaxTilesY = Main.maxTilesY,
                            SpawnX = Main.spawnTileX,
                            SpawnY = Main.spawnTileY,
                            WorldSurface = (int)Main.worldSurface,
                            RockLayer = (int)Main.rockLayer,
                            WorldID = Main.worldID,
                            WorldFlags =
                                (WorldGen.shadowOrbSmashed ? WorldInfoFlag.OrbSmashed : WorldInfoFlag.None) |
                                (NPC.downedBoss1 ? WorldInfoFlag.DownedBoss1 : WorldInfoFlag.None) |
                                (NPC.downedBoss2 ? WorldInfoFlag.DownedBoss2 : WorldInfoFlag.None) |
                                (NPC.downedBoss3 ? WorldInfoFlag.DownedBoss3 : WorldInfoFlag.None) |
                                (Main.hardMode ? WorldInfoFlag.HardMode : WorldInfoFlag.None) |
                                (NPC.downedClown ? WorldInfoFlag.DownedClown : WorldInfoFlag.None),
                            WorldName = Config.ServerName
                        };
                        msg.PackFull(ms);
                        player.SendRawData(ms.ToArray());
                    }
                    e.Handled = true;
                }
            }
		}

		private void OnStartHardMode(HandledEventArgs e)
		{
			if (Config.DisableHardmode)
				e.Handled = true;
		}

	    /*
		 * Useful stuff:
		 * */

		public void StartInvasion()
		{
			Main.invasionType = 1;
			if (Config.InfiniteInvasion)
			{
				Main.invasionSize = 20000000;
			}
			else
			{
				Main.invasionSize = 100 + (Config.InvasionMultiplier*Utils.ActivePlayers());
			}

			Main.invasionWarn = 0;
			if (new Random().Next(2) == 0)
			{
				Main.invasionX = 0.0;
			}
			else
			{
				Main.invasionX = Main.maxTilesX;
			}
		}

		private static int KillCount;

		public static void IncrementKills()
		{
			KillCount++;
			Random r = new Random();
			int random = r.Next(5);
			if (KillCount%100 == 0)
			{
				switch (random)
				{
					case 0:
						Utils.Broadcast(string.Format("You call that a lot? {0} goblins killed!", KillCount), Color.Green);
						break;
					case 1:
						Utils.Broadcast(string.Format("Fatality! {0} goblins killed!", KillCount), Color.Green);
						break;
					case 2:
						Utils.Broadcast(string.Format("Number of 'noobs' killed to date: {0}", KillCount), Color.Green);
						break;
					case 3:
						Utils.Broadcast(string.Format("Duke Nukem would be proud. {0} goblins killed.", KillCount), Color.Green);
						break;
					case 4:
						Utils.Broadcast(string.Format("You call that a lot? {0} goblins killed!", KillCount), Color.Green);
						break;
					case 5:
						Utils.Broadcast(string.Format("{0} copies of Call of Duty smashed.", KillCount), Color.Green);
						break;
				}
			}
		}

		public bool CheckProjectilePermission(TPPlayer player, int index, int type)
		{
			if (type == 43)
			{
				return true;
			}

			if (type == 17 && !player.Group.HasPermission(Permissions.usebanneditem) && Itembans.ItemIsBanned("Dirt Rod", player))
				//Dirt Rod Projectile
			{
				return true;
			}

			if ((type == 42 || type == 65 || type == 68) && !player.Group.HasPermission(Permissions.usebanneditem) &&
				Itembans.ItemIsBanned("Sandgun", player)) //Sandgun Projectiles
			{
				return true;
			}

			Projectile proj = new Projectile();
			proj.SetDefaults(type);

			if (!player.Group.HasPermission(Permissions.usebanneditem) && Itembans.ItemIsBanned(proj.name, player))
			{
				return true;
			}

			if (Main.projHostile[type])
			{
                //player.SendMessage( proj.name, Color.Yellow);
				return true;
			}

			return false;
		}

		public bool CheckRangePermission(TPPlayer player, int x, int y, int range = 32)
		{
			if (Config.RangeChecks && ((Math.Abs(player.TileX - x) > range) || (Math.Abs(player.TileY - y) > range)))
			{
				return true;
			}
			return false;
		}

        public bool CheckTilePermission( TPPlayer player, int tileX, int tileY, byte tileType, byte actionType )
        {
            if (!player.Group.HasPermission(Permissions.canbuild))
            {
				if (Config.AllowIce && actionType != 1)
				{

					foreach (Point p in player.IceTiles)
					{
						if (p.X == tileX && p.Y == tileY && (Main.tile[p.X, p.Y].type == 0 || Main.tile[p.X, p.Y].type == 127))
						{
							player.IceTiles.Remove(p);
							return false;
						}
					}

    		        if (((DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond) - player.BPm) > 2000){
					    player.SendMessage("You do not have permission to build!", Color.Red);
			            player.BPm=DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond;
                    }

					return true;
				}

				if (Config.AllowIce && actionType == 1 && tileType == 127)
				{
					player.IceTiles.Add(new Point(tileX, tileY));
					return false;
				}
				
		        if (((DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond) - player.BPm) > 2000){
					player.SendMessage("You do not have permission to build!", Color.Red);
			        player.BPm=DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond;
                }   

				return true;

            }

            if (!player.Group.HasPermission(Permissions.editspawn) && !Regions.CanBuild(tileX, tileY, player) &&
                Regions.InArea(tileX, tileY))
            {
                if (((DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond) - player.RPm) > 2000)
                {
                    player.SendMessage("This region is protected from changes.", Color.Red);
                    player.RPm = DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond;

                }
                return true;
            }

            if (Config.DisableBuild)
            {
                if (!player.Group.HasPermission(Permissions.editspawn))
                {
 		    if (((DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond) - player.WPm) > 2000){
                        player.SendMessage("The world is protected from changes.", Color.Red);
			player.WPm=DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond;

}
                    return true;
                }
            }
            if (Config.SpawnProtection)
            {
                if (!player.Group.HasPermission(Permissions.editspawn))
                {
                    var flag = CheckSpawn(tileX, tileY);
                    if (flag)
                    {		
					if (((DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond) - player.SPm) > 2000){
                        player.SendMessage("Spawn is protected from changes.", Color.Red);
						player.SPm=DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond;
						}
                    return true;
                    }
                }
            }
            return false;
        }

		public bool CheckTilePermission(TPPlayer player, int tileX, int tileY)
		{
			if (!player.Group.HasPermission(Permissions.canbuild))
			{

		    if (((DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond) - player.BPm) > 2000){
					player.SendMessage("You do not have permission to build!", Color.Red);
					player.BPm=DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond;
					}
				return true;
			}

            if (!player.Group.HasPermission(Permissions.editspawn) && !Regions.CanBuild(tileX, tileY, player) &&
                    Regions.InArea(tileX, tileY))
            {


                if (((DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond) - player.RPm) > 2000)
                {
                    player.SendMessage("This region is protected from changes.", Color.Red);
                    player.RPm = DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond;
                }
                return true;
            }

			if (Config.DisableBuild)
			{
				if (!player.Group.HasPermission(Permissions.editspawn))
				{
				if (((DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond) - player.WPm) > 2000){
                        player.SendMessage("The world is protected from changes.", Color.Red);
						player.WPm=DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond;
						}
					return true;
				}
			}
			if (Config.SpawnProtection)
			{
				if (!player.Group.HasPermission(Permissions.editspawn))
				{
					var flag = CheckSpawn(tileX, tileY);
					if (flag)
					{
					if (((DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond) - player.SPm) > 1000){
                        player.SendMessage("Spawn is protected from changes.", Color.Red);
						player.SPm=DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond;

						}


						return true;
					}
				}
			}
			return false;
		}
		public bool CheckSpawn(int x, int y)
		{
			Vector2 tile = new Vector2(x, y);
			Vector2 spawn = new Vector2(Main.spawnTileX, Main.spawnTileY);
			return Distance(spawn, tile) <= Config.SpawnProtectionRadius;
		}

		public static float Distance(Vector2 value1, Vector2 value2)
		{
			float num2 = value1.X - value2.X;
			float num = value1.Y - value2.Y;
			float num3 = (num2*num2) + (num*num);
			return (float) Math.Sqrt(num3);
		}

		public static bool HackedHealth(TPPlayer player)
		{
			return (player.TPlayer.statManaMax > 400) ||
				   (player.TPlayer.statMana > 400) ||
				   (player.TPlayer.statLifeMax > 400) ||
				   (player.TPlayer.statLife > 400);
		}

		public static bool HackedInventory(TPPlayer player)
		{
			bool check = false;

			Item[] inventory = player.TPlayer.inventory;
			Item[] armor = player.TPlayer.armor;
			for (int i = 0; i < NetItem.maxNetInventory; i++)
			{
				if (i < 49)
				{
					Item item = new Item();
					if (inventory[i] != null && inventory[i].netID != 0)
					{
						item.netDefaults(inventory[i].netID);
						item.Prefix(inventory[i].prefix);
						item.AffixName();
						if (inventory[i].stack > item.maxStack)
						{
							check = true;
							player.SendMessage(
								String.Format("Stack cheat detected. Remove item {0} ({1}) and then rejoin", item.name, inventory[i].stack),
								Color.Cyan);
						}
					}
				}
				else
				{
					Item item = new Item();
					if (armor[i - 48] != null && armor[i - 48].netID != 0)
					{
						item.netDefaults(armor[i - 48].netID);
						item.Prefix(armor[i - 48].prefix);
						item.AffixName();
						if (armor[i - 48].stack > item.maxStack)
						{
							check = true;
							player.SendMessage(
								String.Format("Stack cheat detected. Remove armor {0} ({1}) and then rejoin", item.name, armor[i - 48].stack),
								Color.Cyan);
						}
					}
				}
			}

			return check;
		}

		public static bool CheckInventory(TPPlayer player)
		{
			PlayerData playerData = player.PlayerData;
			bool check = true;

			if (player.TPlayer.statLifeMax > playerData.maxHealth)
			{
				player.SendMessage("Error: Your max health exceeded (" + playerData.maxHealth + ") which is stored on server.",
								   Color.Cyan);
				check = false;
			}

			Item[] inventory = player.TPlayer.inventory;
			Item[] armor = player.TPlayer.armor;
			for (int i = 0; i < NetItem.maxNetInventory; i++)
			{
				if (i < 49)
				{
					Item item = new Item();
					Item serverItem = new Item();
					if (inventory[i] != null && inventory[i].netID != 0)
					{
						if (playerData.inventory[i].netID != inventory[i].netID)
						{
							item.netDefaults(inventory[i].netID);
							item.Prefix(inventory[i].prefix);
							item.AffixName();
							player.SendMessage(player.IgnoreActionsForInventory = "Your item (" + item.name + ") needs to be deleted.",
											   Color.Cyan);
							check = false;
						}
						else if (playerData.inventory[i].prefix != inventory[i].prefix)
						{
							item.netDefaults(inventory[i].netID);
							item.Prefix(inventory[i].prefix);
							item.AffixName();
							player.SendMessage(player.IgnoreActionsForInventory = "Your item (" + item.name + ") needs to be deleted.",
											   Color.Cyan);
							check = false;
						}
						else if (inventory[i].stack > playerData.inventory[i].stack)
						{
							item.netDefaults(inventory[i].netID);
							item.Prefix(inventory[i].prefix);
							item.AffixName();
							player.SendMessage(
								player.IgnoreActionsForInventory =
								"Your item (" + item.name + ") (" + inventory[i].stack + ") needs to have its stack size decreased to (" +
								playerData.inventory[i].stack + ").", Color.Cyan);
							check = false;
						}
					}
				}
				else
				{
					Item item = new Item();
					Item serverItem = new Item();
					if (armor[i - 48] != null && armor[i - 48].netID != 0)
					{
						if (playerData.inventory[i].netID != armor[i - 48].netID)
						{
							item.netDefaults(armor[i - 48].netID);
							item.Prefix(armor[i - 48].prefix);
							item.AffixName();
							player.SendMessage(player.IgnoreActionsForInventory = "Your armor (" + item.name + ") needs to be deleted.",
											   Color.Cyan);
							check = false;
						}
						else if (playerData.inventory[i].prefix != armor[i - 48].prefix)
						{
							item.netDefaults(armor[i - 48].netID);
							item.Prefix(armor[i - 48].prefix);
							item.AffixName();
							player.SendMessage(player.IgnoreActionsForInventory = "Your armor (" + item.name + ") needs to be deleted.",
											   Color.Cyan);
							check = false;
						}
						else if (armor[i - 48].stack > playerData.inventory[i].stack)
						{
							item.netDefaults(armor[i - 48].netID);
							item.Prefix(armor[i - 48].prefix);
							item.AffixName();
							player.SendMessage(
								player.IgnoreActionsForInventory =
								"Your armor (" + item.name + ") (" + inventory[i].stack + ") needs to have its stack size decreased to (" +
								playerData.inventory[i].stack + ").", Color.Cyan);
							check = false;
						}
					}
				}
			}

			return check;
		}

		public bool CheckIgnores(TPPlayer player)
		{
			bool check = false;
			if (Config.PvPMode == "always" && !player.TPlayer.hostile)
				check = true;
			if (player.IgnoreActionsForInventory != "none")
				check = true;
			if (player.IgnoreActionsForCheating != "none")
				check = true;
			if (player.IgnoreActionsForDisabledArmor != "none")
				check = true;
			if (player.IgnoreActionsForClearingTrashCan)
				check = true;
			if (!player.IsLoggedIn && Config.RequireLogin)
				check = true;
			return check;
		}

		public void OnConfigRead(ConfigFile file)
		{
			NPC.defaultMaxSpawns = file.DefaultMaximumSpawns;
			NPC.defaultSpawnRate = file.DefaultSpawnRate;

			Main.autoSave = file.AutoSave;
			if (Backups != null)
			{
				Backups.KeepFor = file.BackupKeepFor;
				Backups.Interval = file.BackupInterval;
			}
			if (!OverridePort)
			{
				Netplay.serverPort = file.ServerPort;
			}

			if (file.MaxSlots > 235)
				file.MaxSlots = 235;
			Main.maxNetPlayers = file.MaxSlots + 20;
			Netplay.password = "";
			Netplay.spamCheck = false;

			RconHandler.Password = file.RconPassword;
			RconHandler.ListenPort = file.RconPort;

			Utils.HashAlgo = file.HashAlgorithm;

            file.ServerName = file.ServerNickname;
		}
	}
}

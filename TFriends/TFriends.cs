using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.ComponentModel;
using System.IO;
using Terraria;
using TPulseAPI;
using TPulseAPI.DB;

namespace TFriends
{

    [APIVersion(1, 12)]
    public class TFriends : TerrariaPlugin
    {
        FriendsDB FriendsList = new FriendsDB();

        public TFriends(Main game) : base(game)
        {
            PlugInHandler.AddPlugIn(this);
        }

        //Plug-In Info
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
                return "TFriends";
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
                return "Give the opportunity to use a friend list";
            }
        }

        //Initialization

        public override void Initialize()
        {
            //Loading Friends DB
            if (File.Exists(FriendsDB.DefaultFile))
            {
                FriendsList = FriendsDB.Load(FriendsDB.DefaultFile);
            }
            else
            {
                FriendsList.Save(FriendsDB.DefaultFile);
            }


           //Adding commands

            Commands.ChatCommands.Add(new Command("", FriendCommand, "friend"));
            Commands.ChatCommands.Add(new Command("", MessageCommand, "fmsg"));
            //Commands.ChatCommands.Add(new Command("", MessageCommand, "sendcoord"));
            Commands.OnPlayerLogin += new PlayerLoginHandler(PlayerLogin);
            Hooks.WorldHooks.SaveWorld += new Hooks.WorldHooks.SaveWorldD(WorldHooks_SaveWorld);

        }

        private void PlayerLogin(PlayerLoginEventArgs args)
        {
            FriendList fl = FriendsList.GetListByUserID(args.Player.UserID);
            
            if (fl == null)
                return;

            foreach (FUser fu in fl.Friends)
            {
                if(IsUserOnline(fu.ID))
                {
                    TPPlayer dplayer = GetOnlinePlayerById(fu.ID);
                    if (dplayer != null)
                    {
                        dplayer.SendInfoMessage(String.Format("{0} just logged in!", args.Player.UserAccountName));
                    }
                }
            }
        }


        private void WorldHooks_SaveWorld(bool resettime, HandledEventArgs e)
        {
            //Need to save friend db
            Console.Write("Saving friends list...");
            FriendsList.Save(FriendsDB.DefaultFile);
        }

        private FriendList GetListOrCreate(User u)
        {
            FriendList fl = FriendsList[u.ID];

            if (fl == null)
            {
                fl = new FriendList(new FUser(u));
                FriendsList.Add(fl);
            }
            
            return fl;
        }

        private bool IsUserOnline(int userId)
        {
            for (int i = 0; i < TPulse.Players.Length; i++)
            {
                if (TPulse.Players[i] != null)
                {
                    if (TPulse.Players[i].UserID == userId)
                        return true;
                }
            }


                return false;
        }


        private TPPlayer GetOnlinePlayerById(int id)
        {
            for (int i = 0; i < TPulse.Players.Length; i++)
            {
                if (TPulse.Players[i] != null)
                {
                    if (TPulse.Players[i].UserID == id)
                        return TPulse.Players[i];
                }
            }

            return null;
        }

        //Command methods

        private static Regex regexNumber = new Regex("^[0-9]{1,3}$");

        private void MessageCommand(CommandArgs args)
        {
            try
            {
                TPPlayer player = args.Player;
                TPPlayer dest;
                FriendList fl;
                if (player.IsLoggedIn)
                {
                    fl = FriendsList.GetListByUserID(player.UserID);
                }
                else
                    return;


                if (args.Parameters.Count >= 1)
                {
                    string target = args.Parameters[0];
                    string message = "";

                    for (int i = 1; i < args.Parameters.Count; i++)
                    {
                        message += args.Parameters[i] + " ";
                    }

                    if (regexNumber.IsMatch(target))
                    {
                        //Friends number

                        int order = int.Parse(target) - 1;

                        FUser udest = fl.GetUser(order);
                        if (udest != null)
                        {
                            dest = GetOnlinePlayerById(udest.ID);

                            if (dest != null)
                            {
                                dest.SendInfoMessage(String.Format("{0}: {1}", player.UserAccountName, message));
                            }
                            else
                            {
                                player.SendInfoMessage("Friends: this friend is not online");
                            }
                        }
                        else
                        {
                            player.SendInfoMessage(String.Format("Friends: {0} is not a valid friend id", order.ToString()));
                        }
                    }
                    else
                    {
                        string name = args.Parameters[0];
                        FUser udest = fl.GetUser(name);
                        if (udest != null)
                        {
                            dest = GetOnlinePlayerById(udest.ID);

                            if (dest != null)
                            {
                                dest.SendInfoMessage(String.Format("{0}: {1}", player.UserAccountName, message));
                            }
                            else
                            {
                                player.SendInfoMessage("Friends: this friend is not online");
                            }
                        }
                        else
                        {
                            player.SendInfoMessage(String.Format("Friends: {0} is not a valid friend", name));
                        }
                    }


                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                Console.WriteLine(ex.StackTrace);
                Console.WriteLine(ex.Source);
                Console.WriteLine(ex.TargetSite);
            }
        }

        private void FriendCommand(CommandArgs args)
        {
  
            //Sub-command
            //add : add a friend
            //del : delete a friend
            //list : list your friend list
            //help : Send a help message
            TPPlayer player = args.Player;

            if (player != null)
            {
                if (player.IsLoggedIn)
                {
                    Console.WriteLine(args.Parameters.Count);

                    foreach (string p in args.Parameters)
                    {
                        Console.WriteLine(p);
                    }

                    if (args.Parameters.Count >= 2)
                    {
                        User cu = TPulse.Users.GetUserByID(player.UserID);
                        FriendList fl = GetListOrCreate(cu);
                        string prams = args.Parameters[0];
                        prams = prams.ToLower();
                        Console.WriteLine(prams);
                        if (prams == "add")
                        {

                            //Add
                            string adduser = args.Parameters[1];

                            User u = TPulse.Users.GetUserByName(adduser);

                            if (u == null)
                            {
                                player.SendInfoMessage(String.Format("Friends: {0} doesn't exists !", adduser));
                                return;
                            }

                            if (fl.Contains(u.Name))
                            {
                                player.SendInfoMessage(String.Format("Friends: {0} is already in your friend list", adduser));
                                return;
                            }

                            FUser fu = new FUser(u);

                            fl.Friends.Add(fu);

                            player.SendInfoMessage(String.Format("Friends: {0} added!", adduser));
                        }
                        else if (prams == "del")
                        {
                            //Delete
                            string remuser = args.Parameters[1];

                            if (fl.Contains(remuser))
                            {
                                fl.RemoveUserByName(remuser);
                                player.SendInfoMessage(String.Format("Friends: {0} deleted!", remuser));
                            }
                            else
                            {
                                player.SendInfoMessage(String.Format("Friends: {0} is not in your friends list", remuser));
                            }
                            
                        }


                    }
                    else if (args.Parameters.Count == 1)
                    {
                        string prams = args.Parameters[0];

                        if (prams == "help")
                        {
                            SendHelp(player);
                        }
                        else if (prams == "list")
                        {
                            User cu = TPulse.Users.GetUserByID(player.UserID);
                            FriendList fl = GetListOrCreate(cu);

                            player.SendInfoMessage(String.Format("Friends: {0} friend(s) in your list", fl.Friends.Count.ToString()));
                            int i = 1;
                            foreach (FUser fu in fl.Friends)
                            {
                                string status = IsUserOnline(fu.ID) ? "Online" : "Offline";

                                player.SendInfoMessage(String.Format("{0} : {1} : {2}", i.ToString(), fu.Name, status));

                                i++;
                            }
                        }
                    }
                    else
                    {
                        player.SendInfoMessage("Friends: Missing arguments");
                    }
                }
                else
                {
                    player.SendInfoMessage("Friends: You can't use friend list, because you're not logged!");
                }
            }
        }

        private void SendHelp(TPPlayer player)
        {
            player.SendInfoMessage("Friends: /friend cmd [args]");
            player.SendInfoMessage("Friends: Available commands");
            player.SendInfoMessage("Friends: add, del, list, help");
        }
    
    }
}

﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.ComponentModel;
using System.IO;
using Terraria;
using TPulseAPI;
using TPulseAPI.DB;
using TPulseAPI.Events;

namespace TFriends
{

    /* Idea :
     *      Friend Message logger
     *      More status like busy,afk
     * To Do
     *     Put the verbose message into the log file
     */ 

    [APIVersion(1, 12)]
    public class TFriends : TerrariaPlugin
    {
        protected FriendsDB FriendsList = new FriendsDB();
        protected Color TextColor = new Color(41, 69, 255);

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

        private TPulse tPulse;

        public override void Initialize()
        {
            tPulse = (TPulse)PlugInHandler.GetPluginByType(typeof(TPulse));

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

            tPulse.Commands.ChatCommands.Add(new Command("", FriendCommand, "friend"));
            tPulse.Commands.ChatCommands.Add(new Command("", MessageCommand, "fmsg"));
            tPulse.Commands.ChatCommands.Add(new Command("", MessageAllCommand, "fall"));
            //Commands.ChatCommands.Add(new Command("", MessageCommand, "sendcoord"));
            tPulse.Commands.OnPlayerLogin += new PlayerLoginHandler(PlayerLogin);
            //Hooks.WorldHooks.SaveWorld += new Hooks.WorldHooks.SaveWorldD(WorldHooks_SaveWorld);
            tPulse.OnWorldSaved += new WorldSavedHandler(OnWorldSaved);
        }

        private void MessageAllCommand(CommandArgs args)
        {
            TPPlayer player = args.Player;

            if(player.IsLoggedIn)
            {
                FriendList fl = FriendsList.GetListByUserID(player.UserID);

                if (fl != null)
                {
                    string message = "";

                    for (int i = 0; i < args.Parameters.Count; i++)
                    {
                        message += args.Parameters[i] + " ";
                    }

                    foreach (FUser fu in fl.Friends)
                    {
                        TPPlayer pdest = GetOnlinePlayerById(fu.ID);

                        if (pdest != null)
                        {
                            pdest.SendMessage(String.Format("{0}: {1}", player.UserAccountName, message), TextColor);
                        }
                    }

                    player.SendMessage(String.Format("{0}: {1}", "To All", message), TextColor);
                }
            }
            else
            {
                player.SendErrorMessage("Friends: You're not logged in!");
            }
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
                        dplayer.SendMessage(String.Format("{0} just logged in!", args.Player.UserAccountName), TextColor);
                    }
                }
            }
        }


        private void OnWorldSaved(WorldSavedEventArgs args)
        {
            //Need to save friend db
            //put this into the log file, when TPulse logger will not be static
            Console.WriteLine("Saving friends list...");
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
            for (int i = 0; i < tPulse.Players.Length; i++)
            {
                if (tPulse.Players[i] != null)
                {
                    if (tPulse.Players[i].UserID == userId)
                        return true;
                }
            }


                return false;
        }


        private TPPlayer GetOnlinePlayerById(int id)
        {
            for (int i = 0; i < tPulse.Players.Length; i++)
            {
                if (tPulse.Players[i] != null)
                {
                    if (tPulse.Players[i].UserID == id)
                        return tPulse.Players[i];
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
                                dest.SendMessage(String.Format("{0}: {1}", player.UserAccountName, message), TextColor);
                                player.SendMessage(String.Format("To {0}: {1}", dest.UserAccountName, message), TextColor);
                            }
                            else
                            {
                                player.SendMessage("Friends: this friend is not online", TextColor);
                            }
                        }
                        else
                        {
                            player.SendMessage(String.Format("Friends: {0} is not a valid friend id", order.ToString()), TextColor);
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
                                dest.SendMessage(String.Format("{0}: {1}", player.UserAccountName, message), TextColor);
                            }
                            else
                            {
                                player.SendMessage("Friends: this friend is not online", TextColor);
                            }
                        }
                        else
                        {
                            player.SendMessage(String.Format("Friends: {0} is not a valid friend", name), TextColor);
                        }
                    }


                }
            }
            catch (Exception ex)
            {
                //put this into a log file or just remove it
                //will see
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

                    if (args.Parameters.Count >= 2)
                    {
                        User cu = tPulse.Users.GetUserByID(player.UserID);
                        FriendList fl = GetListOrCreate(cu);
                        string prams = args.Parameters[0];
                        prams = prams.ToLower();

                        if (prams == "add")
                        {

                            //Add
                            string adduser = args.Parameters[1];

                            User u = tPulse.Users.GetUserByName(adduser);

                            if (u == null)
                            {
                                player.SendMessage(String.Format("Friends: {0} doesn't exists !", adduser), TextColor);
                                return;
                            }

                            if (fl.Contains(u.Name))
                            {
                                player.SendMessage(String.Format("Friends: {0} is already in your friend list", adduser), TextColor);
                                return;
                            }

                            FUser fu = new FUser(u);

                            fl.Friends.Add(fu);

                            player.SendMessage(String.Format("Friends: {0} added!", adduser), TextColor);
                        }
                        else if (prams == "del")
                        {
                            //Delete
                            string remuser = args.Parameters[1];

                            if (fl.Contains(remuser))
                            {
                                fl.RemoveUserByName(remuser);
                                player.SendMessage(String.Format("Friends: {0} deleted!", remuser), TextColor);
                            }
                            else
                            {
                                player.SendMessage(String.Format("Friends: {0} is not in your friends list", remuser), TextColor);
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
                            User cu = tPulse.Users.GetUserByID(player.UserID);
                            FriendList fl = GetListOrCreate(cu);

                            player.SendMessage(String.Format("Friends: {0} friend(s) in your list", fl.Friends.Count.ToString()), TextColor);
                            int i = 1;
                            foreach (FUser fu in fl.Friends)
                            {
                                string status = IsUserOnline(fu.ID) ? "Online" : "Offline";

                                player.SendMessage(String.Format("{0} : {1} : {2}", i.ToString(), fu.Name, status), TextColor);

                                i++;
                            }
                        }
                    }
                    else
                    {
                        player.SendMessage("Friends: Missing arguments", TextColor);
                    }
                }
                else
                {
                    player.SendMessage("Friends: You can't use friend list, because you're not logged!", TextColor);
                }
            }
        }

        private void SendHelp(TPPlayer player)
        {
            player.SendMessage("Friends: /friend cmd [args]", TextColor);
            player.SendMessage("Friends: Available commands", TextColor);
            player.SendMessage("Friends: add, del, list, help", TextColor);
        }
    
    }
}

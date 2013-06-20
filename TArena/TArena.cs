using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Terraria;
using TPulseAPI;
using System.Diagnostics;
namespace TArena
{
    [APIVersion(1, 12)]
    public class TArena : TerrariaPlugin
    {
        public enum ArenaState
        {
            Idle,
            Preparation,
            Battle,
            InMatchWait,
        }

        protected TPulse tPulse;
        protected ArenaSetting Setting;
        protected string ArenaSettingFile = TPulsePaths.Combine(TPulsePath.SavePath, "arena.xml");
        protected List<TPPlayer> Team1 = new List<TPPlayer>();
        protected List<TPPlayer> Team2 = new List<TPPlayer>();
        protected List<TPPlayer> Team1Alive = new List<TPPlayer>();
        protected List<TPPlayer> Team2Alive = new List<TPPlayer>();
        protected bool InBattle = false;
        protected ArenaState State = ArenaState.Idle;

        protected int Team1Point = 0;
        protected int Team2Point = 0;

        protected List<APoint> stones = new List<APoint>();

        public TArena(Main game)
            : base(game)
        {
            PlugInHandler.AddPlugIn(this);
        }

        public override Version Version
        {
            get
            {
                return new Version("0.0");
            }
        }

        public override string Name
        {
            get
            {
                return "TArena";
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
                return "Arena Dueling";
            }
        }

        public override void Initialize()
        {
            tPulse = (TPulse)PlugInHandler.GetPluginByType(typeof(TPulse));
            Setting = ArenaSetting.Load(ArenaSettingFile);
            //todo

            //commands
            tPulse.Commands.ChatCommands.Add(new Command("", SetPrepare, "asetprepare"));
            tPulse.Commands.ChatCommands.Add(new Command("", SetTeamStart, "asetstart"));
            tPulse.Commands.ChatCommands.Add(new Command("", PreparePlayers, "aprepare"));
            tPulse.Commands.ChatCommands.Add(new Command("", AssignPlayerTeam, "aassign"));
            tPulse.Commands.ChatCommands.Add(new Command("", ResetTeams, "areset"));
            tPulse.Commands.ChatCommands.Add(new Command("", SetArenaHall, "asethall"));
            tPulse.Commands.ChatCommands.Add(new Command("", SetAterMatch, "asetroom"));

            Hooks.GameHooks.Update += new Action(GameHooks_Update);

            //Team Color
            //Console.WriteLine(Main.teamColor.Length);
            Main.teamColor[1] = Color.Blue;
            Main.teamColor[2] = Color.Pink;
        }

        protected bool IsParticipantId(int id)
        {
            foreach (TPPlayer p in Team1)
            {
                if (p.Index == id)
                {
                    return true;
                }
            }

            foreach (TPPlayer p in Team2)
            {
                if (p.Index == id)
                {
                    return true;
                }
            }

            return false;
        }

        protected Stopwatch timer = new Stopwatch();

        protected List<long> SecMessageSended = new List<long>();



        protected void BuffPlayers()
        {
            foreach (TPPlayer p in Team1)
            {
                foreach (BuffType b in Setting.Buffs)
                {
                    p.SetBuff((int)b);
                }
            }

            foreach (TPPlayer p in Team2)
            {
                foreach (BuffType b in Setting.Buffs)
                {
                    p.SetBuff((int)b);
                }
            }
        }


        protected void StartBattle()
        {
            ChangePvpStatus();
            Team1Alive.Clear();
            Team2Alive.Clear();

            foreach (TPPlayer p in Team1)
            {
                p.TPlayer.HealEffect(400);
                p.Teleport(Setting.TeamStart1.X, Setting.TeamStart1.Y);
                p.SetTeam(1);
                //p.TPlayer.StatusPvP(122, p.Index);
                Team1Alive.Add(p);
                
            }

            foreach (TPPlayer p in Team2)
            {
                p.TPlayer.HealEffect(400);
                p.Teleport(Setting.TeamStart2.X, Setting.TeamStart2.Y);
                p.SetTeam(2);
                //p.TPlayer.StatusPvP(122, p.Index);
                Team2Alive.Add(p);
            }

            State = ArenaState.Battle;
        }

        protected void ChangePvpStatus(bool pvp = true)
        {
            foreach (TPPlayer p in Team1)
            {
                if (p.TPlayer.hostile != pvp)
                {
                    p.TPlayer.hostile = pvp;
                    NetMessage.SendData((int)PacketTypes.TogglePvp, -1, -1, "", p.Index);
                }
            }
            foreach (TPPlayer p in Team2)
            {
                if (p.TPlayer.hostile != pvp)
                {
                    p.TPlayer.hostile = pvp;
                    NetMessage.SendData((int)PacketTypes.TogglePvp, -1, -1, "", p.Index);
                }
            }
        }

        public void GameHooks_Update()
        {
            if (State == ArenaState.Preparation)
            {
                if (timer.ElapsedMilliseconds < Setting.PreparationTime)
                {
                    long sec = timer.ElapsedMilliseconds / 1000;

                    if (sec % 5 == 0)
                    {
                        if (!SecMessageSended.Contains(sec))
                        {
                            MessageParticipant(String.Format("Battle starting in {0} seconds", (Setting.PreparationTime / 1000) - sec));
                            SecMessageSended.Add(sec);
                        }
                    }
                }
                else
                {
                    SecMessageSended.Clear();
                    //State = ArenaState.Battle;
                    timer.Stop();
                    timer.Reset();
                    BuffPlayers();
                    Utils.Broadcast("Arena: Battle is starting !");
                    StartBattle();

                    //teleport to the starting position
                }
            }
            else if (State == ArenaState.Battle)
            {
                foreach (TPPlayer p in Team1)
                {
                    if (p.TPlayer.dead || p.TPlayer.statLife == 0)
                    {
                        Team1Alive.Remove(p);
                        Utils.Broadcast(String.Format("Arena: {0} is dead !",p.Name));
                    }
                }

                foreach (TPPlayer p in Team2)
                {
                    if (p.TPlayer.dead || p.TPlayer.statLife == 0)
                    {
                        Team2Alive.Remove(p);
                        Utils.Broadcast(String.Format("Arena: {0} is dead !", p.Name));
                    }
                }

                if (Team1Alive.Count == 0)
                {
                    //Team 1 defeated
                    Utils.Broadcast("Arena: Team 1 was defeated !");
                    Team2Point++;
                }
                else if(Team2Alive.Count == 0)
                {
                    //Team 2 defeated
                    Utils.Broadcast("Arena: Team 2 was defeated !");
                    Team1Point++;
                }

                if (Team1Point < Setting.PointsToWin && Team2Point < Setting.PointsToWin && 
                    (Team1Alive.Count == 0 || Team2Alive.Count == 0)
                    )
                {
                    //another battle
                    PrepareBattle();
                }
                else if (Team1Point == Setting.PointsToWin)
                {
                    //team1 win
                    Utils.Broadcast(String.Format("Arena: Team 1 has win ! {0} to {1}", Team1Point, Team2Point));
                    ClearingBattle();
                    State = ArenaState.Idle;
                }
                else if (Team2Point == Setting.PointsToWin)
                {
                    //team2 win
                    Utils.Broadcast(String.Format("Arena: Team 2 has win ! {0} to {1}", Team2Point, Team1Point));
                    ClearingBattle();
                    State = ArenaState.Idle;
                }
            }
        }

        protected void ClearingBattle()
        {
            ChangePvpStatus(false);

            foreach (TPPlayer p in Team1)
            {
                p.Teleport(Setting.AfterMatchRoom.X, Setting.AfterMatchRoom.Y);
            }

            foreach (TPPlayer p in Team2)
            {
                p.Teleport(Setting.AfterMatchRoom.X, Setting.AfterMatchRoom.Y);
            }

            Team2Point = 0;
            Team1Point = 0;

            //Team1.Clear();
            //Team2.Clear();

            //teleporting out of the arena too
            //clearing teams ? maybe
        }


        protected void MessageParticipant(string message)
        {
            foreach (TPPlayer p in Team1)
            {
                p.SendInfoMessage(String.Format("Arena: {0}", message));
            }

            foreach (TPPlayer p in Team2)
            {
                p.SendInfoMessage(String.Format("Arena: {0}", message));
            }
        }

        protected void ResetTeams(CommandArgs args)
        {
            if (args.Player.Group.ContainsGroup("superadmin"))
            {
                Team1.Clear();
                Team2.Clear();
                args.Player.SendInfoMessage("Arena: Team cleared!");
            }
            else
            {
                args.Player.SendErrorMessage("Arena: You can't do this!");
            }
        }

        public bool IsInTeam(TPPlayer player)
        {
            return (Team1.Contains(player) || Team2.Contains(player));
        }

        protected void AssignPlayerTeam(CommandArgs args)
        {
            if (args.Player.Group.ContainsGroup("superadmin"))
            {
                if (args.Parameters.Count != 2)
                {
                    args.Player.SendInfoMessage("Arena: you must specify a player name and a team id");
                    args.Player.SendInfoMessage("Arena: /aassign [player] [1|2] ");
                }
                else
                {
                    string pname = args.Parameters[0];
                    int teamid = int.MinValue;
                    int.TryParse(args.Parameters[1], out teamid);
                    TPPlayer tp = tPulse.GetPlayerByName(pname);

                    if (tp == null)
                    {
                        args.Player.SendErrorMessage(String.Format("Arena: {0} not found!", pname));
                    }
                    else
                    {
                        if (IsInTeam(tp))
                        {
                            args.Player.SendErrorMessage(String.Format("Arena: {0} is already assigned to a team!", pname));
                            return;
                        }

                        if (teamid == 1)
                        {
                            Team1.Add(tp);
                            args.Player.SendInfoMessage(String.Format("Arena: {0} added to the team 1", pname));
                        }
                        else if (teamid == 2)
                        {
                            Team2.Add(tp);
                            args.Player.SendInfoMessage(String.Format("Arena: {0} added to the team 2", pname));
                        }
                        else
                        {
                            args.Player.SendErrorMessage("Arena: invalid team id!");
                        }
                    }
                }
            }
            else
            {
                args.Player.SendErrorMessage("Only superadmin can use this command!");
            }
        }

        protected void PrepareBattle()
        {
            Utils.Broadcast("Arena: A battle is preparing!");

            foreach (TPPlayer p in Team1)
            {
                p.Teleport(Setting.TeamPrepare1.X, Setting.TeamPrepare1.Y);
                p.TPlayer.HealEffect(400);
            }

            foreach (TPPlayer p in Team2)
            {
                p.Teleport(Setting.TeamPrepare2.X, Setting.TeamPrepare2.Y);
                p.TPlayer.HealEffect(400);
            }

            State = ArenaState.Preparation;
            MessageParticipant(String.Format("Battle starting in {0} seconds", Setting.PreparationTime / 1000));
            timer.Reset();
            timer.Start();
        }

        protected void PreparePlayers(CommandArgs args)
        {
            if (args.Player.Group.ContainsGroup("superadmin") && State == ArenaState.Idle)
            {
                PrepareBattle();
            }
        }

        protected void SetPrepare(CommandArgs args)
        {
            if (args.Player.Group.ContainsGroup("superadmin"))
            {
                if (args.Parameters.Count == 0)
                {
                    args.Player.SendInfoMessage("You must specify team index [1|2] after /asetprepare");
                }
                else
                {
                    int teamid = int.MinValue;
                    int.TryParse(args.Parameters[0], out teamid);

                    if (teamid == 1)
                    {

                        Setting.TeamPrepare1.X = args.Player.TileX;
                        Setting.TeamPrepare1.Y = args.Player.TileY;
                        args.Player.SendInfoMessage(String.Format("Arena: Team 1 Prepare position set at : [{0}, {1}]", Setting.TeamPrepare1.X, Setting.TeamPrepare1.Y));
                        Setting.Save(ArenaSettingFile);
                    }
                    else if (teamid == 2)
                    {
                        Setting.TeamPrepare2.X = args.Player.TileX;
                        Setting.TeamPrepare2.Y = args.Player.TileY;
                        args.Player.SendInfoMessage(String.Format("Arena: Team 2 Prepare position set at : [{0}, {1}]", Setting.TeamPrepare2.X, Setting.TeamPrepare2.Y));
                        Setting.Save(ArenaSettingFile);
                    }
                    else
                    {
                        args.Player.SendErrorMessage("Arena: Invalid team id!");
                    }

                }


            }
            else
            {
                args.Player.SendErrorMessage("Only superadmin can use this command!");
            }
        }

        protected void SetAterMatch(CommandArgs args)
        {
            if (args.Player.Group.ContainsGroup("superadmin"))
            {
                Setting.AfterMatchRoom.X = args.Player.TileX;
                Setting.AfterMatchRoom.Y = args.Player.TileY;
                args.Player.SendInfoMessage(String.Format("Arena: After Match Room setted at [{0}, {1}]", Setting.AfterMatchRoom.X, Setting.AfterMatchRoom.Y));
                Setting.Save(ArenaSettingFile);
            }
            else
            {
                args.Player.SendErrorMessage("Only superadmin can use this command!");
            }
        }

        protected void SetArenaHall(CommandArgs args)
        {
            if (args.Player.Group.ContainsGroup("superadmin"))
            {
                Setting.ArenaHall.X = args.Player.TileX;
                Setting.ArenaHall.Y = args.Player.TileY;
                args.Player.SendInfoMessage(String.Format("Arena: Arena hall setted at [{0}, {1}]", Setting.ArenaHall.X, Setting.ArenaHall.Y));
                Setting.Save(ArenaSettingFile);
            }
            else
            {
                args.Player.SendErrorMessage("Only superadmin can use this command!");
            }
        }

        protected void SetTeamStart(CommandArgs args)
        {
            if (args.Player.Group.ContainsGroup("superadmin"))
            {
                if (args.Parameters.Count == 0)
                {
                    args.Player.SendInfoMessage("You must specify team index [1|2] after /asetstart");
                }
                else
                {
                    int teamid = int.MinValue;
                    int.TryParse(args.Parameters[0], out teamid);

                    if (teamid == 1)
                    {

                        Setting.TeamStart1.X = args.Player.TileX;
                        Setting.TeamStart1.Y = args.Player.TileY;
                        args.Player.SendInfoMessage(String.Format("Arena: Team 1 Starting position set at : [{0}, {1}]", Setting.TeamStart1.X, Setting.TeamStart1.Y));
                        Setting.Save(ArenaSettingFile);
                    }
                    else if (teamid == 2)
                    {
                        Setting.TeamStart2.X = args.Player.TileX;
                        Setting.TeamStart2.Y = args.Player.TileY;
                        args.Player.SendInfoMessage(String.Format("Arena: Team 2 Starting position set at : [{0}, {1}]", Setting.TeamStart2.X, Setting.TeamStart2.Y));
                        Setting.Save(ArenaSettingFile);
                    }
                    else
                    {
                        args.Player.SendErrorMessage("Arena: Invalid team id!");
                    }

                }


            }
            else
            {
                args.Player.SendErrorMessage("Only superadmin can use this command!");
            }
        }

    }
}

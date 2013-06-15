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
            Battle
        }

        protected TPulse tPulse;
        protected ArenaSetting Setting;
        protected string ArenaSettingFile = TPulsePaths.Combine(TPulsePath.SavePath, "arena.xml");
        protected List<TPPlayer> Team1 = new List<TPPlayer>();
        protected List<TPPlayer> Team2 = new List<TPPlayer>();
        protected bool InBattle = false;
        protected ArenaState State = ArenaState.Idle;

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
            tPulse.Commands.ChatCommands.Add(new Command("", ScanSwitch, "ascan"));

            Hooks.NetHooks.SendData += new Hooks.NetHooks.SendDataD(NetHooks_SendData);
            Hooks.GameHooks.Update += new Action(GameHooks_Update);
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

        protected void NetHooks_SendData(Hooks.SendDataEventArgs e)
        {
            if (e.MsgID == PacketTypes.PlayerDamage)
            {
                if (IsParticipantId(e.number))
                {
                    Console.WriteLine("PlayerDamage Packet");
                    Console.WriteLine((int)e.number3);
                    Console.WriteLine((int)e.number4);
                    Console.WriteLine(e.text);
                }
            }
            else if (e.MsgID == PacketTypes.PlayerHp)
            {
                TPPlayer player = tPulse.Players[e.number];
                if (IsParticipantId(e.number))
                {
                    Console.WriteLine("PlayerHP : "+player.Name);
                    Console.WriteLine((short)e.number2);
                    Console.WriteLine((short)e.number3);
                }
            }
            else if (e.MsgID == PacketTypes.PlayerKillMe)
            {
                TPPlayer player = tPulse.Players[e.number];
                if (IsParticipantId(e.number))
                {
                    Console.WriteLine("PlayerHP : " + player.Name);

                    Console.WriteLine((short)e.number3);
                    Console.WriteLine(e.text);
                }
            }
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

            foreach (TPPlayer p in Team1)
            {
                p.Teleport(Setting.TeamStart1.X, Setting.TeamStart1.Y);
                p.SetTeam(0);
                p.TPlayer.StatusPvP(122, p.Index);
                
            }

            foreach (TPPlayer p in Team2)
            {
                p.Teleport(Setting.TeamStart2.X, Setting.TeamStart2.Y);
                p.SetTeam(1);
                p.TPlayer.StatusPvP(122, p.Index);
            }
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
                            MessageParticipant(String.Format("Battle starting in {0}", (Setting.PreparationTime / 1000) - sec));
                            SecMessageSended.Add(sec);
                        }
                    }
                }
                else
                {
                    SecMessageSended.Clear();
                    State = ArenaState.Battle;
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
                foreach (TPPlayer p in Team2)
                {
                    if (p.TPlayer.statLife == 0)
                    {
                        MessageParticipant(String.Format("{0} is dead, {1} has won !",p.Name,"Team 1"));
                    }
                }
            }
        }


        protected void ScanSwitch(CommandArgs args)
        {
            if (args.Player.Group.ContainsGroup("superadmin"))
            {
                List<APoint> pts = new List<APoint>();
                //scanning 20 tiles in x, and 5 in y for switch
                int sx = args.Player.TileX;
                int sy = args.Player.TileY;

                for (int i = sx; i < sx + 20; i++)
                {
                    if (i < Main.maxTilesX)
                    {
                        for (int j = sy; j < sy + 10; j++)
                        {
                            if (j < Main.maxTilesY)
                            {
                                Tile t = Main.tile[i, j];
                                if (t.type == (int)BlockType.Switch || t.type == (int)BlockType.Lever)
                                {
                                    APoint pt = new APoint(i, j);
                                    Setting.BattleSwitch.Add(pt);
                                    pts.Add(pt);
                                }
                                else if (t.type == (int)BlockType.XSecondTimer)
                                {
                                    APoint pt = new APoint(i, j);
                                    Setting.BattleSwitch.Add(pt);
                                    pts.Add(pt);
                                }
                                else if (t.type == (int)BlockType.ActiveStone)
                                {
                                    stones.Add(new APoint(i, j));
                                }

                            }
                        }
                    }
                }

                args.Player.SendInfoMessage(String.Format("Arena: {0} switches found!", pts.Count));

                foreach (APoint ap in pts)
                {
                    args.Player.SendInfoMessage(String.Format("Arena: [{0}, {1}]", ap.X, ap.Y));
                }
            }
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

        protected void PreparePlayers(CommandArgs args)
        {
            if (args.Player.Group.ContainsGroup("superadmin") && State == ArenaState.Idle)
            {
                Utils.Broadcast("Arena: A battle is preparing!");

                foreach (TPPlayer p in Team1)
                {
                    p.Teleport(Setting.TeamPrepare1.X, Setting.TeamPrepare1.Y);
                }

                foreach (TPPlayer p in Team2)
                {
                    p.Teleport(Setting.TeamPrepare2.X, Setting.TeamPrepare2.Y);
                }

                State = ArenaState.Preparation;
                MessageParticipant(String.Format("Battle starting in {0} seconds", Setting.PreparationTime/1000));
                timer.Reset();
                timer.Start();
            }

            //starting a timer
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

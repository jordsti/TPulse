﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Terraria;
namespace TPulseAPI
{

    public class TPServerPlayer : TPPlayer
    {
        public TPServerPlayer(float[] rgb, string prefix, string suffix)
            : base("Server")
        {
            Group = new SuperAdminGroup(rgb, prefix, suffix);
        }

        public override void SendErrorMessage(string msg)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine(msg);
            Console.ResetColor();
        }

        public override void SendInfoMessage(string msg)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine(msg);
            Console.ResetColor();
        }

        public override void SendSuccessMessage(string msg)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine(msg);
            Console.ResetColor();
        }

        public override void SendWarningMessage(string msg)
        {
            Console.ForegroundColor = ConsoleColor.DarkRed;
            Console.WriteLine(msg);
            Console.ResetColor();
        }

        public override void SendMessage(string msg)
        {
            SendMessage(msg, 0, 255, 0);
        }

        public override void SendMessage(string msg, Color color)
        {
            SendMessage(msg, color.R, color.G, color.B);
        }

        public override void SendMessage(string msg, byte red, byte green, byte blue)
        {
            Console.WriteLine(msg);
            //RconHandler.Response += msg + "\n";
        }

        public void SetFullMoon(bool fullmoon)
        {
            Main.moonPhase = 0;
            SetTime(false, 0);
        }

        public void SetBloodMoon(bool bloodMoon)
        {
            Main.bloodMoon = bloodMoon;
            SetTime(false, 0);
        }

        public void SetTime(bool dayTime, double time)
        {
            Main.dayTime = dayTime;
            Main.time = time;
            NetMessage.SendData((int)PacketTypes.TimeSet, -1, -1, "", 0, 0, Main.sunModY, Main.moonModY);
            NetMessage.syncPlayers();
        }

        public void SpawnNPC(int type, string name, int amount, int startTileX, int startTileY, int tileXRange = 100,
                             int tileYRange = 50)
        {
            for (int i = 0; i < amount; i++)
            {
                int spawnTileX;
                int spawnTileY;
                MapTools.GetRandomClearTileWithInRange(startTileX, startTileY, tileXRange, tileYRange, out spawnTileX,
                                                           out spawnTileY);
                int npcid = NPC.NewNPC(spawnTileX * 16, spawnTileY * 16, type, 0);
                // This is for special slimes
                Main.npc[npcid].SetDefaults(name);
            }
        }

        public void StrikeNPC(int npcid, int damage, float knockBack, int hitDirection)
        {
            Main.npc[npcid].StrikeNPC(damage, knockBack, hitDirection);
            NetMessage.SendData((int)PacketTypes.NpcStrike, -1, -1, "", npcid, damage, knockBack, hitDirection);
        }

        public void RevertTiles(Dictionary<Vector2, TileData> tiles)
        {
            // Update Main.Tile first so that when tile sqaure is sent it is correct
            foreach (KeyValuePair<Vector2, TileData> entry in tiles)
            {
                Main.tile[(int)entry.Key.X, (int)entry.Key.Y].Data = entry.Value;
            }
            // Send all players updated tile sqaures
            foreach (Vector2 coords in tiles.Keys)
            {
                All.SendTileSquare((int)coords.X, (int)coords.Y, 3);
            }
        }
    }
}

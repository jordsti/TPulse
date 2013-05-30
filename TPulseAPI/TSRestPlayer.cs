using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Terraria;

namespace TPulseAPI
{
    public class TSRestPlayer : TPServerPlayer
    {
        internal List<string> CommandReturn = new List<string>();

        //That bizarre... need to reread this stack
        public TSRestPlayer(ConfigFile config) : base(config.SuperAdminChatRGB, config.SuperAdminChatPrefix, config.SuperAdminChatSuffix)
        {
            Group = new SuperAdminGroup(config.SuperAdminChatRGB, config.SuperAdminChatPrefix, config.SuperAdminChatSuffix);
            AwaitingResponse = new Dictionary<string, Action<object>>();
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
            CommandReturn.Add(msg);
        }

        public List<string> GetCommandOutput()
        {
            return CommandReturn;
        }
    }
}

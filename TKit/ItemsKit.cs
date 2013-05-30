using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Terraria;
using TPulseAPI;
namespace TKit
{
    public class ItemsKit
    {
        public List<TItemStack> Items { get; set; }

        public ItemsKit()
        {
            Items = new List<TItemStack>();
        }
    }
}

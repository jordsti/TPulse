using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Terraria;
namespace TPulseAPI
{
    public class GameResources
    {
        public GameResources()
        { }

        /// <summary>
        /// The lowest id for a prefix.
        /// </summary>
        private const int FirstItemPrefix = 1;

        /// <summary>
        /// The highest id for a prefix.
        /// </summary>
        private const int LastItemPrefix = 83;

        /// <summary>
        /// Gets a list of items by ID or name
        /// </summary>
        /// <param name="idOrName">Item ID or name</param>
        /// <returns>List of Items</returns>
        public static List<Item> GetItemByIdOrName(string idOrName)
        {
            int type = -1;
            if (int.TryParse(idOrName, out type))
            {
                if (type >= Main.maxItemTypes)
                    return new List<Item>();
                return new List<Item> { GetItemById(type) };
            }
            return GetItemByName(idOrName);
        }

        /// <summary>
        /// Gets an item by ID
        /// </summary>
        /// <param name="id">ID</param>
        /// <returns>Item</returns>
        /// 
        public static Item GetItemById(int id)
        {
            Item item = new Item();
            item.netDefaults(id);
            return item;
        }

        /// <summary>
        /// Gets items by name
        /// </summary>
        /// <param name="name">name</param>
        /// <returns>List of Items</returns>
        public static List<Item> GetItemByName(string name)
        {
            var found = new List<Item>();
            Item item = new Item();
            string nameLower = name.ToLower();
            for (int i = -24; i < Main.maxItemTypes; i++)
            {
                item.netDefaults(i);
                if (item.name.ToLower() == nameLower)
                    return new List<Item> { item };
                if (item.name.ToLower().StartsWith(nameLower))
                    found.Add((Item)item.Clone());
            }
            return found;
        }

        /// <summary>
        /// Gets an NPC by ID or Name
        /// </summary>
        /// <param name="idOrName"></param>
        /// <returns>List of NPCs</returns>
        public static List<NPC> GetNPCByIdOrName(string idOrName)
        {
            int type = -1;
            if (int.TryParse(idOrName, out type))
            {
                if (type >= Main.maxNPCTypes)
                    return new List<NPC>();
                return new List<NPC> { GetNPCById(type) };
            }
            return GetNPCByName(idOrName);
        }

        /// <summary>
        /// Gets an NPC by ID
        /// </summary>
        /// <param name="id">ID</param>
        /// <returns>NPC</returns>
        public static NPC GetNPCById(int id)
        {
            NPC npc = new NPC();
            npc.netDefaults(id);
            return npc;
        }

        /// <summary>
        /// Gets a NPC by name
        /// </summary>
        /// <param name="name">Name</param>
        /// <returns>List of matching NPCs</returns>
        public static List<NPC> GetNPCByName(string name)
        {
            var found = new List<NPC>();
            NPC npc = new NPC();
            string nameLower = name.ToLower();
            for (int i = -17; i < Main.maxNPCTypes; i++)
            {
                npc.netDefaults(i);
                if (npc.name.ToLower() == nameLower)
                    return new List<NPC> { npc };
                if (npc.name.ToLower().StartsWith(nameLower))
                    found.Add((NPC)npc.Clone());
            }
            return found;
        }

        /// <summary>
        /// Gets a buff name by id
        /// </summary>
        /// <param name="id">ID</param>
        /// <returns>name</returns>
        public static string GetBuffName(int id)
        {
            return (id > 0 && id < Main.maxBuffs) ? Main.buffName[id] : "null";
        }

        /// <summary>
        /// Gets the description of a buff
        /// </summary>
        /// <param name="id">ID</param>
        /// <returns>description</returns>
        public static string GetBuffDescription(int id)
        {
            return (id > 0 && id < Main.maxBuffs) ? Main.buffTip[id] : "null";
        }

        /// <summary>
        /// Gets a list of buffs by name
        /// </summary>
        /// <param name="name">name</param>
        /// <returns>Matching list of buff ids</returns>
        public static List<int> GetBuffByName(string name)
        {
            string nameLower = name.ToLower();
            for (int i = 1; i < Main.maxBuffs; i++)
            {
                if (Main.buffName[i].ToLower() == nameLower)
                    return new List<int> { i };
            }
            var found = new List<int>();
            for (int i = 1; i < Main.maxBuffs; i++)
            {
                if (Main.buffName[i].ToLower().StartsWith(nameLower))
                    found.Add(i);
            }
            return found;
        }

        /// <summary>
        /// Gets a prefix based on its id
        /// </summary>
        /// <param name="id">ID</param>
        /// <returns>Prefix name</returns>
        public static string GetPrefixById(int id)
        {
            var item = new Item();
            item.SetDefaults(0);
            item.prefix = (byte)id;
            item.AffixName();
            return item.name.Trim();
        }

        /// <summary>
        /// Gets a list of prefixes by name
        /// </summary>
        /// <param name="name">Name</param>
        /// <returns>List of prefix IDs</returns>
        public static List<int> GetPrefixByName(string name)
        {
            Item item = new Item();
            item.SetDefaults(0);
            string lowerName = name.ToLower();
            var found = new List<int>();
            for (int i = FirstItemPrefix; i <= LastItemPrefix; i++)
            {
                item.prefix = (byte)i;
                string prefixName = item.AffixName().Trim().ToLower();
                if (prefixName == lowerName)
                    return new List<int>() { i };
                else if (prefixName.StartsWith(lowerName)) // Partial match
                    found.Add(i);
            }
            return found;
        }

        /// <summary>
        /// Gets a prefix by ID or name
        /// </summary>
        /// <param name="idOrName">ID or name</param>
        /// <returns>List of prefix IDs</returns>
        public static List<int> GetPrefixByIdOrName(string idOrName)
        {
            int type = -1;
            if (int.TryParse(idOrName, out type) && type >= FirstItemPrefix && type <= LastItemPrefix)
            {
                return new List<int> { type };
            }
            return GetPrefixByName(idOrName);
        }
    }
}

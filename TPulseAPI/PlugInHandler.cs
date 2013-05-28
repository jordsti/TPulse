using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Terraria;

namespace TPulseAPI
{
    public static class PlugInHandler
    {
        private static List<TerrariaPlugin> LoadedPlugins = new List<TerrariaPlugin>();

        public static void AddPlugIn(TerrariaPlugin plugin)
        {
            LoadedPlugins.Add(plugin);
        }

        public static void RemovePlugIn(TerrariaPlugin plugin)
        {
            LoadedPlugins.Remove(plugin);
        }

        public static bool Contains(Type type)
        {
            foreach (TerrariaPlugin tp in LoadedPlugins)
            {
                if (type.IsInstanceOfType(tp))
                {
                    return true;
                }
            }

            return false;
        }

        public static List<Type> GetAllLoadedTypes()
        {
            List<Type> types = new List<Type>();

            foreach (TerrariaPlugin tp in LoadedPlugins)
            {
                types.Add(tp.GetType());
            }

            return types;
        }



        public static TerrariaPlugin GetPluginByType(Type type)
        {
            foreach (TerrariaPlugin tp in LoadedPlugins)
            {
                if (type.IsInstanceOfType(tp))
                {
                    return tp;
                }
            }

            return null;
        }

    }
}

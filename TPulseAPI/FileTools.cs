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
using System.IO;

namespace TPulseAPI
{
	public class FileTools
	{
        /// <summary>
        /// Path to the file containing the rules.
        /// </summary>
		internal static string RulesPath
		{
			get { return Path.Combine(TPulse.SavePath, "rules.txt"); }
		}

        /// <summary>
        /// Path to the file containing the message of the day.
        /// </summary>
		internal static string MotdPath
		{
			get { return Path.Combine(TPulse.SavePath, "motd.txt"); }
		}

        /// <summary>
        /// Path to the file containing the whitelist.
        /// </summary>
		internal static string WhitelistPath
		{
			get { return Path.Combine(TPulse.SavePath, "whitelist.txt"); }
		}

        /// <summary>
        /// Path to the file containing the config.
        /// </summary>
		internal static string ConfigPath
		{
			get { return Path.Combine(TPulse.SavePath, "config.json"); }
		}

        /// <summary>
        /// Creates an empty file at the given path.
        /// </summary>
        /// <param name="file">The path to the file.</param>
		public static void CreateFile(string file)
		{
			File.Create(file).Close();
		}

        /// <summary>
        /// Creates a file if the files doesn't already exist.
        /// </summary>
        /// <param name="file">The path to the files</param>
        /// <param name="data">The data to write to the file.</param>
		public static void CreateIfNot(string file, string data = "")
		{
			if (!File.Exists(file))
			{
				File.WriteAllText(file, data);
			}
		}

		/// <summary>
		/// Sets up the configuration file for all variables, and creates any missing files.
		/// </summary>
        [Obsolete("Should be move into TPulse")]
        public static void SetupConfig(TPulse tPulse)
		{
			if (!Directory.Exists(TPulse.SavePath))
			{
				Directory.CreateDirectory(TPulse.SavePath);
			}

			CreateIfNot(RulesPath, "Respect the admins!\nDon't use TNT!");
			CreateIfNot(MotdPath,
			            "This server is running TPulse for Terraria.\n Type /help for a list of commands.\n%255,000,000%Current map: %map%\nCurrent players: %players%");
			CreateIfNot(WhitelistPath);
			if (File.Exists(ConfigPath))
			{
				tPulse.Config = ConfigFile.Read(ConfigPath);
				// Add all the missing config properties in the json file
			}
			tPulse.Config.Write(ConfigPath);

		}

		/// <summary>
		/// Tells if a user is on the whitelist
		/// </summary>
		/// <param name="ip">string ip of the user</param>
		/// <returns>true/false</returns>
        [Obsolete("Should be move into TPulse")]
		public static bool OnWhitelist(string ip, TPulse tPulse)
		{
			if (!tPulse.Config.EnableWhitelist)
			{
				return true;
			}
			CreateIfNot(WhitelistPath, "127.0.0.1");
			using (var tr = new StreamReader(WhitelistPath))
			{
				string whitelist = tr.ReadToEnd();
				ip = Utils.GetRealIP(ip);
				bool contains = whitelist.Contains(ip);
				if (!contains)
				{
					foreach (var line in whitelist.Split(Environment.NewLine.ToCharArray()))
					{
						if (string.IsNullOrWhiteSpace(line))
							continue;
						contains = Utils.GetIPv4Address(line).Equals(ip);
						if (contains)
							return true;
					}
					return false;
				}
				return true;
			}
		}
	}
}
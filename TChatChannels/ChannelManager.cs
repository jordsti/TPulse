using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.IO;
using TPulseAPI;

namespace TChatChannels
{
    public class ChannelManager
    {
        public const String DefaultChannelsFile = "channels.txt";

        public String ChannelsFile { get; protected set; }

        protected List<Channel> Channels = new List<Channel>();

        public Channel DefaultChannel { get; protected set; }

        public ChannelManager(string channelsFile)
        {
            ChannelsFile = TPulsePaths.Combine(TPulsePath.SavePath, channelsFile);
            DefaultChannel = null;
            LoadChannelsFile();
        }

        public ChannelManager()
            : this(DefaultChannelsFile)
        {

        }

        public List<Channel> ChatChannels
        {
            get
            {
                List<Channel> channels = new List<Channel>();
                channels.AddRange(Channels);
                return channels;
            }
        }

        public bool Contains(string name)
        {
            foreach (Channel c in Channels)
            {
                if (c.Name == name)
                    return true;
            }

            return false;
        }

        public bool Add(Channel c)
        {
            if (!Contains(c.Name))
            {
                Channels.Add(c);
                return true;
            }

            return false;
        }

        public bool Remove(string name)
        {
            Channel toRemove = null;

            foreach (Channel c in Channels)
            {
                if (c.Name == name)
                {
                    toRemove = c;
                    break;
                }
            }

            if (toRemove != null)
            {
                Channels.Remove(toRemove);
                return true;
            }

            return false;
        }

        private static Regex reChannel = new Regex("(?<name>[A-Za-z0-9]+) r:(?<r>[0-9]{1,3}) g:(?<g>[0-9]{1,3}) b:(?<b>[0-9]{1,3})");

        private void LoadChannelsFile()
        {
            if (File.Exists(ChannelsFile))
            {
                StreamReader reader = new StreamReader(ChannelsFile);

                string data = reader.ReadToEnd();

                reader.Close();

                string[] lines = data.Split('\n');
                int i = 1;
                foreach (string line in lines)
                {
                    string tline = line.TrimEnd(new char[] { '\n', '\r' });

                    if (!tline.StartsWith("#"))
                    {
                        Match m = reChannel.Match(tline);

                        if (m.Success)
                        {
                            string cname = m.Groups["name"].Value;
                            byte r = byte.Parse(m.Groups["r"].Value);
                            byte g = byte.Parse(m.Groups["g"].Value);
                            byte b = byte.Parse(m.Groups["b"].Value);

                            Channel c = new Channel(cname, r, g, b);

                            Channels.Add(c);
                        }
                        else if(tline.Length > 0)
                        {
                            //Need to write this into a LogFile
                            //IMPORTANT
                            Console.WriteLine(String.Format("Channels: Invalid line ({1}) into {0};", ChannelsFile, i));
                        }
                    }

                    i++;
                }

                if (DefaultChannel == null && Channels.Count >= 1)
                {
                    DefaultChannel = Channels[0];
                }
            }
            else
            {
                //Create default file
                StreamWriter writer = new StreamWriter(ChannelsFile);

                writer.WriteLine("#TChatChannels Configuration files");
                writer.WriteLine("#You can specify the wanted channels here");
                writer.WriteLine("#First channel defined is the default one");
                writer.WriteLine("#[ChannelName] r:[red] g:[green] b:[blue]");
                writer.WriteLine("General r:175 g:224 b:27");
                writer.Close();

                LoadChannelsFile();
            }
            
        }


        public void Save()
        {
            List<String> comments = new List<string>();
            //need to fix this to keep comments
            FileStream fstream = File.OpenRead(ChannelsFile);

            StreamReader reader = new StreamReader(fstream);

            string line = reader.ReadLine();

            while (line.Length != 0)
            {
                string tline = line.Trim();

                if (tline.StartsWith("#"))
                {
                    comments.Add(tline);
                }

                line = reader.ReadLine();
            }

            reader.Close();


            StreamWriter writer = new StreamWriter(File.OpenWrite(ChannelsFile));

            foreach (String c in comments)
            {
                writer.WriteLine(c);
            }

            foreach (Channel c in Channels)
            {
                writer.WriteLine(c.ToString());
                Console.WriteLine(c.ToString());
            }

            Console.WriteLine("flushing");
            writer.Flush();
            writer.Close();
        }
    }
}

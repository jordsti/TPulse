using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace TChatChannels
{
    public class ChatLogger
    {
        public String FilePath { get; protected set; }

        public ChatLogger(String path)
        {
            FilePath = path;

            CreateDirectory(path);
        }

        protected void CreateDirectory(String path)
        {
            path = Path.GetDirectoryName(path);
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
        }

        public void Write(string message)
        {
            StreamWriter writer = File.AppendText(FilePath);

            writer.WriteLine(String.Format("{0} - {1}", DateTime.Now.ToString(), message));
            writer.Flush();
            writer.Close();
        }
    }
}

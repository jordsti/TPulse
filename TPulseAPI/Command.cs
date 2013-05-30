using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TPulseAPI
{
    public class Command
    {
        public string Name
        {
            get { return Names[0]; }
        }

        public List<string> Names { get; protected set; }
        public bool AllowServer { get; set; }
        public bool DoLog { get; set; }
        public string Permission { get; protected set; }
        private CommandDelegate command;

        public Command(string permissionneeded, CommandDelegate cmd, params string[] names)
            : this(cmd, names)
        {
            Permission = permissionneeded;
        }

        public Command(CommandDelegate cmd, params string[] names)
        {
            if (names == null || names.Length < 1)
                throw new NotSupportedException();
            Permission = null;
            Names = new List<string>(names);
            command = cmd;
            AllowServer = true;
            DoLog = true;
        }

        public bool Run(string msg, TPPlayer ply, List<string> parms)
        {
            if (!ply.Group.HasPermission(Permission))
                return false;

            try
            {
                command(new CommandArgs(msg, ply, parms));
            }
            catch (Exception e)
            {
                ply.SendErrorMessage("Command failed, check logs for more details.");
                Log.Error(e.ToString());
            }

            return true;
        }

        public bool HasAlias(string name)
        {
            return Names.Contains(name);
        }

        public bool CanRun(TPPlayer ply)
        {
            return ply.Group.HasPermission(Permission);
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace TPulseAPI
{
    public enum TPulsePath
    {
        SavePath,
        ProcessFile,
        SqliteFile,
        BackupPath,
        GeoIPFile,
        AuthCodeFile,
        AuthLockFile
    }

    public static class TPulsePaths
    {
        private const String SavePath = "tpulse";
        private const String ProcessFile = "tpulse.pid";
        private const String SqliteFile = "tpulse.sqlite";

        public static string Combine(TPulsePath path, string file)
        {
            return Path.Combine(GetPath(path), file);
        }

        public static string GetPath(TPulsePath path)
        {
            if (path == TPulsePath.SavePath)
            {
                return SavePath;
            }
            else if (path == TPulsePath.ProcessFile)
            {
                return Path.Combine(SavePath, ProcessFile);
            }
            else if (path == TPulsePath.SqliteFile)
            {
                return Path.Combine(SavePath, SqliteFile);
            }
            else if (path == TPulsePath.BackupPath)
            {
                return Path.Combine(SavePath, "backups");
            }
            else if (path == TPulsePath.GeoIPFile)
            {
                return Path.Combine(SavePath, "GeoIP.dat");
            }
            else if (path == TPulsePath.AuthCodeFile)
            {
                return Path.Combine(SavePath, "authcode.txt");
            }
            else if (path == TPulsePath.AuthLockFile)
            {
                return Path.Combine(SavePath, "auth.lck");
            }
            else
            {
                return "";
            }
        }

    }
}

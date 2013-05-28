using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Runtime.Serialization;
using TPulseAPI;

namespace TFriends
{
    [Serializable]
    public class FriendsDB
    {
        public static string DefaultFile { get; protected set; }

        protected List<FriendList> FLists = new List<FriendList>();

        public FriendsDB()
        {
            DefaultFile = TPulsePaths.Combine(TPulsePath.SavePath, "friends.db");
        }

        protected static BinaryFormatter bf = new BinaryFormatter();

        public static FriendsDB Load(string path)
        {
            FriendsDB db = new FriendsDB();

            bf.AssemblyFormat = System.Runtime.Serialization.Formatters.FormatterAssemblyStyle.Simple;
            FileStream fin = File.OpenRead(path);

            db = (FriendsDB)bf.Deserialize(fin);

            fin.Close();

            return db;
        }

        public void Save(string path)
        {
            bf.AssemblyFormat = System.Runtime.Serialization.Formatters.FormatterAssemblyStyle.Simple;

            FileStream fout = File.Create(path);

            bf.Serialize(fout, this);

            fout.Close();
        }

        public FriendList GetListByUsername(string uname)
        {
            foreach (FriendList fl in FLists)
            {
                if (fl.User.Name == uname)
                    return fl;
            }

            return null;
        }

        public void Add(FriendList fl)
        {
            if (this[fl.User.Name] == null)
            {
                FLists.Add(fl);
            }
        }

        public FriendList GetListByUserID(int id)
        {
            foreach (FriendList fl in FLists)
            {
                if (fl.User.ID == id)
                    return fl;
            }

            return null;
        }

        public FriendList this[int id]
        {
            get
            {
                return GetListByUserID(id);
            }
        }

        public FriendList this[string uname]
        {
            get
            {
                return GetListByUsername(uname);
            }
        }
    }
}

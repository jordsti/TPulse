using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;
using TPulseAPI;
using TPulseAPI.DB;

namespace TFriends
{
    [Serializable]
    public class FUser
    {
        public int ID { get; set; }
        public string Name { get; set; }

        public FUser(int id, string name)
        {
            ID = id;
            Name = name;
        }

        public FUser(User u)
        {
            ID = u.ID;
            Name = u.Name;
        }

        public FUser()
            : this(-1, "Unknown")
        {

        }
    }
}

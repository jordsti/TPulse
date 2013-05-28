using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

namespace TFriends
{
    [Serializable]
    public class FriendList
    {
        public FUser User { get; set; }
        public List<FUser> Friends { get; set; }

        public FriendList(FUser user)
        {
            User = user;
            Friends = new List<FUser>();
        }

        public FUser GetUser(string name)
        {
            foreach (FUser u in Friends)
            {
                if (u.Name == name)
                    return u;
            }

            return null;
        }

        public FUser GetUser(int order)
        {
            if (order >= 0 && order < Friends.Count)
            {
                return Friends[order];
            }
            return null;
        }

        public void RemoveUserByName(string name)
        {
            FUser user = null;
            foreach(FUser u in Friends)
            {
                if(u.Name == name)
                {
                    user = u;
                    break;
                }
            }
            
            if(user != null)
            {
                Friends.Remove(user);
            }
        }

        public bool Contains(string name)
        {
            foreach (FUser fu in Friends)
            {
                if (fu.Name == name)
                    return true;
            }

            return false;

        }

    }
}

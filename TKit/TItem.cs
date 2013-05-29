using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TKit
{
    public class TItemStack
    {
        public String Name { get; protected set; }
        public int Amount { get; protected set; }

        public TItemStack(String name, int amount)
        {
            Name = name;
            Amount = amount;
        }
    }
}

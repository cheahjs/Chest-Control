using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TShockAPI;

namespace ChestControl
{
    public class CPlayer : TSPlayer
    {
        public bool Setting = false;

        public CPlayer(int index)
            : base(index)
        {
        }
    }
}

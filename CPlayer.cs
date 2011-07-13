using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TShockAPI;

namespace ChestControl
{
    public class CPlayer : TSPlayer
    {
        public SettingState State = SettingState.None;

        public CPlayer(int index)
            : base(index)
        {
        }
    }

    public enum SettingState
    {
        None,
        Setting,
        Deleting
    }
}

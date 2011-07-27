using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TShockAPI;

namespace ChestControl
{
    public class CPlayer : TSPlayer
    {
        protected SettingState State = SettingState.None;

        public CPlayer(int index)
            : base(index)
        {
        }


        public SettingState getState()
        {
            return State;
        }

        public void setState(SettingState state)
        {
            State = state;
        }
    }

    public enum SettingState
    {
        None,
        Setting,
        RegionSetting,
        Deleting
    }
}

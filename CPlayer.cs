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
        public string PasswordForChest = "";
        protected int[] UnlockedChests;

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

        public void unlockedChest(int id)
        {
            UnlockedChests[id] = id;
        }


        public bool hasAccessToChest(int id)
        {
            if (UnlockedChests.Contains(id))
            {
                return true;
            }

            return false;
        }
    }

    public enum SettingState
    {
        None,
        Setting,
        RegionSetting,
        PasswordSetting,
        PasswordUnSetting,
        Deleting,
        UnLocking
    }
}

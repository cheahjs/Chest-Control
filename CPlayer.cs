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
        protected List<int> UnlockedChests = new List<int>();

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
            UnlockedChests.Add(id);
            PasswordForChest = "";
        }


        public bool hasAccessToChest(int id)
        {
            
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

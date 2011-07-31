using TShockAPI;
using System.Collections.Generic;

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
        PublicSetting,
        PasswordSetting,
        PasswordUnSetting,
        Deleting,
        UnLocking
    }
}

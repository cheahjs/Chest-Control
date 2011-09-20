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


        public SettingState GetState()
        {
            return State;
        }

        public void SetState(SettingState state)
        {
            State = state;
        }

        public void UnlockedChest(int id)
        {
            UnlockedChests.Add(id);
            PasswordForChest = "";
        }


        public bool HasAccessToChest(int id)
        {
            return UnlockedChests.Contains(id);
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
        UnLocking,
        RefillSetting,
        RefillUnSetting
    }
}

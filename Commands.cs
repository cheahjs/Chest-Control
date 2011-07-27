using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TShockAPI;

namespace ChestControl
{
    class Commands
    {
        public static void Load()
        {
            TShockAPI.Commands.ChatCommands.Add(new Command("protectchest", Set, "cset", "setchest"));
            TShockAPI.Commands.ChatCommands.Add(new Command("protectchest", SetRegionChest, "crset", "setregionchest"));
            TShockAPI.Commands.ChatCommands.Add(new Command("protectchest", UnSet, "cunset", "unsetchest"));
            TShockAPI.Commands.ChatCommands.Add(new Command("protectchest", CancelSet, "ccset", "ccunset", "cancelsetchest", "cancelunsetchest"));
        }

        private static void Set(CommandArgs args)
        {
            if (ChestControl.Players[args.Player.Index].getState() == SettingState.Setting)
            {
                ChestControl.Players[args.Player.Index].setState(SettingState.None);
                args.Player.SendMessage("You are no longer selecting a chest.", Microsoft.Xna.Framework.Color.BlueViolet);
            }
            else
            {
                ChestControl.Players[args.Player.Index].setState(SettingState.Setting);
                args.Player.SendMessage("Open a chest to protect it.", Microsoft.Xna.Framework.Color.BlueViolet);
            }
        }

        private static void SetRegionChest(CommandArgs args)
        {

            if (ChestControl.Players[args.Player.Index].getState() == SettingState.RegionSetting)
            {
                ChestControl.Players[args.Player.Index].setState(SettingState.None);
                args.Player.SendMessage("You are no longer selecting a chest.", Microsoft.Xna.Framework.Color.BlueViolet);
            }
            else
            {
                ChestControl.Players[args.Player.Index].setState(SettingState.RegionSetting);
                args.Player.SendMessage("Open a chest in region to set it region shareable.", Microsoft.Xna.Framework.Color.BlueViolet);
            }
        }

        private static void UnSet(CommandArgs args)
        {
            if (ChestControl.Players[args.Player.Index].getState() == SettingState.Deleting)
            {
                ChestControl.Players[args.Player.Index].setState(SettingState.None);
                args.Player.SendMessage("You are no longer selecting a chest.", Microsoft.Xna.Framework.Color.BlueViolet);
            }
            else
            {
                ChestControl.Players[args.Player.Index].setState(SettingState.Deleting);
                args.Player.SendMessage("Open a chest to delete it's protection.", Microsoft.Xna.Framework.Color.BlueViolet);
            }
        }

        private static void CancelSet(CommandArgs args)
        {
            ChestControl.Players[args.Player.Index].setState(SettingState.None);
            args.Player.SendMessage("Setting/Unsetting of chest protection canceled.", Microsoft.Xna.Framework.Color.BlueViolet);
        }
    }
}

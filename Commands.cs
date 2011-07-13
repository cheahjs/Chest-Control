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
            TShockAPI.Commands.ChatCommands.Add(new Command("protectchest", UnSet, "cunset", "unsetchest"));
        }

        private static void Set(CommandArgs args)
        {
            ChestControl.Players[args.Player.Index].State = SettingState.Setting;
            args.Player.SendMessage("Open a chest to protect it.", Microsoft.Xna.Framework.Color.BlueViolet);
        }

        private static void UnSet(CommandArgs args)
        {
            ChestControl.Players[args.Player.Index].State = SettingState.Deleting;
            args.Player.SendMessage("Open a chest to delete it's protection.", Microsoft.Xna.Framework.Color.BlueViolet);
        }
    }
}

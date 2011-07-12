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
        }

        private static void Set(CommandArgs args)
        {
            ChestControl.Players[args.Player.Index].Setting = true;
            args.Player.SendMessage("Open a chest to protect it.", Microsoft.Xna.Framework.Color.BlueViolet);
        }
    }
}

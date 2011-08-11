using TShockAPI;
using Microsoft.Xna.Framework;
using System.Collections.Generic;

namespace ChestControl
{
    class Commands
    {
        public static void Load()
        {
            TShockAPI.Commands.ChatCommands.Add(new Command("protectchest", Set, "cset", "setchest"));
            TShockAPI.Commands.ChatCommands.Add(new Command("protectchest", UnSet, "cunset", "unsetchest"));
            TShockAPI.Commands.ChatCommands.Add(new Command("protectchest", CancelSet, "ccset", "ccunset", "cancelsetchest", "cancelunsetchest"));

            TShockAPI.Commands.ChatCommands.Add(new Command("protectchest", SetRegionChest, "crset", "rchest", "regionsharechest"));
            TShockAPI.Commands.ChatCommands.Add(new Command("protectchest", SetPublicChest, "cpset", "pchest", "setpublicchest"));
            TShockAPI.Commands.ChatCommands.Add(new Command("protectchest", SetPasswordChest, "clock", "lockchest", "chestlock"));

            //everyone can unlock
            TShockAPI.Commands.ChatCommands.Add(new Command(UnLockChest, "cunlock", "unlockchest", "chestunlock"));

            //add permissions to db if not exists
            bool perm = false;
            foreach (Group group in TShock.Groups.groups)
            {
                if (group.Name != "superadmin")
                {
                    if (group.HasPermission("protectchest"))
                    {
                        perm = true;
                    }
                }
            }
            if (!perm)
            {
                List<string> permissions = new List<string>();
                permissions.Add("protectchest");
                permissions.Add("openallchests");
                permissions.Add("removechestprotection");
                permissions.Add("showchestinfo");
                TShock.Groups.AddPermissions("trustedadmin", permissions);
            }
        }

        private static void Set(CommandArgs args)
        {
            if (ChestControl.Players[args.Player.Index].getState() == SettingState.Setting || ChestControl.Players[args.Player.Index].getState() == SettingState.PublicSetting)
            {
                ChestControl.Players[args.Player.Index].setState(SettingState.None);
                args.Player.SendMessage("You are no longer selecting a chest.", Color.BlueViolet);
            }
            else
            {
                if (args.Parameters.Count == 1)
                {
                    switch (args.Parameters[0])
                    {
                        case "public":
                            ChestControl.Players[args.Player.Index].setState(SettingState.PublicSetting);
                            args.Player.SendMessage("Open a chest to protect it (public).", Color.BlueViolet);
                            break;

                        case "private":
                            ChestControl.Players[args.Player.Index].setState(SettingState.Setting);
                            args.Player.SendMessage("Open a chest to protect it (private).", Color.BlueViolet);
                            break;

                        default:
                            args.Player.SendMessage("Wrong subcommand, use \"public\" or \"private\".", Color.BlueViolet);
                            break;
                    }
                }
                else
                {
                    ChestControl.Players[args.Player.Index].setState(SettingState.Setting);
                    args.Player.SendMessage("Open a chest to protect it.", Color.BlueViolet);
                }
            }
        }

        private static void SetPasswordChest(CommandArgs args)
        {
            if (ChestControl.Players[args.Player.Index].getState() == SettingState.PasswordSetting || ChestControl.Players[args.Player.Index].getState() == SettingState.PasswordUnSetting)
            {
                ChestControl.Players[args.Player.Index].setState(SettingState.None);
                args.Player.SendMessage("You are no longer selecting a chest.", Color.BlueViolet);
            }
            else
            {
                if (args.Parameters.Count != 1)
                {
                    args.Player.SendMessage("You must enter password! Or use \"remove\" as password to remove password.", Color.Red);
                    return;
                }

                if (args.Parameters[0] == "unset" || args.Parameters[0] == "unlock" || args.Parameters[0] == "remove" || args.Parameters[0] == "rm" || args.Parameters[0] == "delete" || args.Parameters[0] == "del")
                {
                    ChestControl.Players[args.Player.Index].setState(SettingState.PasswordUnSetting);
                    args.Player.SendMessage("Open a chest to remove password.", Color.BlueViolet);
                }
                else
                {
                    ChestControl.Players[args.Player.Index].PasswordForChest = args.Parameters[0];
                    ChestControl.Players[args.Player.Index].setState(SettingState.PasswordSetting);
                    args.Player.SendMessage("Open a chest to set password.", Color.BlueViolet);
                }
            }
        }

        private static void UnLockChest(CommandArgs args)
        {
            if (ChestControl.Players[args.Player.Index].getState() == SettingState.UnLocking)
            {
                ChestControl.Players[args.Player.Index].PasswordForChest = "";
                ChestControl.Players[args.Player.Index].setState(SettingState.None);
                args.Player.SendMessage("You are no longer selecting a chest.", Color.BlueViolet);
            }
            else
            {
                if (args.Parameters.Count != 1)
                {
                    args.Player.SendMessage("You must enter password to unlock chest!", Color.Red);
                    return;
                }

                ChestControl.Players[args.Player.Index].PasswordForChest = args.Parameters[0];
                ChestControl.Players[args.Player.Index].setState(SettingState.UnLocking);
                args.Player.SendMessage("Open a chest to unlock it.", Color.BlueViolet);
            }
        }

        private static void SetRegionChest(CommandArgs args)
        {
            if (ChestControl.Players[args.Player.Index].getState() == SettingState.RegionSetting)
            {
                ChestControl.Players[args.Player.Index].setState(SettingState.None);
                args.Player.SendMessage("You are no longer selecting a chest.", Color.BlueViolet);
            }
            else
            {
                ChestControl.Players[args.Player.Index].setState(SettingState.RegionSetting);
                args.Player.SendMessage("Open a chest in region to set/unset it region shareable.", Color.BlueViolet);
            }
        }

        private static void SetPublicChest(CommandArgs args)
        {
            if (ChestControl.Players[args.Player.Index].getState() == SettingState.PublicSetting)
            {
                ChestControl.Players[args.Player.Index].setState(SettingState.None);
                args.Player.SendMessage("You are no longer selecting a chest.", Color.BlueViolet);
            }
            else
            {
                ChestControl.Players[args.Player.Index].setState(SettingState.PublicSetting);
                args.Player.SendMessage("Open a chest to set/unset it public.", Color.BlueViolet);
            }
        }

        private static void UnSet(CommandArgs args)
        {
            if (ChestControl.Players[args.Player.Index].getState() == SettingState.Deleting)
            {
                ChestControl.Players[args.Player.Index].setState(SettingState.None);
                args.Player.SendMessage("You are no longer selecting a chest.", Color.BlueViolet);
            }
            else
            {
                ChestControl.Players[args.Player.Index].setState(SettingState.Deleting);
                args.Player.SendMessage("Open a chest to delete it's protection.", Color.BlueViolet);
            }
        }

        private static void CancelSet(CommandArgs args)
        {
            ChestControl.Players[args.Player.Index].setState(SettingState.None);
            args.Player.SendMessage("Selection of chest canceled.", Color.BlueViolet);
        }
    }
}

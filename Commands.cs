using System.Collections.Generic;
using System.Linq;
using TShockAPI;

namespace ChestControl
{
    internal static class Commands
    {
        public static void Load()
        {
            //Get ready to update to new Commands when TShock releases the next version
            TShockAPI.Commands.ChatCommands.Add(new Command("protectchest", Set, "cset", "setchest"));
            TShockAPI.Commands.ChatCommands.Add(new Command("protectchest", UnSet, "cunset", "unsetchest"));
            TShockAPI.Commands.ChatCommands.Add(new Command("protectchest", CancelSet, "ccset", "ccunset", "cancelsetchest", "cancelunsetchest"));

            TShockAPI.Commands.ChatCommands.Add(new Command("protectchest", SetRegionChest, "crset", "rchest", "regionsharechest"));
            TShockAPI.Commands.ChatCommands.Add(new Command("protectchest", SetPublicChest, "cpset", "pchest", "setpublicchest"));
            TShockAPI.Commands.ChatCommands.Add(new Command("protectchest", SetPasswordChest, "clock", "lockchest", "chestlock") {DoLog = false});

            TShockAPI.Commands.ChatCommands.Add(new Command("refillchest", SetRefillChest, "crefill", "refillchest", "chestrefill"));

            //everyone can unlock
            TShockAPI.Commands.ChatCommands.Add(new Command(UnLockChest, "cunlock", "unlockchest", "chestunlock") {DoLog = false});

            //add permissions to db if not exists
            bool perm = TShock.Groups.groups.Where(@group => @group.Name != "superadmin").Any(@group => group.HasPermission("protectchest"));
            if (!perm)
            {
                var permissions = new List<string>
                                      {
                                          "protectchest",
                                          "openallchests",
                                          "removechestprotection",
                                          "showchestinfo",
                                          "refillchest"
                                      };
                TShock.Groups.AddPermissions("trustedadmin", permissions);
            }
        }

        private static void Set(CommandArgs args)
        {
            if (ChestControl.Players[args.Player.Index].GetState() == SettingState.Setting ||
                ChestControl.Players[args.Player.Index].GetState() == SettingState.PublicSetting)
            {
                ChestControl.Players[args.Player.Index].SetState(SettingState.None);
                args.Player.SendMessage("You are no longer selecting a chest.", Color.BlueViolet);
            }
            else if (args.Parameters.Count == 1)
                switch (args.Parameters[0])
                {
                    case "public":
                        ChestControl.Players[args.Player.Index].SetState(SettingState.PublicSetting);
                        args.Player.SendMessage("Open a chest to protect it (public).", Color.BlueViolet);
                        break;
                    case "private":
                        ChestControl.Players[args.Player.Index].SetState(SettingState.Setting);
                        args.Player.SendMessage("Open a chest to protect it (private).", Color.BlueViolet);
                        break;
                    default:
                        args.Player.SendMessage("Wrong subcommand, use \"public\" or \"private\".", Color.BlueViolet);
                        break;
                }
            else
            {
                ChestControl.Players[args.Player.Index].SetState(SettingState.Setting);
                args.Player.SendMessage("Open a chest to protect it.", Color.BlueViolet);
            }
        }

        private static void SetPasswordChest(CommandArgs args)
        {
            if (ChestControl.Players[args.Player.Index].GetState() == SettingState.PasswordSetting ||
                ChestControl.Players[args.Player.Index].GetState() == SettingState.PasswordUnSetting)
            {
                ChestControl.Players[args.Player.Index].SetState(SettingState.None);
                args.Player.SendMessage("You are no longer selecting a chest.", Color.BlueViolet);
            }
            else
            {
                if (args.Parameters.Count != 1)
                {
                    args.Player.SendMessage(
                        "You must enter password! Or use \"remove\" as password to remove password.", Color.Red);
                    return;
                }
                if (args.Parameters[0] == "del" || args.Parameters[0] == "delete" || args.Parameters[0] == "rm" ||
                    args.Parameters[0] == "remove" || args.Parameters[0] == "unlock" || args.Parameters[0] == "unset")
                {
                    ChestControl.Players[args.Player.Index].SetState(SettingState.PasswordUnSetting);
                    args.Player.SendMessage("Open a chest to remove password.", Color.BlueViolet);
                }
                else
                {
                    ChestControl.Players[args.Player.Index].PasswordForChest = args.Parameters[0];
                    ChestControl.Players[args.Player.Index].SetState(SettingState.PasswordSetting);
                    args.Player.SendMessage("Open a chest to set password.", Color.BlueViolet);
                }
            }
        }

        private static void SetRefillChest(CommandArgs args)
        {
            if (ChestControl.Players[args.Player.Index].GetState() == SettingState.RefillSetting ||
                ChestControl.Players[args.Player.Index].GetState() == SettingState.PasswordUnSetting)
            {
                ChestControl.Players[args.Player.Index].SetState(SettingState.None);
                args.Player.SendMessage("You are no longer selecting a chest.", Color.BlueViolet);
            }
            else if (args.Parameters.Count == 1)
            {
                if (args.Parameters[0] == "unset" || args.Parameters[0] == "unlock" || args.Parameters[0] == "remove" ||
                    args.Parameters[0] == "rm" || args.Parameters[0] == "delete" || args.Parameters[0] == "del")
                {
                    ChestControl.Players[args.Player.Index].SetState(SettingState.RefillUnSetting);
                    args.Player.SendMessage("Open a chest to remove refill.", Color.BlueViolet);
                }
            }
            else
            {
                ChestControl.Players[args.Player.Index].SetState(SettingState.RefillSetting);
                args.Player.SendMessage("Open a chest to set refill.", Color.BlueViolet);
            }
        }

        private static void UnLockChest(CommandArgs args)
        {
            if (ChestControl.Players[args.Player.Index].GetState() == SettingState.UnLocking)
            {
                ChestControl.Players[args.Player.Index].PasswordForChest = "";
                ChestControl.Players[args.Player.Index].SetState(SettingState.None);
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
                ChestControl.Players[args.Player.Index].SetState(SettingState.UnLocking);
                args.Player.SendMessage("Open a chest to unlock it.", Color.BlueViolet);
            }
        }

        private static void SetRegionChest(CommandArgs args)
        {
            if (ChestControl.Players[args.Player.Index].GetState() == SettingState.RegionSetting)
            {
                ChestControl.Players[args.Player.Index].SetState(SettingState.None);
                args.Player.SendMessage("You are no longer selecting a chest.", Color.BlueViolet);
            }
            else
            {
                ChestControl.Players[args.Player.Index].SetState(SettingState.RegionSetting);
                args.Player.SendMessage("Open a chest in region to set/unset it region shareable.", Color.BlueViolet);
            }
        }

        private static void SetPublicChest(CommandArgs args)
        {
            if (ChestControl.Players[args.Player.Index].GetState() == SettingState.PublicSetting)
            {
                ChestControl.Players[args.Player.Index].SetState(SettingState.None);
                args.Player.SendMessage("You are no longer selecting a chest.", Color.BlueViolet);
            }
            else
            {
                ChestControl.Players[args.Player.Index].SetState(SettingState.PublicSetting);
                args.Player.SendMessage("Open a chest to set/unset it public.", Color.BlueViolet);
            }
        }

        private static void UnSet(CommandArgs args)
        {
            if (ChestControl.Players[args.Player.Index].GetState() == SettingState.Deleting)
            {
                ChestControl.Players[args.Player.Index].SetState(SettingState.None);
                args.Player.SendMessage("You are no longer selecting a chest.", Color.BlueViolet);
            }
            else
            {
                ChestControl.Players[args.Player.Index].SetState(SettingState.Deleting);
                args.Player.SendMessage("Open a chest to delete it's protection.", Color.BlueViolet);
            }
        }

        private static void CancelSet(CommandArgs args)
        {
            ChestControl.Players[args.Player.Index].SetState(SettingState.None);
            args.Player.SendMessage("Selection of chest canceled.", Color.BlueViolet);
        }
    }
}
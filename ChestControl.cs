using Terraria;
using TerrariaAPI;
using TerrariaAPI.Hooks;
using TShockAPI;
using System;
using System.IO;
using Microsoft.Xna.Framework;

namespace ChestControl
{
    [APIVersion(1, 5)]
    public class ChestControl : TerrariaPlugin
    {
        public static bool Init = false;
        public static CPlayer[] Players = new CPlayer[Main.maxNetPlayers];

        public ChestControl(Main game)
            : base(game)
        {
        }
        public override string Name
        {
            get { return "Chest Control"; }
        }

        public override Version Version
        {
            get { return new Version(2, 0); }
        }

        public override string Author
        {
            get { return "Deathmax, Natrim"; }
        }

        public override string Description
        {
            get { return "Gives you control over chests."; }
        }

        public override void Initialize()
        {
            NetHooks.GetData += new NetHooks.GetDataD(NetHooks_GetData);
            ServerHooks.Leave += ServerHooks_Leave;
            GameHooks.Update += OnUpdate;
            WorldHooks.SaveWorld += OnSaveWorld;
        }

        public override void DeInitialize()
        {
            NetHooks.GetData -= NetHooks_GetData;
            ServerHooks.Leave -= ServerHooks_Leave;
            GameHooks.Update -= OnUpdate;
            WorldHooks.SaveWorld -= OnSaveWorld;
        }

        void OnSaveWorld(bool resettime, System.ComponentModel.HandledEventArgs e)
        {
            ChestManager.Save(); //save chests
        }

        void OnUpdate(GameTime obj)
        {
            if (!Init && Main.worldID > 0)
            {
                Console.WriteLine("Initiating ChestControl...");
                ChestManager.Load();
                Commands.Load();
                for (int i = 0; i < Players.Length; i++)
                {
                    Players[i] = new CPlayer(i);
                }
                Init = true;
            }
        }

        void ServerHooks_Leave(int obj)
        {
            Players[obj] = new CPlayer(obj);
        }

        void NetHooks_GetData(GetDataEventArgs e)
        {
            switch (e.MsgID)
            {
                case PacketTypes.ChestGetContents:
                    if (!e.Handled)
                    {
                        using (var data = new MemoryStream(e.Msg.readBuffer, e.Index, e.Length))
                        {
                            BinaryReader reader = new BinaryReader(data);
                            var x = reader.ReadInt32();
                            var y = reader.ReadInt32();
                            reader.Close();
                            var id = Terraria.Chest.FindChest(x, y);
                            var player = ChestControl.Players[e.Msg.whoAmI];
                            var tplayer = TShock.Players[e.Msg.whoAmI];
                            if (id != -1)
                            {
                                var chest = ChestManager.getChest(id);
                                var naggedAboutLock = false;

                                switch (player.getState())
                                {
                                    case SettingState.Setting:
                                        if (chest.hasOwner())
                                        {
                                            if (chest.isOwner(player))
                                            {
                                                player.SendMessage("You already own this chest!", Color.Red);
                                            }
                                            else
                                            {
                                                player.SendMessage("This chest is already owned by someone!", Color.Red);
                                                naggedAboutLock = true;
                                            }
                                        }
                                        else
                                        {
                                            chest.setID(id);
                                            chest.setPosition(x, y);
                                            chest.setOwner(player);
                                            chest.Lock();

                                            player.SendMessage("This chest is now yours, and yours only.", Color.Red);
                                        }

                                        //end player setting
                                        player.setState(SettingState.None);
                                        break;

                                    case SettingState.RegionSetting:
                                        if (chest.hasOwner())
                                        {
                                            if (chest.isOwner(player))
                                            {
                                                if (chest.isRegionLocked())
                                                {
                                                    chest.regionLock(false);

                                                    player.SendMessage("Region share disabled. This chest is now only yours. To fully remove protection use \"cunset\".", Color.Red);
                                                }
                                                else
                                                {
                                                    if (TShock.Regions.InArea(x, y))
                                                    {
                                                        chest.regionLock(true);

                                                        player.SendMessage("This chest is now shared between region users. Use this command again to disable it.", Color.Red);
                                                    }
                                                    else
                                                    {
                                                        player.SendMessage("You can region share chest only if the chest is inside region!", Color.Red);
                                                    }
                                                }
                                            }
                                            else
                                            {
                                                player.SendMessage("This chest isn't yours!", Color.Red);
                                                naggedAboutLock = true;
                                            }
                                        }
                                        else
                                        {
                                            if (TShock.Regions.InArea(x, y))
                                            {
                                                chest.setID(id);
                                                chest.setPosition(x, y);
                                                chest.setOwner(player);
                                                chest.Lock();
                                                chest.regionLock(true);

                                                player.SendMessage("This chest is now shared between region users with you as owner. Use this command again to disable region sharing (You will still be owner).", Color.Red);
                                            }
                                            else
                                            {
                                                player.SendMessage("You can region share chest only if the chest is inside region!", Color.Red);
                                            }

                                        }

                                        //end player setting
                                        player.setState(SettingState.None);
                                        break;

                                    case SettingState.PublicSetting:
                                        if (chest.hasOwner())
                                        {
                                            if (chest.isOwner(player))
                                            {
                                                if (chest.isLocked())
                                                {
                                                    chest.UnLock();
                                                    player.SendMessage("This chest is now public! Use this command again to set it private.", Color.Red);
                                                }
                                                else
                                                {
                                                    chest.Lock();
                                                    player.SendMessage("This chest is now private! Use this command again to set it public.", Color.Red);
                                                }
                                            }
                                            else
                                            {
                                                player.SendMessage("This chest isn't yours!", Color.Red);
                                                naggedAboutLock = true;
                                            }

                                        }
                                        else
                                        {
                                            chest.setID(id);
                                            chest.setPosition(x, y);
                                            chest.setOwner(player);

                                            player.SendMessage("This chest is now yours. This chest is public. Use this command again to set it private.", Color.Red);
                                        }
                                        break;

                                    case SettingState.Deleting:
                                        if (chest.hasOwner())
                                        {
                                            if (chest.isOwner(player) || tplayer.Group.HasPermission("removechestprotection"))
                                            {
                                                chest.reset();
                                                player.SendMessage("This chest is no longer yours!", Color.Red);
                                            }
                                            else
                                            {
                                                player.SendMessage("This chest isn't yours!", Color.Red);
                                                naggedAboutLock = true;
                                            }
                                        }
                                        else
                                        {
                                            player.SendMessage("This chest is not protected!", Color.Red);
                                        }

                                        //end player setting
                                        player.setState(SettingState.None);
                                        break;

                                    case SettingState.PasswordSetting:
                                        if (chest.hasOwner())
                                        {
                                            if (chest.isOwner(player))
                                            {
                                                chest.setPassword(player.PasswordForChest);
                                                player.SendMessage("This chest is now protected with password.", Color.Red);
                                            }
                                            else
                                            {
                                                player.SendMessage("This chest isn't yours!", Color.Red);
                                                naggedAboutLock = true;
                                            }
                                        }
                                        else
                                        {
                                            chest.setID(id);
                                            chest.setPosition(x, y);
                                            chest.setOwner(player);
                                            chest.Lock();
                                            chest.setPassword(player.PasswordForChest);

                                            player.SendMessage("This chest is now protected with password, with you as owner.", Color.Red);
                                        }

                                        //end player setting
                                        player.setState(SettingState.None);
                                        break;

                                    case SettingState.PasswordUnSetting:
                                        if (chest.hasOwner())
                                        {
                                            if (chest.isOwner(player))
                                            {
                                                chest.setPassword("");
                                                player.SendMessage("This chest password has been removed.", Color.Red);
                                            }
                                            else
                                            {
                                                player.SendMessage("This chest isn't yours!", Color.Red);
                                                naggedAboutLock = true;
                                            }
                                        }
                                        else
                                        {
                                            player.SendMessage("This chest is not protected!", Color.Red);
                                        }

                                        //end player setting
                                        player.setState(SettingState.None);
                                        break;

                                    case SettingState.UnLocking:
                                        if (chest.hasOwner())
                                        {
                                            if (chest.isLocked())
                                            {
                                                if (chest.getPassword() == "")
                                                {
                                                    player.SendMessage("This chest can't be unlocked with password!", Color.Red);
                                                    naggedAboutLock = true;
                                                }
                                                else
                                                {
                                                    if (chest.isOwner(player))
                                                    {
                                                        player.SendMessage("You are owner of this chest, you dont need to unlock it. If you want to remove password use \"/lockchest remove\".", Color.Red);
                                                    }
                                                    else if (player.hasAccessToChest(chest.getID()))
                                                    {
                                                        player.SendMessage("You already have access to this chest!", Color.Red);
                                                    }
                                                    else
                                                    {
                                                        if (chest.checkPassword(player.PasswordForChest))
                                                        {
                                                            player.unlockedChest(chest.getID());
                                                            player.SendMessage("Chest unlocked! When you leave game you must unlock it again.", Color.Red);
                                                        }
                                                        else
                                                        {
                                                            player.SendMessage("Wrong password for chest!", Color.Red);
                                                            naggedAboutLock = true;
                                                        }
                                                    }
                                                }
                                            }
                                            else
                                            {
                                                player.SendMessage("This chest is not locked!", Color.Red);
                                            }
                                        }
                                        else
                                        {
                                            player.SendMessage("This chest is not protected!", Color.Red);
                                        }

                                        //end player setting
                                        player.setState(SettingState.None);
                                        break;
                                }

                                if (tplayer.Group.HasPermission("showchestinfo")) //if player should see chest info
                                {
                                    player.SendMessage("Chest Owner: " + chest.getOwner() + " || " + "Public: " + (chest.isLocked() ? "No" : "Yes") + " || " + "RegionShare: " + (chest.isRegionLocked() ? "Yes" : "No") + " || " + "Password: " + (chest.getPassword() == "" ? "No" : "Yes"), Color.Yellow);
                                }

                                if (!tplayer.Group.HasPermission("openallchests") && !chest.isOpenFor(player)) //if player doesnt has permission to see inside chest, then break and message
                                {
                                    e.Handled = true;
                                    if (!naggedAboutLock)
                                    {
                                        if (chest.getPassword() != "") //protected with password - notice player about it
                                        {
                                            player.SendMessage("This chest is magically locked with password. ( Use \"/cunlock PASSWORD\" to unlock it. )", Color.IndianRed);
                                        }
                                        else
                                        {
                                            player.SendMessage("This chest is magically locked.", Color.IndianRed);
                                        }
                                    }
                                    return;
                                }
                            }

                            if (player.getState() != SettingState.None) //if player is still setting something - end his setting
                                player.setState(SettingState.None);
                        }
                    }
                    break;
                case PacketTypes.TileKill:
                case PacketTypes.Tile:
                    using (var data = new MemoryStream(e.Msg.readBuffer, e.Index, e.Length))
                    {
                        BinaryReader reader = new BinaryReader(data);
                        int x;
                        int y;
                        if (e.MsgID == PacketTypes.Tile)
                        {
                            var type = reader.ReadByte();
                            if (!(type == 0 || type == 4))
                                return;
                        }
                        x = reader.ReadInt32();
                        y = reader.ReadInt32();
                        reader.Close();
                        var id = Terraria.Chest.FindChest(x, y);
                        var player = ChestControl.Players[e.Msg.whoAmI];
                        var tplayer = TShock.Players[e.Msg.whoAmI];

                        //dirty fix for finding chest, try to find chest point around
                        //TODO: maybe add some checking on tile type ?
                        if (id == -1)
                        {
                            id = Terraria.Chest.FindChest(x - 1, y); //search one tile left
                            if (id == -1)
                            {
                                id = Terraria.Chest.FindChest(x - 1, y - 1); //search one tile left and one tile up
                                if (id == -1)
                                {
                                    id = Terraria.Chest.FindChest(x, y - 1); //search one tile up
                                }
                            }
                        }

                        if (id != -1) //if have found chest
                        {
                            var chest = ChestManager.getChest(id);
                            if (chest.hasOwner())//if owned stop removing
                            {
                                if (tplayer.Group.HasPermission("removechestprotection") || chest.isOwner(player)) //display more verbose info to player who has permission to remove protection on this chest
                                {
                                    player.SendMessage("This chest is protected. To remove it, first remove protection using \"/cunset\" command.", Color.Red);
                                }
                                else
                                {
                                    player.SendMessage("This chest is protected!", Color.Red);
                                }

                                player.SendTileSquare(x, y);
                                e.Handled = true;
                            }
                        }
                    }
                    break;
                default:
                    break;
            }
        }
    }
}

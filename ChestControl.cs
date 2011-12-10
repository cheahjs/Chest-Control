using Terraria;
using Hooks;
using TShockAPI;
using System;
using System.IO;

namespace ChestControl
{
    [APIVersion(1, 9)]
    public class ChestControl : TerrariaPlugin
    {
        private static bool Init = false;
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
            get { return new Version(2, 2, 1, 4); }
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
            NetHooks.GetData += NetHooks_GetData;
            ServerHooks.Leave += ServerHooks_Leave;
            GameHooks.Update += OnUpdate;
            WorldHooks.SaveWorld += OnSaveWorld;
        }

        protected override void Dispose(bool disposing)
        {
            NetHooks.GetData -= NetHooks_GetData;
            ServerHooks.Leave -= ServerHooks_Leave;
            GameHooks.Update -= OnUpdate;
            WorldHooks.SaveWorld -= OnSaveWorld;

            base.Dispose(disposing);
        }

        private void OnSaveWorld(bool resettime, System.ComponentModel.HandledEventArgs e)
        {
            try
            {
                ChestManager.Save(); //save chests
            }
            catch (Exception ex) //we don't want the world to fail to save.
            {
                Console.WriteLine(ex);
            }
        }

        private void OnUpdate()
        {
            if (Init || Main.worldID <= 0) return;
            Console.WriteLine("Initiating ChestControl...");
            ChestManager.Load();
            Commands.Load();
            new System.Threading.Thread(UpdateChecker).Start();
            for (var i = 0; i < Players.Length; i++)
                Players[i] = new CPlayer(i);
            Init = true;
        }

        private void ServerHooks_Leave(int obj)
        {
            Players[obj] = new CPlayer(obj);
        }

        private void NetHooks_GetData(GetDataEventArgs e)
        {
            switch (e.MsgID)
            {
                case PacketTypes.ChestGetContents:
                    if (!e.Handled)
                        using (var data = new MemoryStream(e.Msg.readBuffer, e.Index, e.Length))
                        {
                            var reader = new BinaryReader(data);
                            var x = reader.ReadInt32();
                            var y = reader.ReadInt32();
                            reader.Close();
                            var id = Terraria.Chest.FindChest(x, y);
                            var player = Players[e.Msg.whoAmI];
                            var tplayer = TShock.Players[e.Msg.whoAmI];
                            if (id != -1)
                            {
                                var chest = ChestManager.GetChest(id);
                                var naggedAboutLock = false;

                                switch (player.GetState())
                                {
                                    case SettingState.Setting:
                                        if (chest.HasOwner())
                                            if (chest.IsOwnerConvert(player))
                                                player.SendMessage("You already own this chest!", Color.Red);
                                            else
                                            {
                                                player.SendMessage("This chest is already owned by someone!", Color.Red);
                                                naggedAboutLock = true;
                                            }
                                        else
                                        {
                                            chest.SetID(id);
                                            chest.SetPosition(x, y);
                                            chest.SetOwner(player);
                                            chest.Lock();

                                            player.SendMessage("This chest is now yours, and yours only.", Color.Red);
                                        }

                                        //end player setting
                                        player.SetState(SettingState.None);
                                        break;

                                    case SettingState.RegionSetting:
                                        if (chest.HasOwner())
                                            if (chest.IsOwnerConvert(player))
                                                if (chest.IsRegionLocked())
                                                {
                                                    chest.regionLock(false);

                                                    player.SendMessage(
                                                        "Region share disabled. This chest is now only yours. To fully remove protection use \"cunset\".",
                                                        Color.Red);
                                                }
                                                else if (TShock.Regions.InArea(x, y))
                                                {
                                                    chest.regionLock(true);

                                                    player.SendMessage(
                                                        "This chest is now shared between region users. Use this command again to disable it.",
                                                        Color.Red);
                                                }
                                                else
                                                    player.SendMessage(
                                                        "You can region share chest only if the chest is inside region!",
                                                        Color.Red);
                                            else
                                            {
                                                player.SendMessage("This chest isn't yours!", Color.Red);
                                                naggedAboutLock = true;
                                            }
                                        else if (TShock.Regions.InArea(x, y))
                                        {
                                            chest.SetID(id);
                                            chest.SetPosition(x, y);
                                            chest.SetOwner(player);
                                            chest.Lock();
                                            chest.regionLock(true);

                                            player.SendMessage(
                                                "This chest is now shared between region users with you as owner. Use this command again to disable region sharing (You will still be owner).",
                                                Color.Red);
                                        }
                                        else
                                            player.SendMessage(
                                                "You can region share chest only if the chest is inside region!",
                                                Color.Red);

                                        //end player setting
                                        player.SetState(SettingState.None);
                                        break;

                                    case SettingState.PublicSetting:
                                        if (chest.HasOwner())
                                            if (chest.IsOwnerConvert(player))
                                                if (chest.IsLocked())
                                                {
                                                    chest.UnLock();
                                                    player.SendMessage(
                                                        "This chest is now public! Use \"/cpset\" to set it private.",
                                                        Color.Red);
                                                }
                                                else
                                                {
                                                    chest.Lock();
                                                    player.SendMessage(
                                                        "This chest is now private! Use \"/cpset\" to set it public.",
                                                        Color.Red);
                                                }
                                            else
                                            {
                                                player.SendMessage("This chest isn't yours!", Color.Red);
                                                naggedAboutLock = true;
                                            }
                                        else
                                        {
                                            chest.SetID(id);
                                            chest.SetPosition(x, y);
                                            chest.SetOwner(player);

                                            player.SendMessage(
                                                "This chest is now yours. This chest is public. Use \"/cpset\" to set it private.",
                                                Color.Red);
                                        }
                                        break;

                                    case SettingState.Deleting:
                                        if (chest.HasOwner())
                                            if (chest.IsOwnerConvert(player) ||
                                                tplayer.Group.HasPermission("removechestprotection"))
                                            {
                                                chest.Reset();
                                                player.SendMessage("This chest is no longer yours!", Color.Red);
                                            }
                                            else
                                            {
                                                player.SendMessage("This chest isn't yours!", Color.Red);
                                                naggedAboutLock = true;
                                            }
                                        else
                                            player.SendMessage("This chest is not protected!", Color.Red);

                                        //end player setting
                                        player.SetState(SettingState.None);
                                        break;

                                    case SettingState.PasswordSetting:
                                        if (chest.HasOwner())
                                            if (chest.IsOwnerConvert(player))
                                            {
                                                chest.SetPassword(player.PasswordForChest);
                                                player.SendMessage("This chest is now protected with password.",
                                                                   Color.Red);
                                            }
                                            else
                                            {
                                                player.SendMessage("This chest isn't yours!", Color.Red);
                                                naggedAboutLock = true;
                                            }
                                        else
                                        {
                                            chest.SetID(id);
                                            chest.SetPosition(x, y);
                                            chest.SetOwner(player);
                                            chest.Lock();
                                            chest.SetPassword(player.PasswordForChest);

                                            player.SendMessage(
                                                "This chest is now protected with password, with you as owner.",
                                                Color.Red);
                                        }

                                        //end player setting
                                        player.SetState(SettingState.None);
                                        break;

                                    case SettingState.PasswordUnSetting:
                                        if (chest.HasOwner())
                                            if (chest.IsOwnerConvert(player))
                                            {
                                                chest.SetPassword("");
                                                player.SendMessage("This chest password has been removed.", Color.Red);
                                            }
                                            else
                                            {
                                                player.SendMessage("This chest isn't yours!", Color.Red);
                                                naggedAboutLock = true;
                                            }
                                        else
                                            player.SendMessage("This chest is not protected!", Color.Red);

                                        //end player setting
                                        player.SetState(SettingState.None);
                                        break;

                                    case SettingState.RefillSetting:
                                        if (chest.HasOwner())
                                            if (chest.IsOwnerConvert(player))
                                            {
                                                chest.SetRefill(true);
                                                player.SendMessage("This chest is will now always refill with items.",
                                                                   Color.Red);
                                            }
                                            else
                                            {
                                                player.SendMessage("This chest isn't yours!", Color.Red);
                                                naggedAboutLock = true;
                                            }
                                        else
                                        {
                                            chest.SetID(id);
                                            chest.SetPosition(x, y);
                                            chest.SetOwner(player);
                                            chest.SetRefill(true);

                                            player.SendMessage(
                                                "This chest is will now always refill with items, with you as owner.",
                                                Color.Red);
                                        }

                                        //end player setting
                                        player.SetState(SettingState.None);
                                        break;

                                    case SettingState.RefillUnSetting:
                                        if (chest.IsRefill())
                                            if (chest.HasOwner())
                                                if (chest.IsOwnerConvert(player))
                                                {
                                                    chest.SetRefill(false);
                                                    player.SendMessage(
                                                        "This chest is will no longer refill with items.", Color.Red);
                                                }
                                                else
                                                {
                                                    player.SendMessage("This chest isn't yours!", Color.Red);
                                                    naggedAboutLock = true;
                                                }
                                            else
                                            {
                                                chest.SetID(id);
                                                chest.SetPosition(x, y);
                                                chest.SetOwner(player);
                                                chest.SetRefill(false);

                                                player.SendMessage("This chest is will no longer refill with items",
                                                                   Color.Red);
                                            }
                                        else
                                            player.SendMessage("This chest is not refilling!", Color.Red);

                                        //end player setting
                                        player.SetState(SettingState.None);
                                        break;

                                    case SettingState.UnLocking:
                                        if (chest.HasOwner())
                                            if (chest.IsLocked())
                                                if (chest.GetPassword() == "")
                                                {
                                                    player.SendMessage("This chest can't be unlocked with password!",
                                                                       Color.Red);
                                                    naggedAboutLock = true;
                                                }
                                                else if (chest.IsOwnerConvert(player))
                                                    player.SendMessage(
                                                        "You are owner of this chest, you dont need to unlock it. If you want to remove password use \"/lockchest remove\".",
                                                        Color.Red);
                                                else if (player.HasAccessToChest(chest.GetID()))
                                                    player.SendMessage("You already have access to this chest!",
                                                                       Color.Red);
                                                else if (chest.CheckPassword(player.PasswordForChest))
                                                {
                                                    player.UnlockedChest(chest.GetID());
                                                    player.SendMessage(
                                                        "Chest unlocked! When you leave game you must unlock it again.",
                                                        Color.Red);
                                                }
                                                else
                                                {
                                                    player.SendMessage("Wrong password for chest!", Color.Red);
                                                    naggedAboutLock = true;
                                                }
                                            else
                                                player.SendMessage("This chest is not locked!", Color.Red);
                                        else
                                            player.SendMessage("This chest is not protected!", Color.Red);

                                        //end player setting
                                        player.SetState(SettingState.None);
                                        break;
                                }

                                if (tplayer.Group.HasPermission("showchestinfo")) //if player should see chest info
                                    player.SendMessage(
                                        string.Format(
                                            "Chest Owner: {0} || Public: {1} || RegionShare: {2} || Password: {3} || Refill: {4}",
                                            chest.GetOwner() == "" ? "-None-" : chest.GetOwner(),
                                            chest.IsLocked() ? "No" : "Yes", chest.IsRegionLocked() ? "Yes" : "No",
                                            chest.GetPassword() == "" ? "No" : "Yes",
                                            chest.IsRefill() ? "Yes" : "No"), Color.Yellow);

                                if (!tplayer.Group.HasPermission("openallchests") && !chest.IsOpenFor(player))
                                    //if player doesnt has permission to see inside chest, then break and message
                                {
                                    e.Handled = true;
                                    if (!naggedAboutLock)
                                        player.SendMessage(
                                            chest.GetPassword() != ""
                                                ? "This chest is magically locked with password. ( Use \"/cunlock PASSWORD\" to unlock it. )"
                                                : "This chest is magically locked.", Color.IndianRed);
                                    return;
                                }
                            }
                            if (player.GetState() != SettingState.None)
                                //if player is still setting something - end his setting
                                player.SetState(SettingState.None);
                        }
                    break;
                case PacketTypes.TileKill:
                case PacketTypes.Tile:
                    using (var data = new MemoryStream(e.Msg.readBuffer, e.Index, e.Length))
                        try
                        {
                            var reader = new BinaryReader(data);
                            if (e.MsgID == PacketTypes.Tile)
                            {
                                var type = reader.ReadByte();
                                if (!(type == 0 || type == 4))
                                    return;
                            }
                            var x = reader.ReadInt32();
                            var y = reader.ReadInt32();
                            reader.Close();

                            if (Chest.TileIsChest(x, y)) //if is Chest
                            {
                                var id = Terraria.Chest.FindChest(x, y);
                                var player = Players[e.Msg.whoAmI];
                                var tplayer = TShock.Players[e.Msg.whoAmI];

                                //dirty fix for finding chest, try to find chest point around
                                if (id == -1)
                                    try
                                    {
                                        id = Terraria.Chest.FindChest(x - 1, y); //search one tile left
                                        if (id == -1)
                                        {
                                            id = Terraria.Chest.FindChest(x - 1, y - 1);
                                            //search one tile left and one tile up
                                            if (id == -1)
                                                id = Terraria.Chest.FindChest(x, y - 1); //search one tile up
                                        }
                                    }
                                    catch (Exception ex)
                                    {
                                        Console.WriteLine(ex);
                                    }

                                if (id != -1) //if have found chest
                                {
                                    var chest = ChestManager.GetChest(id);
                                    if (chest.HasOwner()) //if owned stop removing
                                    {
                                        if (tplayer.Group.HasPermission("removechestprotection") ||
                                            chest.IsOwnerConvert(player))
                                            //display more verbose info to player who has permission to remove protection on this chest
                                            player.SendMessage(
                                                "This chest is protected. To remove it, first remove protection using \"/cunset\" command.",
                                                Color.Red);
                                        else
                                            player.SendMessage("This chest is protected!", Color.Red);

                                        player.SendTileSquare(x, y);
                                        e.Handled = true;
                                    }
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine(ex);
                        }
                    break;
                case PacketTypes.ChestItem:
                    using (var data = new MemoryStream(e.Msg.readBuffer, e.Index, e.Length))
                    {
                        var reader = new BinaryReader(data);
                        var id = reader.ReadInt16();
                        var slot = reader.ReadByte();
                        var stack = reader.ReadByte();
                        //its pure ASCII bytes, not prefixed with the length.
                        //var itemname = reader.ReadString();
                        var itemnamebytes = new byte[e.Length - 4];
                        reader.Read(itemnamebytes, 0, (e.Length - 4));
                        reader.Close();
                        var itemname = System.Text.Encoding.ASCII.GetString(itemnamebytes);
                        if (id != -1)
                        {
                            var chest = ChestManager.GetChest(id);
                            if (chest.IsRefill())
                            {
                                //this should already stop changes to the chest, "refilling" the chest
                                e.Handled = true;
                                //but just in case
                                //Main.chest[id].item = chest.GetRefillItems();
                            }
                        }
                    }
                    break;
            }
        }

        private void UpdateChecker()
        {
            string raw;
            try
            {
                raw = new System.Net.WebClient().DownloadString("https://github.com/Deathmax/Chest-Control/raw/master/version.txt");
                
            }
            catch (Exception)
            {
                return;
            }
            var list = raw.Split('\n');
            Version version;
            if (!Version.TryParse(list[0], out version)) return;
            if (Version.CompareTo(version) >= 0) return;
            TShock.Utils.Broadcast(string.Format("New Chest-Control version : {0}", version), Color.Yellow);
            if (list.Length > 1)
                for (var i = 1; i < list.Length; i++)
                    TShock.Utils.Broadcast(list[i], Color.Yellow);
            TShock.Utils.Broadcast("Get the CC download at bit.ly/chestcontroldl", Color.Yellow);
        }
    }
}
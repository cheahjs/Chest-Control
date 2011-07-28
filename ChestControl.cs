using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Terraria;
using TerrariaAPI;
using TerrariaAPI.Hooks;
using TShockAPI;
using System.IO;
using System.Diagnostics;
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
            get { return new Version(1, 0); }
        }

        public override string Author
        {
            get { return "Deathmax,Natrim"; }
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
        }

        public override void DeInitialize()
        {
            NetHooks.GetData -= NetHooks_GetData;
            ServerHooks.Leave -= ServerHooks_Leave;
            GameHooks.Update -= OnUpdate;
        }

        void OnUpdate(Microsoft.Xna.Framework.GameTime obj)
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
                Console.WriteLine("ChestControl initiated.");
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
                            var player = Players[e.Msg.whoAmI];
                            if (id != -1)
                            {
                                var chest = ChestManager.getChest(id);

                                if (!player.Group.HasPermission("openallchests") && !chest.isOpenFor(player))
                                {
                                    e.Handled = true;
                                    player.SendMessage("This chest is magically locked.", Microsoft.Xna.Framework.Color.IndianRed);
                                    return;
                                }

                                if (chest.isLocked())
                                {
                                    if (player.getState() == SettingState.Setting)
                                    {
                                        player.SendMessage("This chest is already locked!", Color.Red);
                                    }

                                    if (player.getState() == SettingState.Deleting)
                                    {
                                        if (chest.isOwner(player) || player.Group.HasPermission("removechestprotection"))
                                        {
                                            chest.UnLock();
                                            player.SendMessage("This chest is no longer protected!", Color.Red);
                                            ChestManager.Save();
                                        }
                                        else
                                        {
                                            player.SendMessage("This chest isn't yours!", Color.Red);
                                        }
                                    }

                                    if (player.getState() == SettingState.RegionSetting)
                                    {
                                        if (chest.isOwner(player))
                                        {
                                            if (TShock.Regions.InArea(x, y))
                                            {
                                                chest.regionLock(true);

                                                player.SendMessage("This chest is now shared between region users.", Microsoft.Xna.Framework.Color.Red);
                                                ChestManager.Save();
                                            }
                                            else
                                            {
                                                player.SendMessage("You can region share chest only if the chest is inside region!", Color.Red);
                                            }
                                        }
                                        else
                                        {
                                            player.SendMessage("This chest isn't yours!", Color.Red);
                                        }
                                    }
                                }
                                else
                                {
                                    if (player.getState() == SettingState.Deleting)
                                    {
                                        player.SendMessage("This chest is not locked!", Color.Red);
                                    }

                                    if (player.getState() == SettingState.RegionSetting)
                                    {
                                        if (TShock.Regions.InArea(x, y))
                                        {
                                            chest.setID(id);
                                            chest.setPosition(x, y);
                                            chest.setOwner(player);
                                            chest.Lock();
                                            chest.regionLock(true);

                                            player.SendMessage("This chest is now shared between region users with you as owner.", Microsoft.Xna.Framework.Color.Red);

                                            ChestManager.Save();
                                        }
                                        else
                                        {
                                            player.SendMessage("You can region share chest only if the chest is inside region!", Color.Red);
                                            player.setState(SettingState.None);
                                        }
                                    }

                                    if (player.getState() == SettingState.Setting)
                                    {
                                        chest.setID(id);
                                        chest.setPosition(x, y);
                                        chest.setOwner(player);
                                        chest.Lock();

                                        player.SendMessage("This chest is now yours, and yours only.", Microsoft.Xna.Framework.Color.Red);

                                        ChestManager.Save();
                                    }

                                }
                            }
                            
                            if (player.getState() != SettingState.None) //if player sam setting something - end his setting
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
                        var player = Players[e.Msg.whoAmI];
                        if (id != -1)
                        {

                            var chest = ChestManager.getChest(id);
                            if (!player.Group.HasPermission("openallchests") && !chest.isOpenFor(player))
                            {
                                player.SendMessage("This chest is protected!", Microsoft.Xna.Framework.Color.Red);
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

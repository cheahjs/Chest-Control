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
            get { return "Deathmax"; }
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
                    Players[i] = new CPlayer(i);
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
                            var player = TShock.Players[e.Msg.whoAmI];
                            if (id != -1)
                            {
                                if (!player.Group.HasPermission("openallchests") &&
                                    ((ChestManager.Chests[id].Owner != "" &&
                                    ChestManager.Chests[id].Owner.ToLower() != player.Name.ToLower()) ||
                                    TShock.Regions.InProtectedArea(x, y, GetPlayerIP(player.Name)) ||
                                    !player.IsLoggedIn))
                                {
                                    e.Handled = true;
                                    player.SendMessage("This chest is magically locked.", Microsoft.Xna.Framework.Color.IndianRed);
                                    return;
                                }
                                if (Players[e.Msg.whoAmI].Setting)
                                {
                                    ChestManager.Chests[id].Owner = player.Name;
                                    ChestManager.Chests[id].Position = new Microsoft.Xna.Framework.Vector2(x, y);
                                    ChestManager.Chests[id].ID = id;
                                    player.SendMessage("This chest is now yours, and yours only.", Microsoft.Xna.Framework.Color.Red);
                                    Players[e.Msg.whoAmI].Setting = false;
                                    ChestManager.Save();
                                }
                            }
                                
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
                        var player = TShock.Players[e.Msg.whoAmI];
                        if (id != -1)
                        {
                            if (!player.Group.HasPermission("openallchests") &&
                                    ((ChestManager.Chests[id].Owner != "" &&
                                    ChestManager.Chests[id].Owner.ToLower() != player.Name.ToLower()) ||
                                    TShock.Regions.InProtectedArea(x, y, GetPlayerIP(player.Name)) ||
                                    !player.IsLoggedIn))
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

        public static string GetPlayerIP(string playername)
        {
            foreach (TSPlayer player in TShock.Players)
            {
                if (player != null && player.Active)
                {
                    if (playername.ToLower() == player.Name.ToLower())
                    {
                        return player.IP;
                    }
                }
            }
            return null;
        }
    }
}

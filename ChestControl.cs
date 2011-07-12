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
                            //var id = reader.ReadInt16();
                            var x = reader.ReadInt32();
                            var y = reader.ReadInt32();
                            reader.Close();
                            var id = Terraria.Chest.FindChest(x, y);
                            if (id != -1)
                            {
                                if (ChestManager.Chests[id].Owner != "" && ChestManager.Chests[id].Owner.ToLower() != Main.player[e.Msg.whoAmI].name.ToLower() && !TShock.Players[e.Msg.whoAmI].Group.HasPermission("openallchests"))
                                {
                                    e.Handled = true;
                                    TShock.Players[e.Msg.whoAmI].SendMessage("This chest is magically locked.", Microsoft.Xna.Framework.Color.IndianRed);
                                    return;
                                }
                                if (Players[e.Msg.whoAmI].Setting)
                                {
                                    ChestManager.Chests[id].Owner = TShock.Players[e.Msg.whoAmI].Name;
                                    ChestManager.Chests[id].Position = new Microsoft.Xna.Framework.Vector2(x, y);
                                    ChestManager.Chests[id].ID = id;
                                    TShock.Players[e.Msg.whoAmI].SendMessage("This chest is now yours, and yours only.", Microsoft.Xna.Framework.Color.Red);
                                    Players[e.Msg.whoAmI].Setting = false;
                                    ChestManager.Save();
                                }
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

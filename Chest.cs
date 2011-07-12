using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TShockAPI;
using Terraria;
using Microsoft.Xna.Framework;

namespace ChestControl
{
    class Chest
    {
        public int ID;
        public int WorldID;
        public string Owner;
        public Vector2 Position;

        public Chest()
        {
            ID = -1;
            WorldID = Main.worldID;
            Owner = "";
            Position = new Vector2(0, 0);
        }
    }
}

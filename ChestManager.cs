using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Microsoft.Xna.Framework;
using TShockAPI;
using Terraria;

namespace ChestControl
{
    class ChestManager
    {
        public static Chest[] Chests = new Chest[Main.maxChests];

        public static void Load()
        {
            if (!File.Exists(Path.Combine(TShock.SavePath, "\\chestcontrol\\" + Main.worldID + ".txt")))
            {
                Directory.CreateDirectory(Path.Combine(TShock.SavePath, "\\chestcontrol\\"));
                File.Create(Path.Combine(TShock.SavePath, "\\chestcontrol\\" + Main.worldID + ".txt")).Close();
            }

            for (int i = 0; i < Chests.Length; i++)
                Chests[i] = new Chest();

            foreach (var line in File.ReadAllLines(Path.Combine(TShock.SavePath, "\\chestcontrol\\" + Main.worldID + ".txt")))
            {
                var args = line.Split('|');
                if (args.Length < 3)
                    continue;
                try
                {
                    Chests[int.Parse(args[0])].Position = new Vector2(int.Parse(args[1]), int.Parse(args[2]));
                    Chests[int.Parse(args[0])].Owner = string.Join("|", args, 3, args.Length - 2);
                    Chests[int.Parse(args[0])].ID = int.Parse(args[0]);
                }
                catch { }
            }
        }

        public static void Save()
        {
            var lines = new List<string>();
            foreach (var chest in Chests)
            {
                if (chest.Owner != "")
                    lines.Add(string.Format("{0}|{1},{2}|{3}", chest.ID, chest.Position.X, chest.Position.Y, chest.Owner));
            }
            File.WriteAllLines(Path.Combine(TShock.SavePath, "\\chestcontrol\\" + Main.worldID + ".txt"), lines.ToArray());
        }
    }
}

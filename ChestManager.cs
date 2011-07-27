﻿using System;
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
        private static Chest[] Chests = new Chest[Main.maxChests];
        private static string ChestControlDirectory = Path.Combine(TShock.SavePath, "chestcontrol");
        private static string ChestSavePath = Path.Combine(ChestControlDirectory, Main.worldID + ".txt");


        public static Chest getChest(int id)
        {
            return Chests[id];
        }

        public static void Load()
        {
            if (!Directory.Exists(ChestControlDirectory))
            {
                Directory.CreateDirectory(ChestControlDirectory);
            }

            if (!File.Exists(ChestSavePath))
            {
                File.Create(ChestSavePath).Close();
            }

            for (int i = 0; i < Chests.Length; i++)
                Chests[i] = new Chest();

            var error = false;
            foreach (var line in File.ReadAllLines(ChestSavePath))
            {
                var args = line.Split('|');
                if (args.Length < 6)
                {
                    continue;
                }
                try
                {
                    var chest = Chests[int.Parse(args[0])];

                    chest.setPosition(new Vector2(int.Parse(args[1]), int.Parse(args[2])));
                    chest.setOwner(args[3]);
                    chest.setID(int.Parse(args[0]));
                    if (bool.Parse(args[4]))
                    {
                        chest.Lock();
                    }
                    if (bool.Parse(args[5]))
                    {
                        chest.regionLock(true);
                    }
                }
                catch
                {
                    error = true;
                }
            }

            if (error)
            {
                Console.WriteLine("Failed to load some chests data, corresponding chests will be left unprotected.");
            }
        }

        public static void Save()
        {
            var lines = new List<string>();
            foreach (var chest in Chests)
            {
                if (chest.getOwner() != "")
                {
                    lines.Add(string.Format("{0}|{1}|{2}|{3}|{4}|{5}", chest.getID(), chest.getPosition().X, chest.getPosition().Y, chest.getOwner(), chest.isLocked(), chest.isRegionLocked()));
                }
            }
            File.WriteAllLines(ChestSavePath, lines.ToArray());
        }
    }
}

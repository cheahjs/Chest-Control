using Terraria;
using TShockAPI;
using System.Collections.Generic;
using System.IO;
using Microsoft.Xna.Framework;

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
                if (args.Length < 7)
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
                        chest.Lock();
                    if (bool.Parse(args[5]))
                        chest.regionLock(true);
                    if (args[6] != "")
                        chest.setPassword(args[6], true);
                    //provide backwards compatibility
                    if (args.Length == 9)
                        if (bool.Parse(args[7]))
                            chest.setRefillItems(args[8], true);

                    //check if chest still exists in world
                    if (!Chest.TileIsChest(chest.getPosition()))
                    {
                        //chest dont exists - so reset it
                        chest.reset();
                    }
                }
                catch
                {
                    error = true;
                }
            }

            if (error)
            {
                System.Console.WriteLine("Failed to load some chests data, corresponding chests will be left unprotected.");
            }
        }

        public static void Save()
        {
            var lines = new List<string>();
            foreach (var chest in Chests)
            {
                if (Chest.TileIsChest(chest.getPosition()))
                {
                    if (chest.getOwner() != "")
                    {
                        lines.Add(string.Format("{0}|{1}|{2}|{3}|{4}|{5}|{6}|{7}|{8}", 
                            chest.getID(), chest.getPosition().X, chest.getPosition().Y, 
                            chest.getOwner(), chest.isLocked(), chest.isRegionLocked(), 
                            chest.getPassword(), chest.IsRefill(), string.Join(",", chest.getRefillItemNames())));
                    }
                }
            }
            File.WriteAllLines(ChestSavePath, lines.ToArray());
        }
    }
}

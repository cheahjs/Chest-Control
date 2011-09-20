using System.Linq;
using Terraria;
using TShockAPI;
using System.Collections.Generic;
using System.IO;
using System.Drawing;

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
                Directory.CreateDirectory(ChestControlDirectory);

            if (!File.Exists(ChestSavePath))
                File.Create(ChestSavePath).Close();

            for (var i = 0; i < Chests.Length; i++)
                Chests[i] = new Chest();

            var error = false;
            foreach (var args in File.ReadAllLines(ChestSavePath).Select(line => line.Split('|')).Where(args => args.Length >= 7))
            {
                try
                {
                    var chest = new Chest();

                    chest.SetPosition(new PointF(int.Parse(args[1]), int.Parse(args[2])));
                    chest.SetOwner(args[3]);
                    chest.SetID(int.Parse(args[0]));
                    if (bool.Parse(args[4]))
                        chest.Lock();
                    if (bool.Parse(args[5]))
                        chest.regionLock(true);
                    if (args[6] != "")
                        chest.SetPassword(args[6], true);
                    //provide backwards compatibility
                    if (args.Length == 9)
                        if (bool.Parse(args[7]))
                        {
                            chest.SetRefill(true);
                            chest.SetRefillItems(args[8], true);
                        }

                    //check if chest still exists in world
                    if (!Chest.TileIsChest(chest.GetPosition()))
                    {
                        //chest dont exists - so reset it
                        chest.Reset();
                    }
                    //check if chest in array didn't move
                    if (!VerifyChest(chest.GetID(), chest.GetPosition()))
                    {
                        var id = Terraria.Chest.FindChest((int)chest.GetPosition().X, (int)chest.GetPosition().Y);
                        if (id != -1)
                            chest.SetID(id);
                        else
                            chest.Reset();
                    }

                    Chests[chest.GetID()] = chest;
                }
                catch
                {
                    error = true;
                }
            }

            if (error)
                System.Console.WriteLine(
                    "Failed to load some chests data, corresponding chests will be left unprotected.");
        }

        public static void Save()
        {
            var lines = new List<string>();
            foreach (var chest in Chests)
            {
                if (chest != null)
                {
                    if (Chest.TileIsChest(chest.GetPosition()))
                    {
                        if (chest.GetOwner() != "")
                        {
                            lines.Add(string.Format("{0}|{1}|{2}|{3}|{4}|{5}|{6}|{7}|{8}",
                                chest.GetID(), chest.GetPosition().X, chest.GetPosition().Y,
                                chest.GetOwner(), chest.IsLocked(), chest.IsRegionLocked(),
                                chest.GetPassword(), chest.IsRefill(), string.Join(",", chest.GetRefillItemNames())));
                        }
                    }
                }
                else
                    return; //it shouldn't EVER be null
            }
            File.WriteAllLines(ChestSavePath, lines.ToArray());
        }

        private static bool VerifyChest(int id, PointF pos)
        {
            return Terraria.Chest.FindChest((int)pos.X, (int)pos.Y) == id;
        }
    }
}

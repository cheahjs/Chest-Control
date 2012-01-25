using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TShockAPI;
using Terraria;

namespace ChestControl
{
    internal class ChestManager
    {
        private static readonly Chest[] Chests = new Chest[Main.maxChests];
        public static readonly string ChestControlDirectory = Path.Combine(TShock.SavePath, "chestcontrol");
        public static string ChestSavePath = Path.Combine(ChestControlDirectory, Main.worldID + ".txt");
        public static readonly string ChestLogPath = Path.Combine(ChestControlDirectory, "log.txt");

        public static Chest GetChest(int id)
        {
            return Chests[id];
        }

        public static void Load()
        {
            ChestSavePath = Path.Combine(ChestControlDirectory, Main.worldID + ".txt");
            if (!Directory.Exists(ChestControlDirectory))
                Directory.CreateDirectory(ChestControlDirectory);

            if (!File.Exists(ChestSavePath))
                File.Create(ChestSavePath).Close();

            for (int i = 0; i < Chests.Length; i++)
                Chests[i] = new Chest();

            bool error = false;
            foreach (
                var args in
                    File.ReadAllLines(ChestSavePath).Select(line => line.Split('|')).Where(args => args.Length >= 7))
                try
                {
                    var chest = new Chest();

                    chest.SetPosition(new Vector2(int.Parse(args[1]), int.Parse(args[2])));
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
                            //chest.SetRefillItems(args[8]);
                        }

                    //check if chest still exists in world
                    if (!Chest.TileIsChest(chest.GetPosition()))
                        //chest dont exists - so reset it
                        chest.Reset();
                    //check if chest in array didn't move
                    if (!VerifyChest(chest.GetID(), chest.GetPosition()))
                    {
                        int id = Terraria.Chest.FindChest((int) chest.GetPosition().X, (int) chest.GetPosition().Y);
                        if (id != -1)
                            chest.SetID(id);
                        else
                            chest.Reset();
                    }

                    if (Chests.Length > chest.GetID()) Chests[chest.GetID()] = chest;
                }
                catch
                {
                    error = true;
                }

            if (error)
                Log.Write("Failed to load some chests data, corresponding chests will be left unprotected.", LogLevel.Error);
        }

        public static void Save()
        {
            var lines = new List<string>();
            foreach (Chest chest in Chests)
                if (chest == null)
                    return; //it shouldn't EVER be null
                else
                {
                    if (Chest.TileIsChest(chest.GetPosition()))
                        if (chest.GetOwner() != "")
                            lines.Add(string.Format("{0}|{1}|{2}|{3}|{4}|{5}|{6}|{7}|{8}",
                                chest.GetID(), chest.GetPosition().X, chest.GetPosition().Y,
                                chest.GetOwner(), chest.IsLocked(), chest.IsRegionLocked(),
                                chest.GetPassword(), chest.IsRefill(),
                                string.Join(",", chest.GetRefillItemNames())));
                }
            File.WriteAllLines(ChestSavePath, lines.ToArray());
        }

        private static bool VerifyChest(int id, Vector2 pos)
        {
            return Terraria.Chest.FindChest((int) pos.X, (int) pos.Y) == id;
        }
    }
}
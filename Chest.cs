﻿using TShockAPI;
using System.Drawing;

namespace ChestControl
{
    class Chest
    {
        protected int ID;
        protected int WorldID;
        protected string Owner;
        protected PointF Position;
        protected bool Locked;
        protected bool RegionLock;
        protected bool Refill;
        private string HashedPassword;
        protected Terraria.Item[] RefillItems;

        public Chest()
        {
            ID = -1;
            WorldID = Terraria.Main.worldID;
            Owner = "";
            Position = new PointF(0, 0);
            Locked = false;
            RegionLock = false;
            Refill = false;
            HashedPassword = "";
            RefillItems = new Terraria.Item[20];
        }

        public void Reset()
        {
            Owner = "";
            Locked = false;
            RegionLock = false;
            Refill = false;
            HashedPassword = "";
            RefillItems = new Terraria.Item[20];
        }

        public void SetID(int id)
        {
            ID = id;
        }

        public int GetID()
        {
            return ID;
        }

        public void SetOwner(string player)
        {
            Owner = player;
        }

        public void SetOwner(CPlayer player)
        {
            Owner = TShock.Players[player.Index].UserAccountName;//player.Name;
        }

        public string GetOwner()
        {
            return Owner;
        }

        public void SetPosition(PointF position)
        {
            Position = position;
        }

        public void SetPosition(int x, int y)
        {
            Position = new PointF(x, y);
        }

        public PointF GetPosition()
        {
            return Position;
        }

        public void Lock()
        {
            Locked = true;
        }

        public void UnLock()
        {
            Locked = false;
        }

        public void regionLock(bool locking)
        {
            RegionLock = locking;
        }

        public bool HasOwner()
        {
            if (Owner != "")
            {
                return true;
            }
            return false;
        }

        public bool IsOwner(CPlayer player)
        {
            return HasOwner() && Owner.Equals(TShock.Players[player.Index].UserAccountName);
        }

        public bool IsLocked()
        {
            return Locked;
        }

        public bool IsRegionLocked()
        {
            return RegionLock;
        }

        public bool IsRefill()
        {
            return Refill;
        }

        public void SetRefill(bool refill)
        {
            Refill = refill;
            if (refill)
                RefillItems = Terraria.Main.chest[ID].item;
            else
                RefillItems = new Terraria.Item[20];
        }

        public Terraria.Item[] GetRefillItems()
        {
            return RefillItems;
        }

        public System.Collections.Generic.List<string> GetRefillItemNames()
        {
            var list = new System.Collections.Generic.List<string>();
            for (int i = 0; i < RefillItems.Length; i++)
            {
                if (RefillItems[i] != null)
                    if (!string.IsNullOrEmpty(RefillItems[i].name))
                        list.Add(RefillItems[i].name);
            }
            if (list.Count == 0)
                list.Add("");
            return list;
        }

        public void SetRefillItems(string raw, bool set = false)
        {
            var array = raw.Split(',');
            for (int i = 0; i < array.Length && i < 20; i++)
            {
                var item = new Terraria.Item();
                item.SetDefaults(array[i]);
                RefillItems[i] = item;
            }
            //if (set)
            //    setChestItems(RefillItems);
        }

        /*public void SetChestItems(Terraria.Item[] items)
        {
            Terraria.Main.chest[ID].item = items;
        }*/

        public bool IsOpenFor(CPlayer player)
        {
            if (!IsLocked()) //if chest not locked skip all checks

                return true;

            if (!player.IsLoggedIn) //if player isn't logged in, and chest is protectect, don't allow access
                return false;

            if (IsOwner(player)) //if player is owner then skip checks

                return true;

            if (HashedPassword != "") //this chest is passworded, so check if user has unlocked this chest

                if (player.HasAccessToChest(ID)) //has unlocked this chest

                    return true;

            if (IsRegionLocked()) //if region lock then check region
            {
                var x = (int)Position.X;
                var y = (int)Position.Y;

                if (TShock.Regions.InArea(x, y)) //if not in area disable region lock
                {
                    if (TShock.Regions.CanBuild(x, y, TShock.Players[player.Index])) //if can build in area
                        return true;
                }
                else
                    regionLock(false);
            }
            return false;
        }

        public bool CheckPassword(string password)
        {
            if (HashedPassword.Equals(Utils.SHA1(password)))
            {
                return true;
            }

            return false;
        }

        public void SetPassword(string password)
        {
            if (password == "")
            {
                HashedPassword = "";
            }
            else
            {
                HashedPassword = Utils.SHA1(password);
            }
        }

        public void SetPassword(string password, bool checkForHash)
        {
            if (checkForHash)
            {
                var pattern = @"^[0-9a-fA-F]{40}$";
                if (System.Text.RegularExpressions.Regex.IsMatch(password, pattern)) //is SHA1 string

                    HashedPassword = password;
            }
            else
                SetPassword(password);
        }

        public string GetPassword()
        {
            return HashedPassword;
        }


        public static bool TileIsChest(Terraria.Tile tile)
        {
            return tile.type == 0x15;
        }

        public static bool TileIsChest(PointF position)
        {
            var x = (int)position.X;
            var y = (int)position.Y;

            return TileIsChest(x, y);
        }

        public static bool TileIsChest(int x, int y)
        {
            return TileIsChest(Terraria.Main.tile[x, y]);
        }
    }
}

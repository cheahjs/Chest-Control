using System.Collections.Generic;
using System.Linq;
using TShockAPI;
using System.Drawing;
using Terraria;

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
            var userAccountName = TShock.Players[player.Index].UserAccountName;
            if (userAccountName != null)
                Owner = userAccountName;//player.Name;
            else
            {
                Owner = TShock.Players[player.Index].Name;
                player.SendMessage("Warning, you are not registered.", Color.Red);
                //player.SendMessage("Please register an account and open the chest again to future-proof your protection.", Color.Red);
            }
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
            return Owner != "";
        }

        public bool IsOwner(CPlayer player)
        {
            return HasOwner() && Owner.Equals(TShock.Players[player.Index].UserAccountName);
        }

        public bool LegacyIsOwner(CPlayer player)
        {
            return HasOwner() && Owner.Equals(TShock.Players[player.Index].Name);
        }

        public bool IsOwnerConvert(CPlayer player)
        {
            if (LegacyIsOwner(player) && !IsOwner(player))
            {
                SetOwner(player);
                return true;
            }
            return IsOwner(player);
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
            RefillItems = refill ? Terraria.Main.chest[ID].item : new Terraria.Item[20];
        }

        public Terraria.Item[] GetRefillItems()
        {
            return RefillItems;
        }

        public System.Collections.Generic.List<string> GetRefillItemNames()
        {
            var list = (from t in RefillItems 
                        where t != null 
                        where !string.IsNullOrEmpty(t.name) 
                        select t.name + "=" + t.stack).ToList();
            if (list.Count == 0)
                list.Add("");
            return list;
        }

        public void SetRefillItems(string raw)
        {
            var array = raw.Split(',');
            for (var i = 0; i < array.Length && i < 20; i++)
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

            if (!TShock.Players[player.Index].IsLoggedIn) //if player isn't logged in, and chest is protected, don't allow access
                return false;

            if (IsOwnerConvert(player)) //if player is owner then skip checks
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
            return HashedPassword.Equals(Utils.SHA1(password));
        }

        public void SetPassword(string password)
        {
            HashedPassword = password == "" ? "" : Utils.SHA1(password);
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

        public static bool TileIsChest(Terraria.TileData tile)
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
            return TileIsChest(Terraria.Main.tile[x, y].Data);
        }
    }
}

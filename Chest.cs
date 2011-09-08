using TShockAPI;
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

        public void reset()
        {
            Owner = "";
            Locked = false;
            RegionLock = false;
            Refill = false;
            HashedPassword = "";
            RefillItems = new Terraria.Item[20];
        }

        public void setID(int id)
        {
            ID = id;
        }

        public int getID()
        {
            return ID;
        }

        public void setOwner(string player)
        {
            Owner = player;
        }

        public void setOwner(CPlayer player)
        {
            Owner = player.Name;
        }

        public string getOwner()
        {
            return Owner;
        }

        public void setPosition(PointF position)
        {
            Position = position;
        }

        public void setPosition(int x, int y)
        {
            Position = new PointF(x, y);
        }

        public PointF getPosition()
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

        public bool hasOwner()
        {
            if (Owner != "")
            {
                return true;
            }
            return false;
        }

        public bool isOwner(CPlayer player)
        {

            if (hasOwner() && Owner.Equals(player.Name))
            {
                return true;
            }

            return false;
        }

        public bool isLocked()
        {
            return Locked;
        }

        public bool isRegionLocked()
        {
            return RegionLock;
        }

        public bool IsRefill()
        {
            return Refill;
        }

        public void setRefill(bool refill)
        {
            Refill = refill;
            if (refill)
                RefillItems = Terraria.Main.chest[ID].item;
            else
                RefillItems = new Terraria.Item[20];
        }

        public Terraria.Item[] getRefillItems()
        {
            return RefillItems;
        }

        public System.Collections.Generic.List<string> getRefillItemNames()
        {
            var list = new System.Collections.Generic.List<string>();
            for (int i = 0; i < RefillItems.Length; i++)
            {
                if (!string.IsNullOrEmpty(RefillItems[i].name))
                    list.Add(RefillItems[i].name);
            }
            return list;
        }

        public void setRefillItems(string raw, bool set = false)
        {
            var array = raw.Split(',');
            for (int i = 0; i < array.Length && i < 20; i++)
            {
                var item = new Terraria.Item();
                item.SetDefaults(array[i]);
                RefillItems[i] = item;
            }
            if (set)
                setChestItems(RefillItems);
        }

        public void setChestItems(Terraria.Item[] items)
        {
            Terraria.Main.chest[ID].item = items;
        }

        public bool isOpenFor(CPlayer player)
        {
            if (!isLocked()) //if chest not locked skip all checks
            {
                return true;
            }

            if (isOwner(player)) //if player is owner then skip checks
            {
                return true;
            }

            if (HashedPassword != "") //this chest is passworded, so check if user has unlocked this chest
            {
                if (player.hasAccessToChest(ID)) //has unlocked this chest
                {
                    return true;
                }
            }

            if (isRegionLocked()) //if region lock then check region
            {
                int x = (int)Position.X;
                int y = (int)Position.Y;

                if (TShock.Regions.InArea(x, y)) //if not in area disable region lock
                {
                    if (TShock.Regions.CanBuild(x, y, TShock.Players[player.Index])) //if can build in area
                    {
                        return true;
                    }
                }
                else
                {
                    regionLock(false);
                }
            }

            return false;
        }

        public bool checkPassword(string password)
        {
            if (HashedPassword.Equals(Utils.SHA1(password)))
            {
                return true;
            }

            return false;
        }

        public void setPassword(string password)
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

        public void setPassword(string password, bool checkForHash)
        {
            if (checkForHash)
            {
                string pattern = @"^[0-9a-fA-F]{40}$";
                if (System.Text.RegularExpressions.Regex.IsMatch(password, pattern)) //is SHA1 string
                {
                    HashedPassword = password;
                }
            }
            else
            {
                setPassword(password);
            }
        }

        public string getPassword()
        {
            return HashedPassword;
        }


        public static bool TileIsChest(Terraria.Tile tile)
        {
            if (tile.type == 0x15)
            {
                return true;
            }

            return false;
        }

        public static bool TileIsChest(PointF position)
        {
            int x = (int)position.X;
            int y = (int)position.Y;

            return TileIsChest(x, y);
        }

        public static bool TileIsChest(int x, int y)
        {
            return TileIsChest(Terraria.Main.tile[x, y]);
        }
    }
}

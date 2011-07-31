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
        protected int ID;
        protected int WorldID;
        protected string Owner;
        protected Vector2 Position;
        protected bool Locked;
        protected bool RegionLock;
        private string HashedPassword;

        public Chest()
        {
            ID = -1;
            WorldID = Main.worldID;
            Owner = "";
            Position = new Vector2(0, 0);
            Locked = false;
            RegionLock = false;
            HashedPassword = "";
        }

        public void reset()
        {
            Owner = "";
            Locked = false;
            RegionLock = false;
            HashedPassword = "";
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

        public void setPosition(Vector2 position)
        {
            Position = position;
        }

        public void setPosition(int x, int y)
        {
            Position = new Microsoft.Xna.Framework.Vector2(x, y);
        }

        public Vector2 getPosition()
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

        public bool isOpenFor(CPlayer player)
        {
            int x = (int)Position.X;
            int y = (int)Position.Y;

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
                if (TShock.Regions.InArea(x, y)) //if not in area disable region lock
                {
                    if (TShock.Regions.CanBuild(x, y, player)) //if can build in area
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
                if (System.Text.RegularExpressions.Regex.IsMatch(password, pattern))
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
    }
}

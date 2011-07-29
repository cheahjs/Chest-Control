using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Security.Cryptography;
using TShockAPI;

namespace ChestControl
{
    class Utils
    {
        public static string SHA1(string input)
        {
            byte[] buffer = Encoding.ASCII.GetBytes(input);
            SHA1CryptoServiceProvider cryptoTransformSHA1 = new SHA1CryptoServiceProvider();
            string hash = BitConverter.ToString(cryptoTransformSHA1.ComputeHash(buffer)).Replace("-", "");
            return hash;
        }

        public static string GetPlayerIP(string playername)
        {
            foreach (TSPlayer player in TShock.Players)
            {
                if (player != null && player.Active)
                {
                    if (playername.ToLower() == player.Name.ToLower())
                    {
                        return player.IP;
                    }
                }
            }
            return null;
        }
    }
}

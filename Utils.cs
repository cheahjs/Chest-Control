using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using TShockAPI;

namespace ChestControl
{
    internal static class Utils
    {
        public static string SHA1(string input)
        {
            byte[] buffer = Encoding.ASCII.GetBytes(input);
            string hash;
            using (var cryptoTransformSHA1 = new SHA1CryptoServiceProvider())
                hash = BitConverter.ToString(cryptoTransformSHA1.ComputeHash(buffer)).Replace("-", "");
            return hash;
        }

        public static string GetPlayerIP(string playername)
        {
            return (from player in TShock.Players where player != null && player.Active where playername.ToLower() == player.Name.ToLower() select player.IP).FirstOrDefault();
        }
    }
}
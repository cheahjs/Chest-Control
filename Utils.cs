using System;
using System.Linq;
using System.Text;
using System.Security.Cryptography;
using TShockAPI;

namespace ChestControl
{
    static class Utils
    {
        public static string SHA1(string input)
        {
            byte[] buffer = Encoding.ASCII.GetBytes(input);
            var cryptoTransformSHA1 = new SHA1CryptoServiceProvider();
            string hash = BitConverter.ToString(cryptoTransformSHA1.ComputeHash(buffer)).Replace("-", "");
            return hash;
        }

        public static string GetPlayerIP(string playername)
        {
            return (from player in TShock.Players where player != null && player.Active where playername.ToLower() == player.Name.ToLower() select player.IP).FirstOrDefault();
        }
    }
}

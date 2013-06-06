using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using TPulseAPI;

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
    }
}
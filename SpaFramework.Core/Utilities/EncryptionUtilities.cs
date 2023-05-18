using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace SpaFramework.Core.Utilities
{
    public static class EncryptionUtilities
    {
        public static string GetHash(string message, string secret, bool makeUrlSafe = false)
        {
            secret = secret ?? "";
            var encoding = new ASCIIEncoding();
            byte[] keyByte = encoding.GetBytes(secret);
            byte[] messageBytes = encoding.GetBytes(message);
            using (var hmacsha256 = new HMACSHA256(keyByte))
            {
                byte[] hashmessage = hmacsha256.ComputeHash(messageBytes);
                string hash = Convert.ToBase64String(hashmessage);

                if (makeUrlSafe)
                    hash = hash.Replace("+", "X");

                return hash;
            }
        }

        public static string GetRandomCaseSensitiveString(int length)
        {
            const string valid = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890";
            return GetRandomString(length, valid);
        }

        public static string GetRandomCaseInsensitiveString(int length)
        {
            const string valid = "ABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890";
            return GetRandomString(length, valid);
        }

        public static string GetRandomString(int length, string valid)
        {
            StringBuilder res = new StringBuilder();
            using (RNGCryptoServiceProvider rng = new RNGCryptoServiceProvider())
            {
                byte[] uintBuffer = new byte[sizeof(uint)];

                while (length-- > 0)
                {
                    rng.GetBytes(uintBuffer);
                    uint num = BitConverter.ToUInt32(uintBuffer, 0);
                    res.Append(valid[(int)(num % (uint)valid.Length)]);
                }
            }

            return res.ToString();
        }
        
        public static string GetIdentityTokenFromDonorId(long donorId, string key)
        {
            var encoding = new ASCIIEncoding();

            string signature = GetHash(donorId.ToString(), key, false);
            string tokenValue = donorId.ToString() + "|" + signature;
            byte[] tokenBytes = encoding.GetBytes(tokenValue);

            return Convert.ToBase64String(tokenBytes);
        }

        public static long? GetDonorIdFromIdentityToken(string identityToken, string key)
        {
            var encoding = new ASCIIEncoding();

            byte[] tokenBytes = Convert.FromBase64String(identityToken);
            string tokenValue = encoding.GetString(tokenBytes);
            string[] tokenParts = tokenValue.Split('|');

            if (tokenParts.Length != 2)
                return null;

            long donorId = 0;
            if (!long.TryParse(tokenParts[0], out donorId))
                return null;

            string signature = GetHash(donorId.ToString(), key, false);
            if (signature != tokenParts[1])
                return null;

            return donorId;
        }

        public static byte[] StringToByteArray(string hex)
        {
            return Enumerable.Range(0, hex.Length)
                             .Where(x => x % 2 == 0)
                             .Select(x => Convert.ToByte(hex.Substring(x, 2), 16))
                             .ToArray();
        }

    }
}

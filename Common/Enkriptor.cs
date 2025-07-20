using System;
using System.Text;

namespace Common
{
    public static class Enkriptor
    {
        public static string Encrypt(string plainText, string key)
        {
            if (string.IsNullOrEmpty(plainText))
                return plainText;

            byte[] plainBytes = Encoding.UTF8.GetBytes(plainText);
            byte[] keyBytes = Encoding.UTF8.GetBytes(key);
            byte[] encryptedBytes = new byte[plainBytes.Length];

            for (int i = 0; i < plainBytes.Length; i++)
            {
                byte p = plainBytes[i];
                byte k = keyBytes[i % keyBytes.Length];
                encryptedBytes[i] = (byte)((p + k) % 256);
            }

            return Convert.ToBase64String(encryptedBytes);
        }

        public static string Decrypt(string cipherText, string key)
        {
            if (string.IsNullOrEmpty(cipherText))
                return cipherText;

            byte[] cipherBytes = Convert.FromBase64String(cipherText);
            byte[] keyBytes = Encoding.UTF8.GetBytes(key);
            byte[] decryptedBytes = new byte[cipherBytes.Length];

            for (int i = 0; i < cipherBytes.Length; i++)
            {
                byte c = cipherBytes[i];
                byte k = keyBytes[i % keyBytes.Length];
                decryptedBytes[i] = (byte)((256 + c - k) % 256);
            }

            return Encoding.UTF8.GetString(decryptedBytes);
        }
    }
}

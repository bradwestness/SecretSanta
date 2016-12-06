using System;
using System.Security.Cryptography;
using System.Text;

namespace SecretSanta.Utilities
{
    public static class Crypto
    {
        public static string Encrypt(string input)
        {
            var output = string.Empty;

            using (var provider = new  RSACryptoServiceProvider())
            {
                var encryptedBytes = provider.Encrypt(Encoding.UTF8.GetBytes(input), true);
                output = Encoding.UTF8.GetString(encryptedBytes);
            }

            return output;
        }

        public static bool Verify(string input, string hash)
        {
            if (string.IsNullOrWhiteSpace(input))
            {
                return false;
            }

            if (string.IsNullOrWhiteSpace(hash))
            {
                return false;
            }

            var encryptedInput = Encrypt(input.Trim());
            return encryptedInput.Equals(hash.Trim(), StringComparison.InvariantCultureIgnoreCase);
        }
    }
}
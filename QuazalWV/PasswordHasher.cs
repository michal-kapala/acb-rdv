using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace QuazalWV
{
    public class PasswordHasher
    {
        public static bool FixedTimeEquals(byte[] a, byte[] b)
        {
            if (a == null || b == null || a.Length != b.Length)
                return false;

            int result = 0;
            for (int i = 0; i < a.Length; i++)
            {
                result |= a[i] ^ b[i];
            }

            return result == 0;
        }
        public static (byte[] hash, byte[] salt) HashPassword(string password, int iterations = 100_000)
        {
            byte[] random = new byte[16];
            using (RandomNumberGenerator rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(random); // The array is now filled with cryptographically strong random bytes.
            }
            byte[] salt = random; // 128-bit salt

            var pbkdf2 = new Rfc2898DeriveBytes(password, salt, iterations, HashAlgorithmName.SHA256);
            byte[] hash = pbkdf2.GetBytes(32); // 256-bit hash

            return (hash, salt);
        }
        public static bool VerifyPassword(string password, byte[] hash, byte[] salt, int iterations = 100_000)
        {
            var pbkdf2 = new Rfc2898DeriveBytes(password, salt, iterations, HashAlgorithmName.SHA256);
            byte[] hashToCompare = pbkdf2.GetBytes(32);

            return FixedTimeEquals(hash, hashToCompare);
        }

    }
}

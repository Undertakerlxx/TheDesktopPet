using System;
using System.Security.Cryptography;

namespace DesktopPet.Accounts
{
    public static class PasswordHasher
    {
        public static string CreateSaltBase64(int size = 16)
        {
            byte[] bytes = new byte[size];
            RandomNumberGenerator.Fill(bytes);
            return Convert.ToBase64String(bytes);
        }

        public static string HashPassword(string password, string saltBase64, int iterations = 10000, int numBytes = 32)
        {
            if (password == null || string.IsNullOrEmpty(saltBase64))
            {
                return string.Empty;
            }

            byte[] saltBytes = Convert.FromBase64String(saltBase64);
            using Rfc2898DeriveBytes deriveBytes = new(password, saltBytes, iterations, HashAlgorithmName.SHA256);
            return Convert.ToBase64String(deriveBytes.GetBytes(numBytes));
        }

        public static bool VerifyPassword(string password, string saltBase64, string expectedHashBase64)
        {
            if (password == null || string.IsNullOrEmpty(saltBase64) || string.IsNullOrEmpty(expectedHashBase64))
            {
                return false;
            }

            try
            {
                string actualHash = HashPassword(password, saltBase64);
                return CryptographicOperations.FixedTimeEquals(
                    Convert.FromBase64String(actualHash),
                    Convert.FromBase64String(expectedHashBase64));
            }
            catch (FormatException)
            {
                return false;
            }
        }
    }
}

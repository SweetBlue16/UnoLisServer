using System.Text;
using System.Security.Cryptography;

namespace UnoLisServer.Common.Helpers
{
    /// <summary>
    /// Class that provides methods for hashing and verifying passwords.
    /// </summary>
    public static class PasswordHelper
    {
        public static string HashPassword(string password)
        {
            using (SHA256 sha = SHA256.Create())
            {
                byte[] bytes = sha.ComputeHash(Encoding.UTF8.GetBytes(password));
                StringBuilder sb = new StringBuilder();
                foreach (byte b in bytes)
                {
                    sb.Append(b.ToString("x2"));
                }
                return sb.ToString();
            }
        }

        public static bool VerifyPassword(string plainText, string hashed)
        {
            return HashPassword(plainText) == hashed;
        }
    }
}

using System.Security.Cryptography;
using System.Text;

namespace SmartBizERP.Utilities
{
    public static class PasswordHasher
    {
        // Must match exactly how the SQL seed script hashes passwords:
        // CONVERT(NVARCHAR(64), HASHBYTES('SHA2_256', N'...'), 2)
        //
        // SQL Server's HASHBYTES on an NVARCHAR (the N'...' prefix) hashes
        // the UTF-16LE byte representation of the string -- so on the C#
        // side we must use Encoding.Unicode (which IS UTF-16LE), not
        // Encoding.UTF8, or the hashes will never match even for the
        // exact same password.
        public static string Hash(string plainTextPassword)
        {
            using (var sha256 = SHA256.Create())
            {
                byte[] bytes = sha256.ComputeHash(Encoding.Unicode.GetBytes(plainTextPassword));
                var builder = new StringBuilder();
                foreach (byte b in bytes)
                {
                    builder.Append(b.ToString("X2")); // uppercase hex, matches SQL's style-2 output
                }
                return builder.ToString();
            }
        }

        // Case-insensitive compare, since hex casing conventions can
        // differ between tools even when the underlying bytes are identical.
        public static bool Verify(string plainTextPassword, string storedHash)
        {
            string computedHash = Hash(plainTextPassword);
            return string.Equals(computedHash, storedHash, System.StringComparison.OrdinalIgnoreCase);
        }
    }
}

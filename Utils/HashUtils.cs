using System.Security.Cryptography;
using System.Text;

namespace Y2DL.Utils;

public class HashUtils
{
    public static string HashThingToSHA256String(object obj)
    {
        using (var sha256 = SHA256.Create())
        {
            var data = Encoding.UTF8.GetBytes(obj.ToString()); // Convert the object to string
            var hashBytes = sha256.ComputeHash(data);
            var hashString = BitConverter.ToString(hashBytes).Replace("-", "").ToLower();
            return hashString;
        }
    }
}
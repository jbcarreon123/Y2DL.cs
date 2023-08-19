using System.Security.Cryptography;
using System.Text;

namespace Y2DL.Utils;

public class Hashing
{
    public static string HashClassToSHA256String(object obj)
    {
        using (SHA256 sha256 = SHA256.Create())
        {
            byte[] data = Encoding.UTF8.GetBytes(obj.ToString()); // Convert the object to string
            byte[] hashBytes = sha256.ComputeHash(data);
            string hashString = BitConverter.ToString(hashBytes).Replace("-", "").ToLower();
            return hashString;
        }
    }
}
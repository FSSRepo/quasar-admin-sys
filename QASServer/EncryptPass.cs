using System.Security.Cryptography;
using System;

public class EncryptPass
{
    public static string Encrypt(string password)
    {
        byte[] salt;
        new RNGCryptoServiceProvider().GetBytes(salt = new byte[16]);
        var pbdfk = new Rfc2898DeriveBytes(password, salt, 10000);
        byte[] hash = pbdfk.GetBytes(20);
        byte[] hash_bytes = new byte[36];
        Array.Copy(salt, 0, hash_bytes, 0, 16);
        Array.Copy(hash, 0, hash_bytes, 16, 20);
        return Convert.ToBase64String(hash_bytes);
    }

    public static bool Validate(string password,string encryptedPassword) {
        byte[] hashBytes = Convert.FromBase64String(encryptedPassword);
        byte[] salt = new byte[16];
        Array.Copy(hashBytes, 0, salt, 0, 16);
        var pbdfk = new Rfc2898DeriveBytes(password, salt, 10000);
        byte[] hash = pbdfk.GetBytes(20);
        bool result = true;
        for (int i = 0; i < 20; i++)
        {
            if (hashBytes[16 + i] != hash[i])  {
                result = false;
            }
        }
        return result;
    }
}

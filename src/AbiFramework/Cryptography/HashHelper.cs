using System;
using System.Security.Cryptography;
using System.Text;

namespace AbiFramework.Cryptography;

public static class HashHelper
{
    public static string CreateSalt()
    {
        byte[] data = new byte[0x10];

        using var cryptoServiceProvider = RandomNumberGenerator.Create();
        cryptoServiceProvider.GetBytes(data);
        return Convert.ToBase64String(data);
    }

    public static string EncryptPasswordSha512(string password, string salt)
    {
        string saltedPassword = $"{salt}{password}";
        byte[] saltedPasswordAsBytes = Encoding.UTF8.GetBytes(saltedPassword);
        return Convert.ToBase64String(SHA512.HashData(saltedPasswordAsBytes));
    }
}

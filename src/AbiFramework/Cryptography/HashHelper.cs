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

    /// <summary>
    /// Generates a 128-bit salt using a secure PRNG
    /// </summary>
    /// <returns>Salt as byte array</returns>
    public static byte[] GenerateSalt()
    {
        // generate a 128-bit salt using a secure PRNG
        byte[] salt = new byte[128 / 8];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(salt);
        return salt;
    }

    /// <summary>
    /// Hashes a password using PBKDF2 with HMACSHA256
    /// </summary>
    /// <param name="salt">Salt as byte array</param>
    /// <param name="password">Password to hash</param>
    /// <param name="iterationCount">Number of iterations (default: 1000)</param>
    /// <returns>Hashed password as base64 string</returns>
    public static string HashPasswordPbkdf2(byte[] salt, string password, int iterationCount = 1000)
    {
        byte[] passwordBytes = Encoding.UTF8.GetBytes(password);
        byte[] hashed = Rfc2898DeriveBytes.Pbkdf2(
            password: passwordBytes,
            salt: salt,
            iterations: iterationCount,
            hashAlgorithm: HashAlgorithmName.SHA256,
            outputLength: 256 / 8);

        return Convert.ToBase64String(hashed);
    }

    /// <summary>
    /// Hashes a password using PBKDF2 with HMACSHA256
    /// </summary>
    /// <param name="salt">Salt as base64 string</param>
    /// <param name="password">Password to hash</param>
    /// <param name="iterationCount">Number of iterations (default: 1000)</param>
    /// <returns>Hashed password as base64 string</returns>
    public static string HashPasswordPbkdf2(string salt, string password, int iterationCount = 1000)
    {
        byte[] saltBytes = Convert.FromBase64String(salt);
        return HashPasswordPbkdf2(saltBytes, password, iterationCount);
    }
}

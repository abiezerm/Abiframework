using System;
using System.Security.Cryptography;
using System.Text;

namespace AbiFramework.Cryptography
{
   public class HashHelper
   {
        public static string CreateSalt()
        {
            var data = new byte[0x10];

            var cryptoServiceProvider = RandomNumberGenerator.Create();
            cryptoServiceProvider.GetBytes(data);
            return Convert.ToBase64String(data);
        }

        public static string EncryptPasswordSha512(string password, string salt)
        {
            using (var sha = SHA512.Create())
            {
                var saltedPassword = $"{salt}{password}";
                var saltedPasswordAsBytes = Encoding.UTF8.GetBytes(saltedPassword);
                return Convert.ToBase64String(sha.ComputeHash(saltedPasswordAsBytes));
            }
        }
    }
}
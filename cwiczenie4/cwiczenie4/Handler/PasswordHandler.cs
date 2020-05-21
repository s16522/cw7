using System;
using System.Text;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;

namespace cwiczenie3.Handler
{
    public class PasswordHandler
    {
        public static string CreateHash(string password, string salt)
        {
            var valueBytes = KeyDerivation.Pbkdf2(
                password: password,
                salt: Encoding.UTF8.GetBytes(salt),
                prf: KeyDerivationPrf.HMACSHA512,
                iterationCount: 1000,
                numBytesRequested: 256 / 8);
            return Convert.ToBase64String(valueBytes);
        }
    }
}
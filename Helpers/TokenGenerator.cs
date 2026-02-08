using System.Security.Cryptography;
using Microsoft.AspNetCore.WebUtilities;

namespace task.Helpers
{
    public static class TokenGenerator
    {
        public static string CreateToken()  //generates a secure random URL token
        {
            var bytes = RandomNumberGenerator.GetBytes(32);
            return WebEncoders.Base64UrlEncode(bytes);
        }
    }
}
using System.Security.Claims;

namespace cwiczenie3.Services
{
    public class AuthenticationResult
    {
        public Claim[] Claims { get; set; }
        public string RefreshToken { get; set; }
    }
}
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace artgallery_server.Utils
{
    public class ValidJwtKey
    {
        public static SymmetricSecurityKey ValidateJwtKey()
        {
            var secret = Environment.GetEnvironmentVariable("JWT_KEY")
                         ?? throw new InvalidOperationException("JWT_KEY is not set.");

            var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret));
            return signingKey;
        }
    }
}

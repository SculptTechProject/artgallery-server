using artgallery_server.DTO.Admin;
using artgallery_server.Infrastructure;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using artgallery_server.Utils;
using Microsoft.EntityFrameworkCore;

namespace artgallery_server.Controllers
{
    [Controller]
    [Route("api/v1/admin/")]
    public class AdminController : ControllerBase
    {
        private readonly AppDbContext _db;
        private readonly SymmetricSecurityKey SigningKey;

        public AdminController(AppDbContext db, SymmetricSecurityKey signingKey)
        {
            _db = db;
            SigningKey = signingKey;
        }
        
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] AdminDto req)
        {
            var user = await _db.Admins.FirstOrDefaultAsync(u => u.Username == req.Username && u.IsActive);
            if (user is null) return Unauthorized();

            var ok = BCrypt.Net.BCrypt.Verify(req.Password, user.PasswordHash);
            if (!ok) return Unauthorized();

            var creds  = new SigningCredentials(SigningKey, SecurityAlgorithms.HmacSha256);
            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Username),
                new Claim(JwtRegisteredClaimNames.Iss, "ArtGalleryBackend"),
                new Claim(JwtRegisteredClaimNames.Aud, "artgallery_api"),
                new Claim(ClaimTypes.Role, "Admin")
            };

            var token = new JwtSecurityToken(
                claims: claims,
                notBefore: DateTime.UtcNow,
                expires: DateTime.UtcNow.AddHours(2),
                signingCredentials: creds
            );

            return Ok(new { token = new JwtSecurityTokenHandler().WriteToken(token) });
        }
    }
}

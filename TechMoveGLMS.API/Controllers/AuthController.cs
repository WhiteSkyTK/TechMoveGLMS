using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using TechMoveGLMS.API.DTOs;

namespace TechMoveGLMS.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IConfiguration _config;

        public AuthController(IConfiguration config) => _config = config;

        /// <summary>
        /// Login with admin credentials to receive a JWT token.
        /// Use the token as: Authorization: Bearer {token}
        /// </summary>
        [HttpPost("login")]
        [ProducesResponseType(typeof(AuthResponseDto), 200)]
        [ProducesResponseType(401)]
        public IActionResult Login([FromBody] LoginDto dto)
        {
            var expectedUser = _config["AdminCredentials:Username"];
            var expectedPass = _config["AdminCredentials:Password"];

            if (dto.Username != expectedUser || dto.Password != expectedPass)
                return Unauthorized(new { message = "Invalid credentials." });

            var token  = GenerateToken(dto.Username);
            var expiry = DateTime.UtcNow.AddHours(
                double.Parse(_config["JwtSettings:ExpiryInHours"] ?? "8"));

            return Ok(new AuthResponseDto
            {
                Token    = token,
                Username = dto.Username,
                Expiry   = expiry
            });
        }

        private string GenerateToken(string username)
        {
            var jwt = _config.GetSection("JwtSettings");
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt["Secret"]!));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(ClaimTypes.Name,             username),
                new Claim(ClaimTypes.Role,             "Admin"),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(JwtRegisteredClaimNames.Iss, jwt["Issuer"]!),
                new Claim(JwtRegisteredClaimNames.Aud, jwt["Audience"]!)
            };

            var token = new JwtSecurityToken(
                issuer:             jwt["Issuer"],
                audience:           jwt["Audience"],
                claims:             claims,
                expires:            DateTime.UtcNow.AddHours(
                                        double.Parse(jwt["ExpiryInHours"] ?? "8")),
                signingCredentials: creds);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}

using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using Microsoft.IdentityModel.Tokens;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using auth_app_backend.Model;
using Microsoft.Extensions.Configuration;

namespace auth_app_backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly CouchDbService _couchDbService;
        private readonly IConfiguration _configuration;

        public AuthController(CouchDbService couchDbService, IConfiguration configuration)
        {
            _couchDbService = couchDbService;
            _configuration = configuration;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            if (request == null || string.IsNullOrEmpty(request.Email) || string.IsNullOrEmpty(request.Password))
            {
                return BadRequest(new { message = "Invalid LoginRequest" });
            }

            var user = await _couchDbService.GetUserByEmailAsync(request.Email);
            if (user == null || user.Password != request.Password)
            {
                return Unauthorized(new { message = "Invalid credentials." });
            }

            var token = GenerateJwtToken(user);
            return Ok(new { Token = token, User = user });
        }

        private string GenerateJwtToken(User user)
        {
            var jwtSettings = _configuration.GetSection("JwtSettings");
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings["Key"] ?? throw new ArgumentNullException("JwtSettings:Key")));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Email ?? throw new ArgumentNullException("Email")),
                new Claim(ClaimTypes.Role, user.Role ?? "User"), // Defaults to User if role is null
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            var token = new JwtSecurityToken(
                issuer: jwtSettings["Issuer"],
                audience: jwtSettings["Audience"],
                claims: claims,
                expires: DateTime.Now.AddMinutes(double.Parse(jwtSettings["ExpiresInMinutes"] ?? "60")),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] User user)
        {
            if (ModelState.IsValid)
            {
                var existingUser = await _couchDbService.GetUserByEmailAsync(user.Email ?? throw new ArgumentNullException("Email"));
                if (existingUser != null)
                {
                    return BadRequest("User already exists.");
                }

                await _couchDbService.AddUserAsync(user);
                return Ok(new { message = "Registration Successful" });
            }

            return BadRequest(ModelState);
        }

        public class LoginRequest
        {
            public string Email { get; set; } = string.Empty;
            public string Password { get; set; } = string.Empty;
        }

        [Authorize(Roles = "Admin")]
        [HttpGet("admin-dashboard")]
        public IActionResult AdminDashboard()
        {
            return Ok("Admin Dashboard Accessed");
        }

        [Authorize(Roles = "User")]
        [HttpGet("user-dashboard")]
        public IActionResult UserDashboard()
        {
            return Ok("User Dashboard Accessed");
        }
    }
}

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using auth_app_backend.Model;
using auth_app_backend.Services;
using System.Diagnostics;

namespace auth_app_backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly CouchDbService _couchDbService;
        private readonly string _jwtKey;

        public AuthController(CouchDbService couchDbService, IConfiguration configuration)
        {
            _couchDbService = couchDbService;
            _jwtKey = configuration["JwtSettings:Key"];
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] User user)
        {
            user.status = "Pending";
            user.Id = await GenerateUniqueRandomId();
            await _couchDbService.AddUserAsync(user);
            return Ok(new { message = "User registered successfully. Awaiting approval." });
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto loginDto)
        {
            // Fetch user based on email
            var user = await _couchDbService.GetUserByEmailAsync(loginDto.Email);
            if (user == null || !user.password.Equals(loginDto.Password))
            {
                return Unauthorized("Invalid credentials.");
            }

            if (!user.status.Equals("Approved", StringComparison.OrdinalIgnoreCase))
            {
                return Unauthorized("User is not approved for login.");
            }

            var token = GenerateJwtToken(user);
            Debug.WriteLine(user.firstName);
            Debug.WriteLine(user.email);
            return Ok(new
            {
                token = token,
                user = new
                {
                    id = user._id, // Use the _id as the unique identifier
                    email = user.email, // Align with the frontend email field
                    role = user.role,
                    status = user.status
                }
            });
        }



        private string GenerateJwtToken(User user)
        {
            var claims = new[] {
                new Claim(ClaimTypes.NameIdentifier, user.Id),
                new Claim(ClaimTypes.Email, user.email),
                new Claim(ClaimTypes.Role, user.role)
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtKey));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var token = new JwtSecurityToken(
                issuer: "yourissuer",
                audience: "youraudience",
                claims: claims,
                expires: DateTime.Now.AddMinutes(40),
                signingCredentials: creds);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        private async Task<string> GenerateUniqueRandomId()
        {
            Random random = new Random();
            string id;

            do
            {
                id = random.Next(1, 1000000).ToString();
            } while (await _couchDbService.UserIdExistsAsync(id));

            return id;
        }
    }

    public class LoginDto
    {
        public string Email { get; set; }
        public string Password { get; set; }
    }
}

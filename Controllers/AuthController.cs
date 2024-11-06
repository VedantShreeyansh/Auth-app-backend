using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using auth_app_backend.Model;
using auth_app_backend.Services;

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
            user.status = "Pending"; // Set initial status to Pending
            user.Id = await GenerateUniqueRandomId(); // Await GenerateUniqueRandomId for the async result
            await _couchDbService.AddUserAsync(user);
            return Ok(new { message = "User registered successfully. Awaiting approval." }); // Return JSON response
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto loginDto)
        {
            var user = await _couchDbService.GetUserByEmailAsync(loginDto.Email);
            if (user == null || user.password != loginDto.Password)
            {
                return Unauthorized("Invalid credentials.");
            }
            if (user.status != "Approved")
            {
                return Unauthorized("User is not approved for login.");
            }

            var token = GenerateJwtToken(user);
            return Ok(new
            {
                token = token,
                user = new
                {
                    id = user.Id,
                    Email = user.email,
                    Role = user.role,
                    Status = user.status
                }
            });
        }

        private string GenerateJwtToken(User user)
        {
            var claims = new[] {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()), // Convert Id to string
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

        private async Task<int> GenerateUniqueRandomId() // Marking as async
        {
            Random random = new Random();
            int id;

            do
            {
                id = random.Next(1, 1000000); // Generate a random number
            } while (await _couchDbService.UserIdExistsAsync(id.ToString())); // Check for unique ID

            return id; // Return the unique ID
        }
    }

    public class LoginDto
    {
        public string Email { get; set; }
        public string Password { get; set; }
    }
}

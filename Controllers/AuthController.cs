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
using auth_app_backend.Dto;

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
            try
            {
                user.status = "Pending";
                user._id = await GenerateUniqueRandomId();
            //   user._rev = null; // Ensure _rev is null for new users
                await _couchDbService.AddUserAsync(user);
                return Ok(new { message = "User registered successfully. Awaiting approval." });
            }
            catch (Exception ex)
            {
                // Improved error logging
                Console.Error.WriteLine($"Registration error: {ex.Message}");
                return BadRequest(new { message = "Registration failed. Please check the input data." });
            }
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto loginDto)
        {
            var user = await _couchDbService.GetUserByEmailAsync(loginDto.Email);
            if (user == null || !user.password.Equals(loginDto.Password))
            {
                return Unauthorized("Invalid credentials.");
            }

            var token = GenerateJwtToken(user);
            return Ok(new
            {
                token = token,
                user = new
                {
                    id = user._id,
                    email = user.email,
                    role = user.role,
                    status = user.status
                }
            });
        }

        [HttpPut("User/{id}/approve")]
        public async Task<IActionResult> ApproveUser(string id, [FromBody] ApproveUserDto approveUserDto)
        {
            try
            {
                var user = await _couchDbService.GetUserByIdAsync(id);
                if (user == null)
                {
                    return NotFound("User not found.");
                }

                user.role = approveUserDto.Role;
                user.status = "Approved";
                await _couchDbService.UpdateUserAsync(user);

                return Ok(new { message = "User approved successfully." });
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Approval error: {ex.Message}");
                return BadRequest(new { message = "Approval failed. Please try again." });
            }
        }
        private string GenerateJwtToken(User user)
        {
            var claims = new[] {
                new Claim(ClaimTypes.NameIdentifier, user._id),
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


using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using BookWebApi.DTOs;
using BookWebApi.Models;

namespace BookWebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly BookWebApi.Data.AppDbContext _context;
        private readonly IConfiguration _configuration;

        public AuthController(BookWebApi.Data.AppDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        [HttpPost("register")]
        public IActionResult Register(RegisterDto register)
        {
            if (string.IsNullOrWhiteSpace(register.UserName) || string.IsNullOrWhiteSpace(register.Password))
                return BadRequest("Username and password are required.");

            if (_context.Set<Customer>().Any(c => c.UserName == register.UserName))
                return Conflict("Username already exists.");

            var passwordHash = Convert.ToBase64String(Encoding.UTF8.GetBytes(register.Password));

            var customer = new Customer
            {
                Name = register.Name,
                Email = register.Email,
                Mobile = "",
                Address = "",
                UserName = register.UserName,
                PasswordHash = passwordHash
            };

            _context.Add(customer);
            _context.SaveChanges();

            return Ok();
        }

        [HttpPost("login")]
        public ActionResult<AuthResponseDto> Login(LoginDto login)
        {
            if (string.IsNullOrWhiteSpace(login.UserName) || string.IsNullOrWhiteSpace(login.Password))
                return BadRequest("Username and password are required.");

            var user = _context.Set<Customer>().FirstOrDefault(c => c.UserName == login.UserName);
            if (user == null)
                return Unauthorized();

            var providedHash = Convert.ToBase64String(Encoding.UTF8.GetBytes(login.Password));
            if (user.PasswordHash != providedHash)
                return Unauthorized();

            var token = GenerateJwtToken(user);
            return Ok(new AuthResponseDto { Token = token });
        }

        [HttpPost("forgot-password")]
        public IActionResult ForgotPassword(ForgotPasswordDto forgotPassword)
        {
            if (string.IsNullOrWhiteSpace(forgotPassword.Email))
                return BadRequest("Email is required.");

            var user = _context.Set<Customer>().FirstOrDefault(c => c.Email == forgotPassword.Email);
            if (user == null)
                return NotFound("User with the provided email not found.");

            // Generate a temporary password
            var tempPassword = GenerateTemporaryPassword();
            var tempPasswordHash = Convert.ToBase64String(Encoding.UTF8.GetBytes(tempPassword));

            // Update user's password
            user.PasswordHash = tempPasswordHash;
            _context.SaveChanges();

            // In a real application, you would send this temporary password via email
            // For now, we'll return it in the response (not recommended for production)
            return Ok(new { Message = "Password reset successful. Check your email for the temporary password.", TemporaryPassword = tempPassword });
        }

        private string GenerateJwtToken(Customer user)
        {
            var jwtSettings = _configuration.GetSection("JwtSettings");
            var key = jwtSettings.GetValue<string>("SecretKey");
            var issuer = jwtSettings.GetValue<string>("Issuer");
            var audience = jwtSettings.GetValue<string>("Audience");

            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var claims = new[] {
                new Claim(JwtRegisteredClaimNames.Sub, user.UserName),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim("id", user.Id.ToString())
            };

            var expiryMinutes = jwtSettings.GetValue<int>("ExpiryMinutes");

            var token = new JwtSecurityToken(
                issuer: issuer,
                audience: audience,
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(expiryMinutes),
                signingCredentials: credentials
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        private string GenerateTemporaryPassword()
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
            var random = new Random();
            return new string(Enumerable.Repeat(chars, 8)
                .Select(s => s[random.Next(s.Length)]).ToArray());
        }
    }
}

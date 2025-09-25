using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using BookWebApi.DTOs;
using BookWebApi.Models;
using System.Linq;

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

        private const int DEFAULT_PAGE_SIZE = 10;

        /// <summary>
        /// Returns a paginated list of users along with pagination metadata.
        /// Sensitive data like passwords are excluded and emails are masked.
        /// </summary>
        /// <param name="page">The 1-based page number to return. Must be greater than 0.</param>
        /// <param name="pageSize">The number of items per page. If not supplied, a default is used.</param>
        /// <returns>An object containing the page data and pagination numbers (current, previous, next).</returns>
        [HttpGet("users")]
        [Microsoft.AspNetCore.Authorization.Authorize]
        public ActionResult<object> GetUsersPaged([FromQuery] int page = 1, [FromQuery] int pageSize = DEFAULT_PAGE_SIZE)
        {
            try
            {
                if (page <= 0 || pageSize <= 0)
                {
                    return BadRequest("Both 'page' and 'pageSize' must be greater than zero.");
                }

                var allUsers = _context.Set<Customer>().ToList();
                
                var totalCount = allUsers.Count;
                var totalPages = (int)Math.Ceiling((double)totalCount / pageSize);
                totalPages = Math.Max(1, totalPages);

                if (page > totalPages)
                {
                    return BadRequest($"Requested page '{page}' exceeds total pages '{totalPages}'.");
                }

                var startIndex = (page - 1) * pageSize;
                var takeCount = Math.Min(pageSize, Math.Max(0, totalCount - startIndex));
                var pageUsers = startIndex < totalCount ? allUsers.GetRange(startIndex, takeCount) : new();

                // Map to UserResponseDto and mask emails
                var userResponseList = pageUsers.Select(user => new UserResponseDto
                {
                    Id = user.Id,
                    Name = user.Name,
                    Email = MaskEmail(user.Email),
                    Mobile = user.Mobile,
                    Address = user.Address,
                    UserName = user.UserName
                }).ToList();

                int? prevPage = page > 1 ? page - 1 : null;
                int? nextPage = page < totalPages ? page + 1 : null;

                var responsePayload = new
                {
                    Data = userResponseList,
                    CurrentPage = page,
                    PrevPage = prevPage,
                    NextPage = nextPage
                };

                return Ok(responsePayload);
            }
            catch (Exception)
            {
                return StatusCode(500, "An error occurred while retrieving paginated users.");
            }
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

        private string? MaskEmail(string? email)
        {
            if (string.IsNullOrWhiteSpace(email))
                return email;

            var atIndex = email.IndexOf('@');
            if (atIndex <= 1)
                return email; // Can't mask effectively

            var localPart = email.Substring(0, atIndex);
            var domainPart = email.Substring(atIndex);

            // Keep first character and last character before @, mask the middle
            if (localPart.Length <= 2)
            {
                return $"{localPart[0]}*{domainPart}";
            }
            
            var maskedLength = localPart.Length - 2;
            var masked = new string('*', maskedLength);
            return $"{localPart[0]}{masked}{localPart[localPart.Length - 1]}{domainPart}";
        }
    }
}

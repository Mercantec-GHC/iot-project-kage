using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using IotProject.API.Data;
using IotProject.Shared.Models.Database;
using IotProject.Shared.Models.Requests;
using IotProject.Shared.Models.Responses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace IotProject.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController(AppDbContext context, IConfiguration configuration) : ControllerBase
    {
        [HttpPost("Register")]
        public async Task<ActionResult<UserCreateResponse>> Register(UserCreateRequest requestModel) 
        {
            // Checks for errors in user information.
            if (requestModel == null) return BadRequest();
            if (string.IsNullOrEmpty(requestModel.FirstName) || string.IsNullOrEmpty(requestModel.LastName)) return BadRequest("User information must not be empty.");
            if (requestModel.Password != requestModel.ConfirmPassword) return BadRequest("Passwords do not match.");
            if (!IsPasswordSecure(requestModel.Password)) return BadRequest("Password not secure.");
            if (!IsValidEmail(requestModel.Email)) return BadRequest("Email is not valid.");
            if (await context.Users.FirstOrDefaultAsync(u => u.Email == requestModel.Email) != null) return BadRequest("Email is already in use.");

            // Maps a new user.
            var user = new User
            {
                Id = await GenerateUserID(),
                Email = requestModel.Email.ToLower(),
                Password = BCrypt.Net.BCrypt.HashPassword(requestModel.Password),
                FirstName = requestModel.FirstName.Trim(),
                LastName = requestModel.LastName.Trim(),
                DateCreated = DateTime.UtcNow,
                DateUpdated = DateTime.UtcNow
            };

            // Adds & saves the user to the database.
            await context.Users.AddAsync(user);
            await context.SaveChangesAsync();

            return StatusCode(201, new UserCreateResponse(user.Id, Message: "User successfully registered."));
        }

        [HttpPost("Login")]
        public async Task<ActionResult<UserLoginResponse>> Login(UserLoginRequest requestModel)
        {
            // Checks for errors in user information.
            if (requestModel == null) return BadRequest();
            if (string.IsNullOrEmpty(requestModel.Email) || string.IsNullOrEmpty(requestModel.Password)) return BadRequest("Login information must not be empty.");
            if (!IsValidEmail(requestModel.Email)) return BadRequest("Email is not valid.");
            
            // Finds user and verifies password.
            var user = await context.Users.FirstOrDefaultAsync(u => u.Email == requestModel.Email.ToLower());
            if (user == null) return NotFound($"User with email: {requestModel.Email.ToLower()} was not found.");
            if (!BCrypt.Net.BCrypt.Verify(requestModel.Password, user.Password)) return BadRequest("Incorrect password!");

            // Generates a JWT token for the given user.
            var token = GenerateJwtToken(user);
            
            return Ok(new UserLoginResponse(token, 1800, ":-)", Message: "User successfully logged in.")); // ":-)" to be replaced by proper refresh token.
        }

        [HttpGet("Me"), Authorize]
        public async Task<ActionResult<UserMeResponse>> Me()
        {
            // Attempts to find the current user from the controller user context.
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null) return StatusCode(500);
            var user = await context.Users.FirstOrDefaultAsync(u => u.Id == userId);
            if (user == null) return StatusCode(500);

            // Creates an empty list of roles.
            var Roles = new List<string>();

            return Ok(new UserMeResponse(user.Id, user.FirstName, user.LastName, user.Email, Roles));
        }

        // Checks if the password is valid using RegEx.
        private bool IsPasswordSecure(string password)
        {
            if (string.IsNullOrEmpty(password)) return false;

            // Regex to check if the password meets the following criteria:
            // ^               - Ensures the match starts at the beginning of the string.
            // (?=.*[A-Z])     - Asserts that there is at least one uppercase letter in the string.
            // (?=.*[a-z])     - Asserts that there is at least one lowercase letter in the string.
            // (?=.*\d)        - Asserts that there is at least one digit (number) in the string.
            // (?=.*[\W_])     - Asserts that there is at least one special character (non-word character or underscore).
            // [^\s]{8,}       - Ensures the string is at least 8 characters long and does not contain any whitespace.
            // $               - Ensures the match ends at the end of the string.
            var regex = new System.Text.RegularExpressions.Regex(@"^(?=.*[A-Z])(?=.*[a-z])(?=.*\d)(?=.*[\W_])[^\s]{8,}$");
            return regex.IsMatch(password);
        }

        // Checks if the email is valid using RegEx.
        private bool IsValidEmail(string email)
        {
            if (string.IsNullOrEmpty(email)) return false;

            // Regex to validate email format
            // ^                  - Ensures the match starts at the beginning of the string.
            // [a-zA-Z0-9._%+-]+  - Matches the local part of the email (letters, digits, dots, underscores, etc.).
            // @                  - Matches the "@" symbol.
            // [a-zA-Z0-9.-]+     - Matches the domain name (letters, digits, dots, and hyphens).
            // \.                 - Matches the dot before the domain extension.
            // [a-zA-Z]{2,}$      - Matches the domain extension (at least 2 letters).
            var regex = new System.Text.RegularExpressions.Regex(@"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$");
            return regex.IsMatch(email);
        }
         
        // Generates a GUID for the userID, checks if GUID exists in the database.
        private async Task<string> GenerateUserID()
        {
            while (true)
            {
                var userId = Guid.NewGuid().ToString();
                if (await context.Users.FindAsync(userId) == null) return userId;
            }
        }

        // Generates a JWT token for a user, and returns the token as a string.
        private string GenerateJwtToken(User user)
        {
            var JWT = configuration.GetSection("JWT"); // Fetches JWT section from the configuration.
            var Issuer = Environment.GetEnvironmentVariable("JWT_ISSUER") ?? JWT["Issuer"];
            var Audience = Environment.GetEnvironmentVariable("JWT_AUDIENCE") ?? JWT["Audience"];
            var Secret = Environment.GetEnvironmentVariable("JWT_SECRET") ?? JWT["Secret"];
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Secret!)); // Builds a symmectric security key using the JWT secret.
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256); // Builds signature for the key using HmacSha256 algorithm.

            // Creates list of claims for the user.
            var claims = new List<Claim> 
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Id),
                new Claim("Email", user.Email),
                new Claim("FirstName", user.FirstName),
                new Claim("LastName", user.LastName)
            };

            // Builds the JWT token from the given objects.
            var token = new JwtSecurityToken(
                issuer: Issuer,
                audience: Audience,
                claims: claims,
                expires: DateTime.Now.AddMinutes(30),
                signingCredentials: creds);
                
            // Returns the token as a string.
            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using BCrypt.Net;
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
    [Route("[controller]")]
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
            var jwtToken = GenerateJwtToken(user);

            // Generates a unique token to be used instead of the users email and password, with a lifetime of 7 days.
            var refreshToken = await GenerateRefreshToken();
            context.RefreshTokens.Add(new RefreshToken
            {
                Id = refreshToken,
                UserId = user.Id,
                ExpiryDate = DateTime.UtcNow.AddDays(7),
                IsRevoked = false
            });
            await context.SaveChangesAsync();

            return Ok(new UserLoginResponse(jwtToken, 1800, refreshToken, Message: "User successfully logged in."));
        }

        [HttpPost("Refresh")]
        public async Task<ActionResult<UserLoginResponse>> Refresh(UserRefreshRequest requestModel)
        {
            // Finds the Refresh token and includes the User object.
            var token = await context.RefreshTokens.Where(t => t.Id == requestModel.Token)
                .Include(t => t.User)
                .FirstOrDefaultAsync();
            if (token == null || token.User == null) return BadRequest(string.Empty);
            if (token.IsRevoked) return BadRequest("Refresh token has already been used.");
            if (token.ExpiryDate < DateTime.UtcNow) return BadRequest("Refresh token has expired.");

            // Marks the Refresh token as used.
            token.IsRevoked = true;

            // Generates a JWT token for the given user.
            var jwtToken = GenerateJwtToken(token.User);

            // Generates a unique token to be used instead of the users email and password, with a lifetime of 7 days.
            var refreshToken = await GenerateRefreshToken();
            context.RefreshTokens.Add(new RefreshToken
            {
                Id = refreshToken,
                UserId = token.User.Id,
                ExpiryDate = DateTime.UtcNow.AddDays(7),
                IsRevoked = false
            });
            await context.SaveChangesAsync();

            return Ok(new UserLoginResponse(jwtToken, 1800, refreshToken, Message: "User successfully logged in."));
        }

        [HttpGet("Me"), Authorize]
        public async Task<ActionResult<UserMeResponse>> Me()
        {
            // Attempts to find the current user from the controller user context.
            var user = await GetSignedInUser();
            if (user == null) return StatusCode(500);
            
            return Ok(new UserMeResponse(user.Id, user.FirstName, user.LastName, user.Email, user.Role.ToString()));
        }

        [HttpPatch("UpdateInfo"), Authorize]
        public async Task<ActionResult<UserLoginResponse>> UpdateInfo(UserEditInformationRequest userEditInfo)
        {
            var user = await GetSignedInUser();
            if (user == null) return StatusCode(500);

            bool isChanged = false;
            if (userEditInfo == null) return BadRequest();
            
            // Change user email.
            if (userEditInfo.Email == null && userEditInfo.FirstName == null && userEditInfo.LastName == null) return BadRequest("Can't change nothing.");
            if (userEditInfo.Email != null && user.Email != userEditInfo.Email)
            {
                if (IsValidEmail(userEditInfo.Email) && !(await context.Users.AnyAsync(u => u.Email == userEditInfo.Email)))
                {
                    user.Email = userEditInfo.Email;
                    isChanged = true;
                }
                else return BadRequest("Email is not valid.");
            }
            
            // Change user first name.
            if (!string.IsNullOrWhiteSpace(userEditInfo.FirstName) && user.FirstName != userEditInfo.FirstName)
            {
                user.FirstName = userEditInfo.FirstName.Trim();
                isChanged = true;
            }

            // Change user last name.
            if (!string.IsNullOrWhiteSpace(userEditInfo.LastName) && user.LastName != userEditInfo.LastName)
            {
                user.LastName = userEditInfo.LastName.Trim();
                isChanged = true;
            }

            if (isChanged)
            {
                await context.SaveChangesAsync();

                // Generates a JWT token for the given user.
                var jwtToken = GenerateJwtToken(user);

                // Generates a unique token to be used instead of the users email and password, with a lifetime of 7 days.
                var refreshToken = await GenerateRefreshToken();
                context.RefreshTokens.Add(new RefreshToken
                {
                    Id = refreshToken,
                    UserId = user.Id,
                    ExpiryDate = DateTime.UtcNow.AddDays(7),
                    IsRevoked = false
                });
                await context.SaveChangesAsync();

                return Ok(new UserLoginResponse(jwtToken, 1800, refreshToken, Message: "User info has been changed."));
            } 
            else return BadRequest("Nothing was changed.");
        }

        [HttpPatch("UpdatePassword"), Authorize]
        public async Task<ActionResult> UpdatePassword(UserEditPasswordRequest userEditPassword)
        {
            var user = await GetSignedInUser();
            if (user == null) return StatusCode(500);
            if (string.IsNullOrEmpty(userEditPassword.Password) || string.IsNullOrEmpty(userEditPassword.ConfirmPassword)) return BadRequest("Password can't be empty.");
            if (userEditPassword.Password != userEditPassword.ConfirmPassword) return BadRequest("Passwords do not match.");
            if (!IsPasswordSecure(userEditPassword.Password)) return BadRequest("Password not secure.");
            if (BCrypt.Net.BCrypt.Verify(userEditPassword.Password, user.Password)) return BadRequest("Password is already used.");
            
            user.Password = BCrypt.Net.BCrypt.HashPassword(userEditPassword.Password);
            await context.SaveChangesAsync();

            return Ok("User password has been changed."); 
        }

        [HttpPost("Delete"), Authorize]
        public async Task<ActionResult> Delete(UserDeleteRequest userDelete)
        {
            var user = await GetSignedInUser();
            if (user == null) return StatusCode(500);
            if (string.IsNullOrEmpty(userDelete.Password)) return BadRequest("Password can't be empty.");
            if (!BCrypt.Net.BCrypt.Verify(userDelete.Password, user.Password)) return BadRequest("Wrong password.");

            context.Remove(user);
            context.SaveChanges();

            return Ok("User was successfully deleted.");
        }

        private async Task<User?> GetSignedInUser()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null) return null;
            var user = await context.Users.FirstOrDefaultAsync(u => u.Id == userId);
            if (user == null) return null;

            return user;
        }

        /// <summary>
        /// Checks if the password is secure, using <see cref="Regex"/>.
        /// </summary>
        /// <param name="password"></param>
        /// <returns><see cref="bool"/> of true, if the given password is secure.</returns>
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
            var regex = new Regex(@"^(?=.*[A-Z])(?=.*[a-z])(?=.*\d)(?=.*[\W_])[^\s]{8,}$");
            return regex.IsMatch(password);
        }

        /// <summary>
        /// Checks if the email is valid, using <see cref="Regex"/>.
        /// </summary>
        /// <param name="email"></param>
        /// <returns><see cref="bool"/> of true, if the given email is valid.</returns>
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
            var regex = new Regex(@"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$");
            return regex.IsMatch(email);
        }
         
        /// <summary>
        /// Generates a GUID for the userID, checks if GUID exists in the database.
        /// </summary>
        /// <returns>A unique id not already used</returns>
        private async Task<string> GenerateUserID()
        {
            while (true)
            {
                var userId = Guid.NewGuid().ToString();
                if (await context.Users.FindAsync(userId) == null) return userId;
            }
        }

        /// <summary>
        /// Generates a JWT token for a user, and returns the token as a string.
        /// </summary>
        /// <param name="user"></param>
        /// <returns>A new JWT Token for the User.</returns>
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
                new Claim("LastName", user.LastName),
                new Claim(ClaimTypes.Role, user.Role.ToString())
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

        /// <summary>
        /// Generates a new random 32 byte number to be used as the Id for the refresh token.
        /// </summary>
        /// <returns>The created Refresh Token.</returns>
        private async Task<string> GenerateRefreshToken()
        {
            while (true)
            {
                var randomNumber = new byte[32];
                using (var rng = RandomNumberGenerator.Create())
                {
                    rng.GetBytes(randomNumber);
                    var token = Convert.ToBase64String(randomNumber);
                    if (await context.RefreshTokens.FindAsync(token) == null) return token;
                }
            }
        }
    }
}
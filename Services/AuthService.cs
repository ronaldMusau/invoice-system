using InvoiceSystem.Data;
using InvoiceSystem.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace InvoiceSystem.Services
{
    public class AuthService
    {
        private readonly ApplicationDbContext _context;
        private readonly IConfiguration _configuration;
        private readonly ILogger<AuthService> _logger;

        public AuthService(
            ApplicationDbContext context,
            IConfiguration configuration,
            ILogger<AuthService> logger)
        {
            _context = context;
            _configuration = configuration;
            _logger = logger;
        }

        public async Task<User> Register(string username, string email, string password, string role = "User")
        {
            try
            {
                // Validate role
                if (role != "User" && role != "Admin")
                {
                    throw new Exception("Invalid role. Must be either 'User' or 'Admin'");
                }

                // Trim inputs
                username = username?.Trim() ?? string.Empty;
                email = email?.Trim() ?? string.Empty;

                // Validate inputs
                if (string.IsNullOrWhiteSpace(username))
                {
                    throw new Exception("Username is required");
                }

                if (string.IsNullOrWhiteSpace(email))
                {
                    throw new Exception("Email is required");
                }

                if (string.IsNullOrWhiteSpace(password))
                {
                    throw new Exception("Password is required");
                }

                _logger.LogInformation($"Checking if username '{username}' already exists...");

                // Check if user exists (case-insensitive)
                if (await _context.Users.AnyAsync(u => u.Username.ToLower() == username.ToLower()))
                {
                    _logger.LogWarning($"Username '{username}' already exists");
                    throw new Exception("Username already exists");
                }

                _logger.LogInformation($"Checking if email '{email}' already exists...");

                if (await _context.Users.AnyAsync(u => u.Email.ToLower() == email.ToLower()))
                {
                    _logger.LogWarning($"Email '{email}' already exists");
                    throw new Exception("Email already exists");
                }

                // Create password hash
                _logger.LogInformation($"Creating password hash for user '{username}'");
                var passwordHash = BCrypt.Net.BCrypt.HashPassword(password);

                var user = new User
                {
                    Username = username,
                    Email = email,
                    PasswordHash = passwordHash,
                    Role = role,
                    CreatedAt = DateTime.UtcNow,
                    RefreshToken = null,
                    RefreshTokenExpiry = null
                };

                _context.Users.Add(user);

                try
                {
                    _logger.LogInformation($"Saving user '{username}' to database...");
                    await _context.SaveChangesAsync();
                    _logger.LogInformation($"User '{username}' registered successfully with ID {user.Id} and role '{role}'");
                }
                catch (DbUpdateException ex)
                {
                    _logger.LogError(ex, $"Database error while registering user '{username}'");

                    // Log inner exception details
                    var innerException = ex.InnerException;
                    while (innerException != null)
                    {
                        _logger.LogError($"Inner exception: {innerException.Message}");
                        innerException = innerException.InnerException;
                    }

                    throw new Exception($"Failed to register user. Database error: {ex.InnerException?.Message ?? ex.Message}");
                }

                return user;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error during registration for username: {username}");
                throw;
            }
        }

        public async Task<(string accessToken, string refreshToken, User user)> Login(string username, string password, string userType)
        {
            _logger.LogInformation($"Login attempt for username: '{username}', userType: '{userType}'");

            // Find user by username (case-insensitive)
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Username.ToLower() == username.ToLower());

            if (user == null)
            {
                _logger.LogWarning($"User not found: '{username}'");
                throw new Exception("Invalid username or password");
            }

            _logger.LogInformation($"User found: ID={user.Id}, Username='{user.Username}', Role='{user.Role}'");

            // Verify password
            _logger.LogInformation("Verifying password...");
            if (!BCrypt.Net.BCrypt.Verify(password, user.PasswordHash))
            {
                _logger.LogWarning($"Invalid password for user '{username}'");
                throw new Exception("Invalid username or password");
            }

            _logger.LogInformation("Password verified successfully");

            // Verify user type matches role
            if (user.Role != userType)
            {
                _logger.LogWarning($"Role mismatch for user '{username}'. Expected: '{userType}', Actual: '{user.Role}'");
                throw new Exception($"User is not registered as {userType}. This account is registered as {user.Role}");
            }

            _logger.LogInformation($"Role verification passed. User role: '{user.Role}' matches requested type: '{userType}'");

            // Generate tokens
            _logger.LogInformation("Generating access token...");
            var accessToken = GenerateAccessToken(user);

            _logger.LogInformation("Generating refresh token...");
            var refreshToken = GenerateRefreshToken();

            // Store refresh token
            user.RefreshToken = refreshToken;
            user.RefreshTokenExpiry = DateTime.UtcNow.AddDays(7);
            await _context.SaveChangesAsync();

            _logger.LogInformation($"Login successful for user '{username}' with role '{user.Role}'");

            return (accessToken, refreshToken, user);
        }

        public async Task<(string accessToken, string refreshToken)> RefreshToken(string refreshToken)
        {
            _logger.LogInformation("Attempting to refresh token");

            var user = await _context.Users.FirstOrDefaultAsync(u => u.RefreshToken == refreshToken);

            if (user == null)
            {
                _logger.LogWarning("Refresh token not found in database");
                throw new Exception("Invalid refresh token");
            }

            if (user.RefreshTokenExpiry <= DateTime.UtcNow)
            {
                _logger.LogWarning($"Refresh token expired for user '{user.Username}'");
                throw new Exception("Refresh token has expired");
            }

            // Generate new tokens
            var newAccessToken = GenerateAccessToken(user);
            var newRefreshToken = GenerateRefreshToken();

            // Update refresh token
            user.RefreshToken = newRefreshToken;
            user.RefreshTokenExpiry = DateTime.UtcNow.AddDays(7);
            await _context.SaveChangesAsync();

            _logger.LogInformation($"Token refreshed successfully for user '{user.Username}'");

            return (newAccessToken, newRefreshToken);
        }

        public async Task<bool> RevokeToken(string refreshToken)
        {
            _logger.LogInformation("Attempting to revoke token");

            var user = await _context.Users.FirstOrDefaultAsync(u => u.RefreshToken == refreshToken);

            if (user == null)
            {
                _logger.LogWarning("Refresh token not found for revocation");
                return false;
            }

            user.RefreshToken = null;
            user.RefreshTokenExpiry = null;
            await _context.SaveChangesAsync();

            _logger.LogInformation($"Token revoked successfully for user '{user.Username}'");

            return true;
        }

        private string GenerateAccessToken(User user)
        {
            _logger.LogInformation($"Generating JWT access token for user '{user.Username}'");

            var jwtSettings = _configuration.GetSection("Jwt");
            var keyString = jwtSettings["Key"] ?? throw new Exception("JWT Key not configured");
            var key = Encoding.UTF8.GetBytes(keyString);

            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.Username),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Role, user.Role)
            };

            _logger.LogInformation($"JWT Claims - ID: {user.Id}, Username: {user.Username}, Email: {user.Email}, Role: {user.Role}");

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddHours(1), // Access token expires in 1 hour
                Issuer = jwtSettings["Issuer"],
                Audience = jwtSettings["Audience"],
                SigningCredentials = new SigningCredentials(
                    new SymmetricSecurityKey(key),
                    SecurityAlgorithms.HmacSha256Signature
                )
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            var token = tokenHandler.CreateToken(tokenDescriptor);
            var tokenString = tokenHandler.WriteToken(token);

            _logger.LogInformation($"Access token generated successfully. Expires: {tokenDescriptor.Expires}");

            return tokenString;
        }

        private string GenerateRefreshToken()
        {
            var randomNumber = new byte[64];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(randomNumber);
            return Convert.ToBase64String(randomNumber);
        }
    }
}
using InvoiceSystem.DTOs;
using InvoiceSystem.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace InvoiceSystem.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly AuthService _authService;
        private readonly ILogger<AuthController> _logger;

        public AuthController(AuthService authService, ILogger<AuthController> logger)
        {
            _authService = authService;
            _logger = logger;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register(UserRegisterDto request)
        {
            _logger.LogInformation($"Registration attempt - Username: {request.Username}, Email: {request.Email}, UserType: {request.UserType}");

            try
            {
                // Model validation will handle the RegularExpression check
                if (!ModelState.IsValid)
                {
                    var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                    _logger.LogWarning($"Registration validation failed for {request.Username}: {string.Join(", ", errors)}");
                    return BadRequest(new { message = "Invalid input data", errors = errors });
                }

                // Additional validation
                if (request.UserType != "User" && request.UserType != "Admin")
                {
                    _logger.LogWarning($"Invalid user type for registration: {request.UserType}");
                    return BadRequest(new { message = "Invalid user type. Must be either 'User' or 'Admin'" });
                }

                var user = await _authService.Register(
                    request.Username,
                    request.Email,
                    request.Password,
                    request.UserType
                );

                _logger.LogInformation($"User {user.Username} registered successfully with role {user.Role}");

                var (accessToken, refreshToken, loginUser) = await _authService.Login(
                    request.Username,
                    request.Password,
                    request.UserType
                );

                _logger.LogInformation($"Auto-login successful for newly registered user {user.Username}");

                var response = new AuthResponseDto
                {
                    AccessToken = accessToken,
                    RefreshToken = refreshToken,
                    Username = user.Username,
                    Role = user.Role,
                    AccessTokenExpiry = DateTime.UtcNow.AddHours(1),
                    RefreshTokenExpiry = DateTime.UtcNow.AddDays(7)
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Registration failed for username: {request.Username}");
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(UserLoginDto request)
        {
            _logger.LogInformation($"Login attempt - Username: {request.Username}, UserType: {request.UserType}");

            try
            {
                // Model validation will handle the RegularExpression check
                if (!ModelState.IsValid)
                {
                    var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                    _logger.LogWarning($"Login validation failed for {request.Username}: {string.Join(", ", errors)}");
                    return BadRequest(new { message = "Invalid input data", errors = errors });
                }

                // Additional validation
                if (request.UserType != "User" && request.UserType != "Admin")
                {
                    _logger.LogWarning($"Invalid user type for login: {request.UserType}");
                    return BadRequest(new { message = "Invalid user type. Must be either 'User' or 'Admin'" });
                }

                var (accessToken, refreshToken, user) = await _authService.Login(
                    request.Username,
                    request.Password,
                    request.UserType
                );

                _logger.LogInformation($"Login successful for user {user.Username} with role {user.Role}");

                var response = new AuthResponseDto
                {
                    AccessToken = accessToken,
                    RefreshToken = refreshToken,
                    Username = user.Username,
                    Role = user.Role,
                    AccessTokenExpiry = DateTime.UtcNow.AddHours(1),
                    RefreshTokenExpiry = DateTime.UtcNow.AddDays(7)
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Login failed for username: {request.Username}, UserType: {request.UserType}");
                return Unauthorized(new { message = ex.Message });
            }
        }

        [HttpPost("refresh-token")]
        public async Task<IActionResult> RefreshToken(RefreshTokenDto request)
        {
            _logger.LogInformation("Refresh token request received");

            try
            {
                var (newAccessToken, newRefreshToken) = await _authService.RefreshToken(request.RefreshToken);

                _logger.LogInformation("Token refresh successful");

                var response = new
                {
                    AccessToken = newAccessToken,
                    RefreshToken = newRefreshToken,
                    AccessTokenExpiry = DateTime.UtcNow.AddHours(1),
                    RefreshTokenExpiry = DateTime.UtcNow.AddDays(7)
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Token refresh failed");
                return Unauthorized(new { message = ex.Message });
            }
        }

        [HttpPost("revoke-token")]
        [Authorize]
        public async Task<IActionResult> RevokeToken(RefreshTokenDto request)
        {
            _logger.LogInformation("Token revocation request received");

            try
            {
                var result = await _authService.RevokeToken(request.RefreshToken);

                if (result)
                {
                    _logger.LogInformation("Token revoked successfully");
                    return Ok(new { message = "Token revoked successfully" });
                }

                _logger.LogWarning("Failed to revoke token - token not found");
                return BadRequest(new { message = "Failed to revoke token" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Token revocation error");
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPost("logout")]
        public async Task<IActionResult> Logout(RefreshTokenDto request)
        {
            _logger.LogInformation("Logout request received");

            try
            {
                await _authService.RevokeToken(request.RefreshToken);
                _logger.LogInformation("Logout successful");
                return Ok(new { message = "Logged out successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Logout error");
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}
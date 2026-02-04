using InvoiceSystem.DTOs;
using InvoiceSystem.Services;
using Microsoft.AspNetCore.Mvc;

namespace InvoiceSystem.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly AuthService _authService;

        public AuthController(AuthService authService)
        {
            _authService = authService;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register(UserRegisterDto request)
        {
            try
            {
                var user = await _authService.Register(request.Username, request.Email, request.Password);
                var token = await _authService.Login(request.Username, request.Password);

                return Ok(new { Token = token, Username = user.Username, Role = user.Role });
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(UserLoginDto request)
        {
            try
            {
                var token = await _authService.Login(request.Username, request.Password);
                return Ok(new { Token = token });
            }
            catch (Exception ex)
            {
                return Unauthorized(ex.Message);
            }
        }

        [HttpPost("register-admin")]
        public async Task<IActionResult> RegisterAdmin(UserRegisterDto request)
        {
            try
            {
                var user = await _authService.Register(request.Username, request.Email, request.Password, "Admin");
                var token = await _authService.Login(request.Username, request.Password);

                return Ok(new { Token = token, Username = user.Username, Role = user.Role });
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
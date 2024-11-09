using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebProject_API_React.Server.Models;
using WebProject_API_React.Server.Repository.IRepository;
using WebProject_API_React.Server.Services;

namespace WebProject_API_React.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {

        private readonly IUserRepository _userRepository;
        private readonly TokenProvider _tokenProvider;
        private readonly IConfiguration _configuration;


        public AuthController(IUserRepository userRepository, TokenProvider tokenProvider, IConfiguration configuration)
        {

            _userRepository = userRepository;
            _tokenProvider = tokenProvider;
            _configuration = configuration;
        }


        [HttpGet("users"), Authorize]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<UserRequestDto>>> GetUsers()
        {
            var users = await _userRepository.GetAllAsync();
            return Ok(users);
        }

        [HttpPost("register")]
        public async Task<ActionResult> Register(UserRegisterDto request)
        {
            try
            {
                var existingUser = await _userRepository.FirstOrDefaultAsync(u => u.Email == request.Email);
                if (existingUser != null)
                {
                    return Conflict(new { Message = "Email is already taken." });
                }

                var passwordHash = BCrypt.Net.BCrypt.HashPassword(request.Password);

                var user = new User
                {
                    Username = request.Username,
                    Email = request.Email,
                    Password = passwordHash,
                };

                await _userRepository.AddAsync(user);

                var accessToken = _tokenProvider.CreateAccessToken(user);
                var refreshToken = _tokenProvider.CreateRefreshToken();
                user.RefreshToken = refreshToken;
                user.RefreshTokenExpiryTime = DateTime.Now.AddMinutes(_configuration.GetValue<int>("Jwt:ExpirationInDays"));

                await _userRepository.UpdateAsync(user);

                SetRefreshTokenCookie(refreshToken);

                return Ok(new TokenResponseDto
                {
                    AccessToken = accessToken,
                    
                });
            }
            catch (Exception ex)
            {

                return StatusCode(500, new { Message = "An error occurred while processing your request." });
            }
        }

        [HttpPost("login")]
        public async Task<ActionResult> Login(UserLoginDto request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                if (string.IsNullOrEmpty(request.Email) || string.IsNullOrEmpty(request.Password))
                    return BadRequest(new { message = "Username and password are required." });

                var user = await _userRepository.FirstOrDefaultAsync(u => u.Email == request.Email);
                if (user == null || !BCrypt.Net.BCrypt.Verify(request.Password, user.Password))
                    return Unauthorized();

                var accessToken = _tokenProvider.CreateAccessToken(user);
                var refreshToken = _tokenProvider.CreateRefreshToken();
                user.RefreshToken = refreshToken;
                user.RefreshTokenExpiryTime = DateTime.Now.AddMinutes(_configuration.GetValue<int>("Jwt:ExpirationInDays"));

                await _userRepository.UpdateAsync(user);

                SetRefreshTokenCookie(refreshToken);

                return Ok(new TokenResponseDto
                {
                    AccessToken = accessToken,
                    
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "An error occurred while processing your request." });
            }
        }


        [HttpPost("logout")]
        public async Task<IActionResult> Logout()
        {

            var refreshToken = Request.Cookies["refreshToken"];
            if (string.IsNullOrEmpty(refreshToken))
                return NoContent();

            if (refreshToken != null)
            {
                var user = await _userRepository.FirstOrDefaultAsync(u => u.RefreshToken == refreshToken);
                if (user != null)
                {
                    user.RefreshToken = "";
                    user.RefreshTokenExpiryTime = DateTime.MinValue;
                    await _userRepository.UpdateAsync(user);
                }
            }

            Response.Cookies.Append("refreshToken", "", new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Strict,
                Expires = DateTime.Now.AddDays(-1)
            });

            return Ok(new { message = "Logged out successfully" });
        }


        [HttpGet("refresh")]
        public async Task<IActionResult> RefreshToken()
        {

            try
            {
                var refreshToken = Request.Cookies["refreshToken"];
                if (refreshToken == null)
                    return Unauthorized();

                var user = await _userRepository.FirstOrDefaultAsync(u => u.RefreshToken == refreshToken);
                if (user == null)
                    return Forbid();

                var newAccessToken = _tokenProvider.CreateAccessToken(user);
                var newRefreshToken = _tokenProvider.CreateRefreshToken();
                user.RefreshToken = newRefreshToken;
                user.RefreshTokenExpiryTime = DateTime.Now.AddMinutes(_configuration.GetValue<int>("Jwt:ExpirationInDays"));

                await _userRepository.UpdateAsync(user);

                SetRefreshTokenCookie(newRefreshToken);

                return Ok(new TokenResponseDto { 
                    AccessToken = newAccessToken,
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "An error occurred while processing your request." });
            }
        }

        private void SetRefreshTokenCookie(string refreshToken)
        {
            var cookieOptions = new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Strict,
                Expires = DateTime.Now.AddMinutes(_configuration.GetValue<int>("Jwt:ExpirationInDays"))
            };
            Response.Cookies.Append("refreshToken", refreshToken, cookieOptions);
        }
    }
}


﻿using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebProject.Server.Models;
using WebProject.Server.Repository.IRepository;
using WebProject.Server.Services;

namespace WebProject.Server.Controllers
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
                // Перевірка, чи існує користувач з таким email
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
                    Expiration = DateTime.Now.AddMinutes(_configuration.GetValue<int>("Jwt:ExpirationInMinutes"))
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
                    Expiration = DateTime.Now.AddMinutes(_configuration.GetValue<int>("Jwt:ExpirationInMinutes"))
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
                // Знаходимо користувача за токеном і видаляємо його
                var user = await _userRepository.FirstOrDefaultAsync(u => u.RefreshToken == refreshToken);
                if (user != null)
                {
                    user.RefreshToken = ""; // Очищаємо токен
                    user.RefreshTokenExpiryTime = DateTime.MinValue;
                    await _userRepository.UpdateAsync(user);
                }
            }

            // Очищуємо кукі з refreshToken
            Response.Cookies.Append("refreshToken", "", new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Strict,
                Expires = DateTime.Now.AddDays(-1) // Видаляє кукі
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

                return Ok(new TokenResponseDto { AccessToken = newAccessToken, Expiration = DateTime.Now.AddMinutes(_configuration.GetValue<int>("Jwt:ExpirationInMinutes")) });
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


using EduMatch.BusinessLogicLayer.DTOs;
using EduMatch.BusinessLogicLayer.Interfaces;
using EduMatch.BusinessLogicLayer.Settings;
using EduMatch.DataAccessLayer.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Sprache;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace EduMatch.PresentationLayer.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly JwtSettings _jwt;

        public UserController(IUserService userService, IOptions<JwtSettings> options)
        {
            _userService = userService;
            _jwt = options.Value;
        }

        [HttpPost("register")]
        public async Task<IActionResult> RegisterAsync([FromBody] RegisterRequestDto req)
        {
            if (string.IsNullOrWhiteSpace(req.Email) || string.IsNullOrWhiteSpace(req.Password) || string.IsNullOrWhiteSpace(req.FullName))
                return BadRequest("All fields required");

            var baseUrl = $"{Request.Scheme}://{Request.Host}";
            var success = await _userService.RegisterAsync(req.FullName ,req.Email, req.Password, baseUrl);
            if (!success) return BadRequest("Email already exists");

            return Ok(new { message = "Registration successful, check your email to verify." });
        }

        [HttpGet("verify-email")]
        public async Task<IActionResult> VerifyEmailAsync(string token)
        {
            var success = await _userService.VerifyEmailAsync(token);
            if (!success) return BadRequest("Invalid or expired token!");

            return Redirect("http://localhost:3000/login");
        }

        [HttpPost("resend-verify")]
        public async Task<IActionResult> ResendVerificationAsync([FromBody] string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                return BadRequest(new { message = "Email is required" });

            var success = await _userService.ResendEmailVerifyAsync(email, $"{Request.Scheme}://{Request.Host}");
            if (!success)
                return BadRequest(new { message = "Email not found or already verified" });

            return Ok(new { message = "Verification email sent successfully" });
        }


        [HttpPost("login")]
        public async Task<IActionResult> LoginAsync([FromBody] LoginRequestDto req)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(req.Email) || string.IsNullOrWhiteSpace(req.Password))
                    return BadRequest("Email and password required");

                var auth = await _userService.LoginAsync(req.Email, req.Password);
                if (auth == null) return Unauthorized("Invalid credentials");

                // Refresh token set vào cookie
                Response.Cookies.Append("refresh_token", auth.RefreshToken, new CookieOptions
                {
                    HttpOnly = true,
                    Secure = true,
                    SameSite = SameSiteMode.None,
                    Expires = DateTime.UtcNow.AddDays(_jwt.RefreshTokenDays)
                });

                return Ok(new
                {
                    accessToken = auth.AccessToken,
                    accessTokenExpiresAt = auth.AccessTokenExpiresAt,
                    tokenType = "Bearer",
                    message = "Login successful!"
                });
            }
            catch (Exception ex)
            {
                // Nếu email chưa xác thực
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPost("refresh-token")]
        public async Task<IActionResult> RefreshTokenAsync()
        {
            var oldRefreshToken = Request.Cookies["refresh_token"];
            if (string.IsNullOrEmpty(oldRefreshToken))
                return Unauthorized(new { message = "No refresh token found" });

            var auth = await _userService.RefreshTokenAsync(oldRefreshToken);
            if (auth == null) return Unauthorized("Invalid or expired refresh token");

            //Gắn refresh token mới vào cookie (rotation)
            Response.Cookies.Append("refresh_token", auth.RefreshToken, new CookieOptions
            {
                HttpOnly = true,
                Secure = true, // bật khi dùng HTTPS
                SameSite = SameSiteMode.None,
                Expires = DateTime.UtcNow.AddDays(_jwt.RefreshTokenDays)
            });

            return Ok(new
            {
                accessToken = auth.AccessToken,
                expiresAt = auth.AccessTokenExpiresAt
            });
        }

        [HttpPost("logout")]
        public async Task<IActionResult> RevokeTokenAsync()
        {
            // Lấy refresh token từ cookie
            var refreshToken = Request.Cookies["refresh_token"];

            if (string.IsNullOrEmpty(refreshToken))
                return BadRequest(new { message = "No refresh token found" });

            var result = await _userService.RevokeTokenAsync(refreshToken);

            if (!result)
                return BadRequest(new { message = "Invalid or already revoked token" });

            // Xóa cookie
            Response.Cookies.Delete("refresh_token", new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.None
            });

            return Ok(new { revoked = true, message = "Logout successful" });
        }

        [HttpPost("signIn-google")]
        public async Task<IActionResult> GoogleLoginAsync([FromBody] GoogleLoginRequest request)
        {
            if (string.IsNullOrEmpty(request.IdToken))
                return BadRequest("Missing Google ID token.");

            try
            {
                var payload = await _userService.LoginWithGoogleAsync(request);
                var response = payload as LoginResponseDto;

                if (response == null)
                    return StatusCode(500, new { Message = "Invalid login response." });

                // Refresh token set vào cookie
                Response.Cookies.Append("refresh_token", response.RefreshToken, new CookieOptions
                {
                    HttpOnly = true,
                    Secure = true,
                    SameSite = SameSiteMode.None,
                    Expires = DateTime.UtcNow.AddDays(_jwt.RefreshTokenDays)
                });

                return Ok(new
                {
                    accessToken = response.AccessToken,
                    accessTokenExpiresAt = response.AccessTokenExpiresAt,
                });

            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { Message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = ex.Message });
            }

        }

        [Authorize]
        [HttpGet("me")]
        public async Task<IActionResult> GetCurrentUser()
        {
            var email = User.FindFirst(ClaimTypes.Email)?.Value;

            if (string.IsNullOrEmpty(email))
                return Unauthorized(new { Message = "Invalid token: missing email." });

            var name = User.FindFirst(ClaimTypes.Name)?.Value;
            var roleName = User.FindFirst(ClaimTypes.Role)?.Value;
            var loginProvider = User.FindFirst("provider")?.Value;
            var createdAt = User.FindFirst("createdAt")?.Value;
            var avatarUrl = User.FindFirst("avatarUrl")?.Value;

            return Ok(new
            {
                Email = email,
                Name = name,
                RoleName = roleName,
                LoginProvider = loginProvider,
                CreatedAt = createdAt,
                AvatarUrl = avatarUrl
            });
        }
    }
}

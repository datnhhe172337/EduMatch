using EduMatch.BusinessLogicLayer.DTOs;
using EduMatch.BusinessLogicLayer.Interfaces;
using EduMatch.BusinessLogicLayer.Settings;
using EduMatch.DataAccessLayer.Entities;
using EduMatch.DataAccessLayer.Interfaces;
using EduMatch.DataAccessLayer.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace EduMatch.BusinessLogicLayer.Services
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepo;
        private readonly JwtSettings _jwt;
        private readonly EmailService _emailService;
        private readonly IRefreshTokenRepositoy _refreshRepo;
        private readonly IGoogleAuthService _googleAuthService;


        public UserService(IUserRepository userRepo, IOptions<JwtSettings> options, EmailService emailService, IRefreshTokenRepositoy refreshRepo, IGoogleAuthService googleAuthService)
        {
            _userRepo = userRepo;
            _jwt = options.Value;
            _emailService = emailService;
            _refreshRepo = refreshRepo;
            _googleAuthService = googleAuthService;
        }

        public async Task<bool> RegisterAsync(string email, string password, string baseUrl)
        {
            if(await _userRepo.IsEmailAvailableAsync(email))
                return false;

            var user = new User
            {
                Email = email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(password),
                UserName = email,
                LoginProvider = "Local",
                RoleId = 1,
                IsEmailConfirmed = false,
            };

            await _userRepo.CreateUserAsync(user);

            var verificationToken = GenerateEmailVerificationToken(user);

            var verifyUrl = $"{baseUrl}/api/User/verify-email?token={verificationToken}";

            var body = $@"
                <h3>Verify your email</h3>
                <p>Click <a href='{verifyUrl}'>here</a> to confirm your email.</p>
                <p>This link will expire in 10 minutes.</p>";

            await _emailService.SendMailAsync(new MailContent
            {
                To = email,
                Subject = "Send: Verify your email",
                Body = body
            });

            return true;
        }
        public async Task<IEnumerable<ManageUserDto>> GetUserByRoleAsync(int roleId)
        {
            IEnumerable<ManageUserDto> users;
            switch (roleId)
            {
                case 1: //learner
                    users = (await _userRepo.GetLearnerAsync())
                        .Select(u => new ManageUserDto
                        {
                            Email = u.Email,
                            UserName = u.UserName,
                            Phone = u.Phone,
                            IsActive = u.IsActive,
                            CreateAt = u.CreatedAt
                        });
                    break;
                case 2: //tutor
                    users = (await _userRepo.GetTutorAsync())
                        .Select(u => new ManageUserDto
                        {
                            Email = u.Email,
                            UserName = u.UserName,
                            Phone = u.Phone,
                            IsActive = u.IsActive,
                            CreateAt = u.TutorProfile?.CreatedAt ?? u.CreatedAt,
                            Subjects = u.TutorProfile?.TutorSubjects
                                    .Select(ts => ts.Subject.SubjectName)
                                    .ToList() ?? new List<string>()
                        });
                    break;
                case 3: //admin
                    users = (await _userRepo.GetAdminAsync())
                        .Select(u => new ManageUserDto
                        {
                            Email = u.Email,
                            UserName = u.UserName,
                            Phone = u.Phone,
                            IsActive = u.IsActive,
                            CreateAt = u.CreatedAt
                        });
                    break;
                default:
                    return Enumerable.Empty<ManageUserDto>();
            }
            return users;
        }


        public async Task<bool> DeactivateUserAsync(string email)
        {
            return await _userRepo.UpdateUserStatusAsync(email, false);
        }

        public async Task<bool> ActivateUserAsync(string email)
        {
            return await _userRepo.UpdateUserStatusAsync(email, true);
        }

        public async Task<User> CreateAdminAccAsync(string email)
        {
            var existingUser = await _userRepo.GetUserByEmailAsync(email);
            if (existingUser != null) throw new InvalidOperationException("Email đã tồn tại.");

            var admin = new User
            {
                Email = email,
                UserName = email.Split('@')[0],
                PasswordHash = "123",
                RoleId = 3,
                IsEmailConfirmed = true,
                IsActive = true,
                LoginProvider = "System",
                CreatedAt = DateTime.UtcNow
            };

            await _userRepo.CreateAdminAccAsync(admin);
            return admin;
        }

        private string GenerateEmailVerificationToken(User user)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes(_jwt.Secret);


            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Email),
                new Claim("purpose", "email_verification"), // thêm claim phân biệt loại token
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddMinutes(10),
                Issuer = _jwt.Issuer,
                //Tạo chữ ký
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            var tokenString = tokenHandler.WriteToken(token);

            return tokenString;
        }

        public async Task<bool> VerifyEmailAsync(string token)
        {
            JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear();

            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes(_jwt.Secret);

            try
            {
                var claims = tokenHandler.ValidateToken(token, new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = false,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ClockSkew = TimeSpan.Zero,
                    ValidIssuer = _jwt.Issuer,
                    IssuerSigningKey = new SymmetricSecurityKey(key)
                }, out var validatedToken);

                var userEmail = claims.FindFirstValue(JwtRegisteredClaimNames.Sub);

                if (string.IsNullOrEmpty(userEmail))
                    return false;

                var user = await _userRepo.GetUserByEmailAsync(userEmail);
                if (user == null) return false;

                user.IsEmailConfirmed = true;
                user.IsActive = true;
                await _userRepo.UpdateUserAsync(user);

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine("[VerifyEmailAsync] Token invalid: " + ex.ToString());
                return false;
            }
        }

        public async Task<LoginResponseDto> LoginAsync(string email, string password)
        {

            var user = await _userRepo.GetUserByEmailAsync(email);
            if (user == null)
                throw new UnauthorizedAccessException("Invalid email");

            if (user.LoginProvider != "Local")
                throw new UnauthorizedAccessException("Email is logged in with google");

            var valid = BCrypt.Net.BCrypt.Verify(password, user.PasswordHash);
            if (!valid) return null;

            if (user.IsEmailConfirmed == false)
                throw new UnauthorizedAccessException("Email not verified. Please verify before login!");

            if (user.IsActive == false)
                throw new UnauthorizedAccessException("Account is deactivated.");

            var accessToken = GenerateJwtToken(user);
            var refreshToken = GenerateRefreshToken();
            var refreshTokenHash = ComputeSha256Hash(refreshToken);

            var existingRefreshToken = await _refreshRepo.ExistingRefreshTokenAsync(user);

            if (existingRefreshToken != null)
            {
                // Ghi đè refresh token cũ
                existingRefreshToken.TokenHash = refreshTokenHash;
                existingRefreshToken.ExpiresAt = DateTime.UtcNow.AddDays(_jwt.RefreshTokenDays);
                await _refreshRepo.UpdateRefreshTokenAsync(existingRefreshToken);
            }
            else
            {
                var refreshEntity = new RefreshToken
                {
                    UserEmail = user.Email,
                    TokenHash = refreshTokenHash,
                    ExpiresAt = DateTime.UtcNow.AddDays(_jwt.RefreshTokenDays)
                };
                await _refreshRepo.CreateRefreshTokenAsync(refreshEntity);
            }

            return new LoginResponseDto
            {
                AccessToken = accessToken.Token,
                AccessTokenExpiresAt = accessToken.ExpiresAt,
                RefreshToken = refreshToken
            };
        }

        /* Trả ra cả token lẫn expiresAt của token đó -> nó trả ra (token, expires) */
        private (string Token, DateTime ExpiresAt) GenerateJwtToken(User user)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes(_jwt.Secret);

            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Email),
                new Claim(ClaimTypes.Name, user.UserName ?? user.Email),
                new Claim(ClaimTypes.Role, user.RoleId.ToString()),
                new Claim("provider", user.LoginProvider),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };


            var expires = DateTime.UtcNow.AddMinutes(_jwt.AccessTokenMinutes);


            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = expires,
                Issuer = _jwt.Issuer,
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            var tokenString = tokenHandler.WriteToken(token);
            return (tokenString, expires);
        }

        private static string GenerateRefreshToken()
        {
            // 64 bytes -> base64 length ~88 chars.
            var randomBytes = new byte[64];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(randomBytes);
            return Convert.ToBase64String(randomBytes);
        }

        /* Hash refresh token với hàm băm SHA256 */
        private static string ComputeSha256Hash(string raw)
        {
            using var sha = SHA256.Create();
            var bytes = Encoding.UTF8.GetBytes(raw);
            var hash = sha.ComputeHash(bytes);
            return Convert.ToBase64String(hash);
        }

        public async Task<bool> ResendEmailVerifyAsync(string email, string baseUrl)
        {
            var user = await _userRepo.GetUserByEmailAsync(email);
            if (user == null) return false;

            if (user.IsEmailConfirmed == true) return false;

            var verificationToken = GenerateEmailVerificationToken(user);

            var verifyUrl = $"{baseUrl}/api/User/verify-email?token={verificationToken}";

            var body = $@"
                <h3>Verify your email</h3>
                <p>Click <a href='{verifyUrl}'>here</a> to confirm your email.</p>
                <p>This link will expire in 10 minutes.</p>";

            // ✅ Gửi email
            await _emailService.SendMailAsync(new MailContent
            {
                To = email,
                Subject = "Resend: Verify your email",
                Body = body
            });

            return true;
        }

        public async Task<LoginResponseDto> RefreshTokenAsync(string token)
        {
            var tokenHash = ComputeSha256Hash(token);

            var existing = await _refreshRepo.GetValidRefreshTokenAsync(tokenHash);

            if (existing == null) return null; // invalid token

            // Rotate: revoke existing and create new
            var newRefreshToken = GenerateRefreshToken();
            var newHash = ComputeSha256Hash(newRefreshToken);


            existing.TokenHash = newHash;
            existing.ExpiresAt = DateTime.UtcNow.AddDays(_jwt.RefreshTokenDays);
            existing.CreatedAt = DateTime.UtcNow;

            await _refreshRepo.UpdateRefreshTokenAsync(existing);

            var access = GenerateJwtToken(existing.UserEmailNavigation);

            return new LoginResponseDto
            {
                AccessToken = access.Token,
                AccessTokenExpiresAt = access.ExpiresAt,
                RefreshToken = newRefreshToken
            };
        }

        public async Task<bool> RevokeTokenAsync(string token)
        {
            var tokenHash = ComputeSha256Hash(token);

            var existing = await _refreshRepo.GetByTokenHashAsync(tokenHash);
            if (existing == null) return false;

            if (existing.RevokedAt != null) return false; // đã revoke rồi

            // Nếu token đã hết hạn thì vẫn cho revoke
            existing.RevokedAt = DateTime.UtcNow;
            await _refreshRepo.UpdateRefreshTokenAsync(existing);

            return true;
        }

        public async Task<object?> LoginWithGoogleAsync(GoogleLoginRequest request)
        {
            var payload = await _googleAuthService.VerifyGoogleTokenAsync(request.IdToken);
            if (payload == null || string.IsNullOrEmpty(payload.Email))
                throw new UnauthorizedAccessException("Invalid Google token or missing email.");

            var user = await _userRepo.GetUserByEmailAsync(payload.Email);
            if (user == null)
            {
                user = new User
                {
                    Email = payload.Email,
                    UserName = payload.Name,
                    IsEmailConfirmed = payload.EmailVerified,
                    LoginProvider = "Google",
                    RoleId = 1,
                    IsActive = true
                };

                user.UserProfile = new UserProfile
                {
                    AvatarUrl = payload.Picture
                };

                await _userRepo.CreateUserAsync(user);
            } 
 
            if (user.IsActive == false)
                 throw new UnauthorizedAccessException("Account is deactivated.");
            if (user.LoginProvider != "Google")
                throw new UnauthorizedAccessException("Email registered with local account");

            var accessToken = GenerateJwtToken(user);
            var refreshToken = GenerateRefreshToken();
            var refreshTokenHash = ComputeSha256Hash(refreshToken);

            var existingRefreshToken = await _refreshRepo.ExistingRefreshTokenAsync(user);

            if (existingRefreshToken != null)
            {
                // Ghi đè refresh token cũ
                existingRefreshToken.TokenHash = refreshTokenHash;
                existingRefreshToken.ExpiresAt = DateTime.UtcNow.AddDays(_jwt.RefreshTokenDays);
                await _refreshRepo.UpdateRefreshTokenAsync(existingRefreshToken);
            }
            else
            {
                var refreshEntity = new RefreshToken
                {
                    UserEmail = user.Email,
                    TokenHash = refreshTokenHash,
                    ExpiresAt = DateTime.UtcNow.AddDays(_jwt.RefreshTokenDays)
                };
                await _refreshRepo.CreateRefreshTokenAsync(refreshEntity);
            }

            return new LoginResponseDto
            {
                AccessToken = accessToken.Token,
                AccessTokenExpiresAt = accessToken.ExpiresAt,
                RefreshToken = refreshToken
            };
        }
    }
}

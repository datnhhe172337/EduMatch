using AutoMapper;
using EduMatch.BusinessLogicLayer.DTOs;
using EduMatch.BusinessLogicLayer.Interfaces;
using EduMatch.BusinessLogicLayer.Requests;
using EduMatch.BusinessLogicLayer.Requests.User;
using EduMatch.BusinessLogicLayer.Settings;
using EduMatch.DataAccessLayer.Entities;
using EduMatch.DataAccessLayer.Interfaces;
using EduMatch.DataAccessLayer.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Org.BouncyCastle.Ocsp;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Runtime.InteropServices;
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
        private readonly IMapper _mapper;
        private readonly IUserProfileRepository _profileRepo;

		public UserService(IUserRepository userRepo, 
        IMapper mapper , 
        IOptions<JwtSettings> options,
         EmailService emailService, 
         IRefreshTokenRepositoy refreshRepo, 
         IGoogleAuthService googleAuthService,
         IUserProfileRepository profileRepo)
        {
            _userRepo = userRepo;
            _jwt = options.Value;
            _emailService = emailService;
            _refreshRepo = refreshRepo;
            _googleAuthService = googleAuthService;
            _mapper = mapper;
             _profileRepo = profileRepo;
		}
     
        


        public async Task<bool> RegisterAsync(string fullName, string email, string password, string baseUrl)
        {
            if(await _userRepo.IsEmailAvailableAsync(email))
                return false;

            var user = new User
            { 
                Email = email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(password),
                UserName = fullName,
                LoginProvider = "Local",
                RoleId = 1,
                IsEmailConfirmed = false,
            };

            await _userRepo.CreateUserAsync(user);

            var profile = new UserProfile
            {
                UserEmail = email
            };

            await _userRepo.CreateUserProfileAsync(profile);

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

        public async Task<bool> IsEmailAvailableAsync(string email)
        {
            if (await _userRepo.IsEmailAvailableAsync(email))
                return false;

            return true;
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
                            RoleName = u.Role.RoleName,
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
                            RoleName = u.Role.RoleName,
                            Phone = u.Phone,
                            IsActive = u.IsActive,
                            CreateAt = u.TutorProfile?.CreatedAt ?? u.CreatedAt
                        });
                    break;
                case 3: //admin
                    users = (await _userRepo.GetAdminAsync())
                        .Select(u => new ManageUserDto
                        {
                            Email = u.Email,
                            UserName = u.UserName,
                            RoleName = u.Role.RoleName,
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

        public async Task<User> CreateAdminAccAsync(string email, string password)
        {
            var existingUser = await _userRepo.GetUserByEmailAsync(email);
            if (existingUser != null) throw new InvalidOperationException("Email đã tồn tại.");

            var admin = new User
            {
                Email = email,
                UserName = email.Split('@')[0],
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(password),
                RoleId = 3,
                IsEmailConfirmed = true,
                IsActive = true,
                LoginProvider = "Local",
                CreatedAt = DateTime.UtcNow
            };

            await _userRepo.CreateAdminAccAsync(admin);

            var subject = "Tài khoản Quản trị viên của bạn đã được tạo thành công";
            var body = $@"
        <div style='font-family: Arial, sans-serif; color: #333;'>
            <h2 style='color:#2c3e50;'>Xin chào {admin.UserName},</h2>
            <p>Hệ thống xin thông báo rằng tài khoản <strong>Quản trị viên</strong> của bạn đã được khởi tạo thành công.</p>
            <p>Dưới đây là thông tin đăng nhập của bạn:</p>
            <table style='border-collapse: collapse;'>
                <tr>
                    <td style='padding: 6px 12px; font-weight: bold;'>Email:</td>
                    <td style='padding: 6px 12px;'>{email}</td>
                </tr>
                <tr>
                    <td style='padding: 6px 12px; font-weight: bold;'>Mật khẩu:</td>
                    <td style='padding: 6px 12px;'>{password}</td>
                </tr>
            </table>
            <p><em>Vui lòng thay đổi mật khẩu ngay sau khi đăng nhập để đảm bảo an toàn tài khoản.</em></p>
            <br/>
            <p>Trân trọng,<br/><strong>Đội ngũ Hỗ trợ Hệ thống</strong></p>
            <hr style='border: none; border-top: 1px solid #ddd;'/>
            <p style='font-size: 12px; color: #777;'>Đây là email tự động, vui lòng không trả lời lại.</p>
        </div>";

            await _emailService.SendMailAsync(new MailContent
            {
                To = email,
                Subject = subject,
                Body = body
            });

            return admin;
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
            if (!valid)
                throw new UnauthorizedAccessException("Invalid password");

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
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Name, user.UserName ?? user.Email),
                new Claim(ClaimTypes.Role, user.Role.RoleName),
                new Claim("provider", user.LoginProvider),
                new Claim("createdAt", user.CreatedAt.ToString()),
                new Claim("avatarUrl", user.UserProfile?.AvatarUrl ?? string.Empty),
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

            var user = await _userRepo.GetUserByEmailAsync(existing.UserEmail);
            if (user == null)
                return null;

            // Rotate: revoke existing and create new
            var newRefreshToken = GenerateRefreshToken();
            var newHash = ComputeSha256Hash(newRefreshToken);


            existing.TokenHash = newHash;
            existing.ExpiresAt = DateTime.UtcNow.AddDays(_jwt.RefreshTokenDays);
            existing.CreatedAt = DateTime.UtcNow;

            await _refreshRepo.UpdateRefreshTokenAsync(existing);

            var access = GenerateJwtToken(user);

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


		public async Task<UserDto?> GetByEmailAsync(string email)
		{
			if (string.IsNullOrWhiteSpace(email))
                throw new ArgumentException("Email is required.");

            var entity = await _userRepo.GetUserByEmailAsync(email);
            return entity is null ? null : _mapper.Map<UserDto>(entity);
		}


        public async Task<IEnumerable<ManageUserDto>> GetAllUsers()
        {
            IEnumerable<ManageUserDto> users;
            users = (await _userRepo.GetAllUsers())
                .Select(u => new ManageUserDto
                {
                    Email = u.Email,
                    UserName = u.UserName,
                    RoleName = u.Role.RoleName,
                    Phone = u.Phone,
                    IsActive = u.IsActive,
                    CreateAt = u.TutorProfile?.CreatedAt ?? u.CreatedAt,
                });
            return users;
        }

        public async Task<bool> UpdateRoleUserAsync(string email, int roleId)
        {
            if(string.IsNullOrWhiteSpace(email))
                throw new ArgumentException("Email is required.");
            if(roleId <= 0)
                throw new ArgumentException("RoleId is invalid.");
             var role = await _userRepo.GetRoleByIdAsync(roleId);
            if(role == null)
                throw new ArgumentException("RoleId does not exist.");

			return await _userRepo.UpdateRoleUserAsync(email, roleId);
        }


		public async Task<UserDto?> UpdateUserNameAndPhoneAsync(string email, string phone, string name)
		{
			if(string.IsNullOrWhiteSpace(email))
				throw new ArgumentException("Email is required.");

            if(string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Name is required.");

            if(string.IsNullOrWhiteSpace(phone))
                throw new ArgumentException("Phone is required.");
            var user = await _userRepo.GetUserByEmailAsync(email);
            if (user == null) return null;
           
            await _userRepo.UpdateNameAndPhoneUserAsync(name, phone, email);

            return _mapper.Map<UserDto>(user);

		}

        public async Task<bool> ChangePasswordAsync(string email, ChangePasswordRequest request)
        {
            var user = await _userRepo.GetUserByEmailAsync(email);
            if (user == null)
                throw new UnauthorizedAccessException("Invalid email");
            if (user.LoginProvider != "Local")
                throw new UnauthorizedAccessException("Email is logged in with google, unable to change password");

            var valid = BCrypt.Net.BCrypt.Verify(request.oldPass, user.PasswordHash);
            if (!valid) return false;

            if (request.oldPass == request.newPass)
                throw new Exception("New password must be different from old password.");

            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.newPass);

            await _userRepo.UpdateUserAsync(user);

            return true;

        }

        public async Task<bool> ResetPasswordAsync(string email)
        {
            var user = await _userRepo.GetUserByEmailAsync(email);
            if (user == null)
                return false;
            if (user.LoginProvider != "Local")
                throw new InvalidOperationException("This account uses Google Login. Unable to reset password.");

            string tempPassword = GenerateTemporaryPassword(8);

            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(tempPassword);

            await _userRepo.UpdateUserAsync(user);

            await _emailService.SendMailAsync(new MailContent
            {
                To = user.Email,
                Subject = "Mật khẩu của bạn đã được đặt lại",
                Body = $"Mật khẩu tạm thời của bạn là: <b>{tempPassword}</b><br/><br/>Vui lòng không chia sẻ email này cho bất kì ai. Hãy dùng mật khẩu tạm thời này để đăng nhập và thay đổi mật khẩu mà bạn muốn."
            });

            return true;
        }

        private static string GenerateTemporaryPassword(int length)
        {
            // tránh ký tự dễ nhầm lẫn như O, 0, I, l.
            string chars =
                 "ABCDEFGHJKLMNPQRSTUVWXYZabcdefghijkmnpqrstuvwxyz23456789!@$?";

            var random = new Random();
            return new string(Enumerable.Repeat(chars, length)
                .Select(s => s[random.Next(s.Length)]).ToArray());
        }
    }

}

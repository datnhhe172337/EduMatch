using EduMatch.BusinessLogicLayer.DTOs;
using EduMatch.DataAccessLayer.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EduMatch.BusinessLogicLayer.Interfaces
{
    public interface IUserService
    {
        Task<bool> RegisterAsync(string email, string password, string baseUrl);
        Task<bool> VerifyEmailAsync(string token);
        Task<LoginResponseDto> LoginAsync(string email, string password);
        Task<bool> ResendEmailVerifyAsync(string email, string baseUrl);
        Task<LoginResponseDto> RefreshTokenAsync(string token);
        Task<bool> RevokeTokenAsync(string token);
        Task<object?> LoginWithGoogleAsync(GoogleLoginRequest request);
    }
}

using EduMatch.BusinessLogicLayer.DTOs;
using EduMatch.BusinessLogicLayer.Interfaces;
using EduMatch.DataAccessLayer.Entities;
using EduMatch.DataAccessLayer.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EduMatch.BusinessLogicLayer.Services
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepo;


        public UserService(IUserRepository userRepo)
        {
            _userRepo = userRepo;
        }

        public async Task<bool> RegisterAsync(string email, string password, string baseUrl)
        {
            if(await _userRepo.IsEmailAvailableAsync(email))
                return false;

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
            if(existingUser != null) throw new InvalidOperationException("Email đã tồn tại.");

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
    }
}

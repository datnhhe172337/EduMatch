
using EduMatch.BusinessLogicLayer.Constants;
using EduMatch.BusinessLogicLayer.DTOs;
using EduMatch.BusinessLogicLayer.Interfaces;
using EduMatch.PresentationLayer.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace EduMatch.PresentationLayer.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = Roles.BusinessAdmin + "," + Roles.SystemAdmin)]
    public class AdminController : ControllerBase
    {
        private readonly IUserService _userService;

        public AdminController(IUserService userService)
        {
            _userService = userService;
        }

        [HttpGet("role/{roleId}")]
        public async Task<IActionResult> GetUserByRole(int roleId)
        {
            var users = await _userService.GetUserByRoleAsync(roleId);
            if (users == null || !users.Any())
                return Ok(ApiResponse<IEnumerable<ManageUserDto>>.Fail("RoleId không hợp lệ hoặc danh sách rỗng."));

            string roleName = users.First().RoleName;
            return Ok(ApiResponse<object>.Ok(users, $"Lấy danh sách {roleName} thành công."));
        }

        [HttpGet("getAllUsers")]
        public async Task<IActionResult> GetAllUser()
        {
            var users = await _userService.GetAllUsers();
            if (users == null || !users.Any())
                return Ok(ApiResponse<IEnumerable<ManageUserDto>>.Fail("Danh sách người dùng rỗng."));
            
            return Ok(ApiResponse<object>.Ok(users, "Lấy danh sách người dùng thành công."));
        }

        [HttpPut("users/{email}/deactivate")]
        public async Task<IActionResult> DeactivateUser(string email)
        {
            var result = await _userService.DeactivateUserAsync(email);
            if (!result) return NotFound(ApiResponse<string>.Fail("Không tìm thấy người dùng."));
            return Ok(ApiResponse<string>.Ok(null, "Vô hiệu hóa tài khoản thành công."));
        }

        [HttpPut("users/{email}/activate")]
        public async Task<IActionResult> ActivateUser(string email)
        {
            var result = await _userService.ActivateUserAsync(email);
            if (!result) return NotFound(ApiResponse<string>.Fail("Không tìm thấy người dùng."));
            return Ok(ApiResponse<string>.Ok(null, "Kích hoạt tài khoản thành công."));
        }

        [HttpPut("users/{email}/{roleId}")]
        public async Task<IActionResult> UpdateRoleUser(string email, int roleId)
        {
            var result = await _userService.UpdateRoleUserAsync(email, roleId);
            if (!result) return NotFound(ApiResponse<string>.Fail("Không tìm thấy người dùng"));
            return Ok(ApiResponse<string>.Ok(null, "Cập nhật vai trò người dùng thành công."));
        }

        [HttpPost("create-admin")]
        public async Task<IActionResult> CreateAdminAcc(CreateAdminAccDto admin)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ApiResponse<object>.Fail("Dữ liệu không hợp lệ."));
            }
            try
            {
                var ad = await _userService.CreateAdminAccAsync(admin.Email, admin.Password);
                return Ok(ApiResponse<object>.Ok(ad, "Tạo tài khoản admin thành công."));
            }
            catch(InvalidOperationException ex)
            {
                return BadRequest(ApiResponse<string>.Fail(ex.Message));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<string>.Fail("Lỗi hệ thống", ex.Message));
            }
        }
    }
}
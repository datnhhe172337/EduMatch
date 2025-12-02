using AutoMapper;
using EduMatch.BusinessLogicLayer.DTOs;
using EduMatch.BusinessLogicLayer.Interfaces;
using EduMatch.BusinessLogicLayer.Requests.TutorVerificationRequest;
using EduMatch.BusinessLogicLayer.Services;
using EduMatch.DataAccessLayer.Entities;
using EduMatch.DataAccessLayer.Enum;
using EduMatch.DataAccessLayer.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EduMatch.BusinessLogicLayer.Services
{
    public class TutorVerificationRequestService : ITutorVerificationRequestService
    {
        private readonly ITutorVerificationRequestRepository _tutorVerificationRequestRepository;
        private readonly IUserRepository _userRepository;
        private readonly ITutorProfileRepository _tutorProfileRepository;
        private readonly IMapper _mapper;
        private readonly CurrentUserService _currentUserService;
        private readonly EduMatchContext _context;

        public TutorVerificationRequestService(
            ITutorVerificationRequestRepository tutorVerificationRequestRepository,
            IUserRepository userRepository,
            ITutorProfileRepository tutorProfileRepository,
            IMapper mapper,
            CurrentUserService currentUserService,
            EduMatchContext context)
        {
            _tutorVerificationRequestRepository = tutorVerificationRequestRepository;
            _userRepository = userRepository;
            _tutorProfileRepository = tutorProfileRepository;
            _mapper = mapper;
            _currentUserService = currentUserService;
            _context = context;
        }

        /// <summary>
        /// Lấy tất cả TutorVerificationRequest với lọc theo Status
        /// </summary>
        public async Task<List<TutorVerificationRequestDto>> GetAllAsync(TutorVerificationRequestStatus? status = null)
        {
            try
            {
                int? statusInt = status.HasValue ? (int?)status.Value : null;
                var entities = await _tutorVerificationRequestRepository.GetAllByStatusAsync(statusInt);
                return _mapper.Map<List<TutorVerificationRequestDto>>(entities);
            }
            catch (Exception ex)
            {
                throw new Exception($"Lỗi khi lấy danh sách yêu cầu xác minh gia sư: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Lấy tất cả TutorVerificationRequest theo Email hoặc TutorId với lọc theo Status
        /// </summary>
        public async Task<List<TutorVerificationRequestDto>> GetAllByEmailOrTutorIdAsync(string? email = null, int? tutorId = null, TutorVerificationRequestStatus? status = null)
        {
            try
            {
                int? statusInt = status.HasValue ? (int?)status.Value : null;
                var entities = await _tutorVerificationRequestRepository.GetAllByEmailOrTutorIdAsync(email, tutorId, statusInt);
                return _mapper.Map<List<TutorVerificationRequestDto>>(entities);
            }
            catch (Exception ex)
            {
                throw new Exception($"Lỗi khi lấy danh sách yêu cầu xác minh gia sư theo email hoặc tutorId: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Lấy TutorVerificationRequest theo ID
        /// </summary>
        public async Task<TutorVerificationRequestDto?> GetByIdAsync(int id)
        {
            try
            {
                if (id <= 0)
                    throw new Exception("Id phải lớn hơn 0");

                var entity = await _tutorVerificationRequestRepository.GetByIdAsync(id);
                return entity == null ? null : _mapper.Map<TutorVerificationRequestDto>(entity);
            }
            catch (Exception ex)
            {
                throw new Exception($"Lỗi khi lấy yêu cầu xác minh gia sư: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Tạo TutorVerificationRequest mới
        /// </summary>
        public async Task<TutorVerificationRequestDto> CreateAsync(TutorVerificationRequestCreateRequest request)
        {
            try
            {
                // Validate UserEmail tồn tại
                var user = await _userRepository.GetUserByEmailAsync(request.UserEmail);
                if (user == null)
                    throw new Exception("Người dùng không tồn tại");

                // Kiểm tra xem đã có TutorVerificationRequest nào với cùng UserEmail hoặc TutorId chưa
                var existingRequests = await _tutorVerificationRequestRepository.GetAllByEmailOrTutorIdAsync(
                    email: request.UserEmail,
                    tutorId: request.TutorId,
                    status: null);

                if (existingRequests != null && existingRequests.Any())
                {
                    // Kiểm tra xem có request nào đang ở trạng thái Pending hoặc Approved không
                    var hasPendingOrApproved = existingRequests.Any(r => 
                        r.Status == (int)TutorVerificationRequestStatus.Pending || 
                        r.Status == (int)TutorVerificationRequestStatus.Approved);

                    if (hasPendingOrApproved)
                    {
                        throw new Exception("Không thể tạo yêu cầu xác minh mới khi đã có yêu cầu đang ở trạng thái Chờ duyệt hoặc Đã duyệt. Chỉ có thể tạo mới khi tất cả các yêu cầu trước đó đều bị từ chối.");
                    }
                }

                var entity = new TutorVerificationRequest
                {
                    UserEmail = request.UserEmail,
                    TutorId = request.TutorId,
                    Description = request.Description,
                    Status = (int)TutorVerificationRequestStatus.Pending,
                    CreatedAt = DateTime.UtcNow
                };

                await _tutorVerificationRequestRepository.CreateAsync(entity);
                return _mapper.Map<TutorVerificationRequestDto>(entity);
            }
            catch (Exception ex)
            {
                throw new Exception($"Lỗi khi tạo yêu cầu xác minh gia sư: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Cập nhật TutorVerificationRequest
        /// </summary>
        public async Task<TutorVerificationRequestDto> UpdateAsync(TutorVerificationRequestUpdateRequest request)
        {
            try
            {
                if (request.Id <= 0)
                    throw new Exception("Id phải lớn hơn 0");

                var entity = await _tutorVerificationRequestRepository.GetByIdAsync(request.Id);
                if (entity == null)
                    throw new Exception("Yêu cầu xác minh gia sư không tồn tại");

                // Cập nhật các trường
                if (request.Description != null)
                    entity.Description = request.Description;

                if (request.AdminNote != null)
                    entity.AdminNote = request.AdminNote;

                await _tutorVerificationRequestRepository.UpdateAsync(entity);
                return _mapper.Map<TutorVerificationRequestDto>(entity);
            }
            catch (Exception ex)
            {
                throw new Exception($"Lỗi khi cập nhật yêu cầu xác minh gia sư: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Cập nhật Status của TutorVerificationRequest
        /// </summary>
        public async Task<TutorVerificationRequestDto> UpdateStatusAsync(int id, TutorVerificationRequestStatus status)
        {
            try
            {
                if (id <= 0)
                    throw new Exception("Id phải lớn hơn 0");

                var entity = await _tutorVerificationRequestRepository.GetByIdAsync(id);
                if (entity == null)
                    throw new Exception("Yêu cầu xác minh gia sư không tồn tại");

                var currentUserEmail = _currentUserService.Email;
                if (string.IsNullOrWhiteSpace(currentUserEmail))
                    throw new Exception("Không thể xác định người dùng hiện tại");

                var currentStatus = (TutorVerificationRequestStatus)entity.Status;
                var newStatus = status;

                // Validation: Chỉ cho phép cập nhật status theo thứ tự tăng dần
                // Không cho phép từ Approved về Rejected
                if (currentStatus == TutorVerificationRequestStatus.Approved && newStatus == TutorVerificationRequestStatus.Rejected)
                {
                    throw new Exception("Không thể chuyển trạng thái từ Đã duyệt sang Bị từ chối");
                }

                // Không cho phép từ Rejected về Approved
                if (currentStatus == TutorVerificationRequestStatus.Rejected && newStatus == TutorVerificationRequestStatus.Approved)
                {
                    throw new Exception("Không thể chuyển trạng thái từ Bị từ chối sang Đã duyệt");
                }

                // Không cho phép chuyển ngược lại (ví dụ từ Approved về Pending)
                if ((int)newStatus < (int)currentStatus)
                {
                    throw new Exception("Không thể chuyển trạng thái ngược lại. Chỉ được phép cập nhật theo thứ tự tăng dần.");
                }

                // Không cho phép cập nhật nếu đã ở trạng thái cuối cùng (Approved hoặc Rejected) và cố gắng giữ nguyên
                if (currentStatus == newStatus)
                {
                    throw new Exception($"Yêu cầu đã ở trạng thái {newStatus}");
                }

                entity.Status = (int)status;
                entity.ProcessedAt = DateTime.UtcNow;
                entity.ProcessedBy = currentUserEmail;

                var now = DateTime.UtcNow;

                // Nếu update từ Pending sang Approved, cần update status của TutorProfile thành Approved
                if (currentStatus == TutorVerificationRequestStatus.Pending && newStatus == TutorVerificationRequestStatus.Approved)
                {
                    if (entity.TutorId.HasValue)
                    {
                        var tutorProfile = await _tutorProfileRepository.GetByIdFullAsync(entity.TutorId.Value);
                        if (tutorProfile != null)
                        {
                            tutorProfile.Status = (int)TutorStatus.Approved;
                            tutorProfile.VerifiedBy = currentUserEmail;
                            tutorProfile.VerifiedAt = now;
                            tutorProfile.UpdatedAt = now;
                            await _tutorProfileRepository.UpdateAsync(tutorProfile);
                        }
                    }
                }

                // Nếu update từ Pending sang Rejected, cần update status của TutorProfile thành Rejected
                if (currentStatus == TutorVerificationRequestStatus.Pending && newStatus == TutorVerificationRequestStatus.Rejected)
                {
                    if (entity.TutorId.HasValue)
                    {
                        var tutorProfile = await _tutorProfileRepository.GetByIdFullAsync(entity.TutorId.Value);
                        if (tutorProfile != null)
                        {
                            tutorProfile.Status = (int)TutorStatus.Rejected;
                            tutorProfile.VerifiedBy = currentUserEmail;
                            tutorProfile.VerifiedAt = now;
                            tutorProfile.UpdatedAt = now;
                            await _tutorProfileRepository.UpdateAsync(tutorProfile);
                        }
                    }
                }

                await _tutorVerificationRequestRepository.UpdateAsync(entity);

                return _mapper.Map<TutorVerificationRequestDto>(entity);
            }
            catch (Exception ex)
            {
                throw new Exception($"Lỗi khi cập nhật trạng thái yêu cầu xác minh gia sư: {ex.Message}", ex);
            }
        }
    }
}


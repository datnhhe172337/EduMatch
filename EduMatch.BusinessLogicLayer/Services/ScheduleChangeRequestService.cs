using AutoMapper;
using EduMatch.BusinessLogicLayer.DTOs;
using EduMatch.BusinessLogicLayer.Interfaces;
using EduMatch.BusinessLogicLayer.Requests.ScheduleChangeRequest;
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
    public class ScheduleChangeRequestService : IScheduleChangeRequestService
    {
        private readonly IScheduleChangeRequestRepository _scheduleChangeRequestRepository;
        private readonly IScheduleRepository _scheduleRepository;
        private readonly IUserRepository _userRepository;
        private readonly ITutorAvailabilityRepository _tutorAvailabilityRepository;
        private readonly EduMatchContext _context;
        private readonly IMapper _mapper;

        public ScheduleChangeRequestService(
            IScheduleChangeRequestRepository scheduleChangeRequestRepository,
            IScheduleRepository scheduleRepository,
            IUserRepository userRepository,
            ITutorAvailabilityRepository tutorAvailabilityRepository,
            EduMatchContext context,
            IMapper mapper)
        {
            _scheduleChangeRequestRepository = scheduleChangeRequestRepository;
            _scheduleRepository = scheduleRepository;
            _userRepository = userRepository;
            _tutorAvailabilityRepository = tutorAvailabilityRepository;
            _context = context;
            _mapper = mapper;
        }

        /// <summary>
        /// Lấy ScheduleChangeRequest theo ID
        /// </summary>
        public async Task<ScheduleChangeRequestDto?> GetByIdAsync(int id)
        {
            if (id <= 0)
                throw new ArgumentException("ID phải lớn hơn 0");

            var entity = await _scheduleChangeRequestRepository.GetByIdAsync(id);
            return entity == null ? null : _mapper.Map<ScheduleChangeRequestDto>(entity);
        }

        /// <summary>
        /// Tạo ScheduleChangeRequest mới
        /// </summary>
        public async Task<ScheduleChangeRequestDto> CreateAsync(ScheduleChangeRequestCreateRequest request)
        {
            using var dbTransaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // Check Schedule tồn tại
                var schedule = await _scheduleRepository.GetByIdAsync(request.ScheduleId)
                    ?? throw new Exception($"CreateAsync - Schedule không tồn tại với ScheduleId: {request.ScheduleId}");

                // Check RequesterEmail tồn tại
                var requesterUser = await _userRepository.GetUserByEmailAsync(request.RequesterEmail)
                    ?? throw new Exception($"CreateAsync - RequesterEmail '{request.RequesterEmail}' không tồn tại");

                // Check RequestedToEmail tồn tại
                var requestedToUser = await _userRepository.GetUserByEmailAsync(request.RequestedToEmail)
                    ?? throw new Exception($"CreateAsync - RequestedToEmail '{request.RequestedToEmail}' không tồn tại");

                // Check OldAvailabiliti tồn tại
                var oldAvailability = await _tutorAvailabilityRepository.GetByIdFullAsync(request.OldAvailabilitiId)
                    ?? throw new Exception($"CreateAsync - OldAvailabiliti không tồn tại với OldAvailabilitiId: {request.OldAvailabilitiId}");

                // Check NewAvailabiliti tồn tại và phải Available
                var newAvailability = await _tutorAvailabilityRepository.GetByIdFullAsync(request.NewAvailabilitiId)
                    ?? throw new Exception($"CreateAsync - NewAvailabiliti không tồn tại với NewAvailabilitiId: {request.NewAvailabilitiId}");

                if (newAvailability.Status != (int)TutorAvailabilityStatus.Available)
                    throw new Exception($"CreateAsync - NewAvailabiliti phải ở trạng thái Available, hiện tại: {newAvailability.Status}");

                // Map request to entity manually
                var entity = new ScheduleChangeRequest
                {
                    ScheduleId = request.ScheduleId,
                    RequesterEmail = request.RequesterEmail,
                    RequestedToEmail = request.RequestedToEmail,
                    OldAvailabilitiId = request.OldAvailabilitiId,
                    NewAvailabilitiId = request.NewAvailabilitiId,
                    Reason = request.Reason,
                    Status = (int)ScheduleChangeRequestStatus.Pending,
                    CreatedAt = DateTime.UtcNow
                };

                // Create ScheduleChangeRequest
                await _scheduleChangeRequestRepository.CreateAsync(entity);

                // Nếu NewAvailabiliti là Available thì chuyển sang Booked
                if (newAvailability.Status == (int)TutorAvailabilityStatus.Available)
                {
                    newAvailability.Status = (int)TutorAvailabilityStatus.Booked;
                    await _tutorAvailabilityRepository.UpdateAsync(newAvailability);
                }

                // Commit transaction
                await dbTransaction.CommitAsync();

                // Map entity sang DTO
                return _mapper.Map<ScheduleChangeRequestDto>(entity);
            }
            catch (Exception ex)
            {
                await dbTransaction.RollbackAsync();
                throw new Exception($"CreateAsync - Lỗi khi tạo ScheduleChangeRequest: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Cập nhật ScheduleChangeRequest
        /// </summary>
        public async Task<ScheduleChangeRequestDto> UpdateAsync(ScheduleChangeRequestUpdateRequest request)
        {
            try
            {
                // Check ScheduleChangeRequest tồn tại
                var entity = await _scheduleChangeRequestRepository.GetByIdAsync(request.Id)
                    ?? throw new Exception($"UpdateAsync - ScheduleChangeRequest không tồn tại với Id: {request.Id}");

                // Check Schedule tồn tại nếu có thay đổi
                if (request.ScheduleId.HasValue)
                {
                    var schedule = await _scheduleRepository.GetByIdAsync(request.ScheduleId.Value)
                        ?? throw new Exception($"UpdateAsync - Schedule không tồn tại với ScheduleId: {request.ScheduleId.Value}");
                    entity.ScheduleId = request.ScheduleId.Value;
                }

                // Check RequesterEmail tồn tại nếu có thay đổi
                if (!string.IsNullOrWhiteSpace(request.RequesterEmail))
                {
                    var requesterUser = await _userRepository.GetUserByEmailAsync(request.RequesterEmail)
                        ?? throw new Exception($"UpdateAsync - RequesterEmail '{request.RequesterEmail}' không tồn tại");
                    entity.RequesterEmail = request.RequesterEmail;
                }

                // Check RequestedToEmail tồn tại nếu có thay đổi
                if (!string.IsNullOrWhiteSpace(request.RequestedToEmail))
                {
                    var requestedToUser = await _userRepository.GetUserByEmailAsync(request.RequestedToEmail)
                        ?? throw new Exception($"UpdateAsync - RequestedToEmail '{request.RequestedToEmail}' không tồn tại");
                    entity.RequestedToEmail = request.RequestedToEmail;
                }

                // Check OldAvailabiliti tồn tại nếu có thay đổi
                if (request.OldAvailabilitiId.HasValue)
                {
                    var oldAvailability = await _tutorAvailabilityRepository.GetByIdFullAsync(request.OldAvailabilitiId.Value)
                        ?? throw new Exception($"UpdateAsync - OldAvailabiliti không tồn tại với OldAvailabilitiId: {request.OldAvailabilitiId.Value}");
                    entity.OldAvailabilitiId = request.OldAvailabilitiId.Value;
                }

                // Check NewAvailabiliti tồn tại và phải Available nếu có thay đổi
                if (request.NewAvailabilitiId.HasValue)
                {
                    var newAvailability = await _tutorAvailabilityRepository.GetByIdFullAsync(request.NewAvailabilitiId.Value)
                        ?? throw new Exception($"UpdateAsync - NewAvailabiliti không tồn tại với NewAvailabilitiId: {request.NewAvailabilitiId.Value}");

                    if (newAvailability.Status != (int)TutorAvailabilityStatus.Available)
                        throw new Exception($"UpdateAsync - NewAvailabiliti phải ở trạng thái Available, hiện tại: {newAvailability.Status}");

                    // Nếu đổi NewAvailabiliti, trả Old NewAvailabiliti về Available và chuyển New NewAvailabiliti sang Booked
                    var oldNewAvailability = await _tutorAvailabilityRepository.GetByIdFullAsync(entity.NewAvailabilitiId);
                    if (oldNewAvailability != null && oldNewAvailability.Id != request.NewAvailabilitiId.Value)
                    {
                        // Trả Old NewAvailabiliti về Available
                        oldNewAvailability.Status = (int)TutorAvailabilityStatus.Available;
                        await _tutorAvailabilityRepository.UpdateAsync(oldNewAvailability);

                        // Chuyển New NewAvailabiliti sang Booked
                        newAvailability.Status = (int)TutorAvailabilityStatus.Booked;
                        await _tutorAvailabilityRepository.UpdateAsync(newAvailability);
                    }
                    else if (oldNewAvailability == null || oldNewAvailability.Id == request.NewAvailabilitiId.Value)
                    {
                        // Nếu là lần đầu set hoặc giữ nguyên, chỉ cần chuyển sang Booked nếu chưa Booked
                        if (newAvailability.Status == (int)TutorAvailabilityStatus.Available)
                        {
                            newAvailability.Status = (int)TutorAvailabilityStatus.Booked;
                            await _tutorAvailabilityRepository.UpdateAsync(newAvailability);
                        }
                    }

                    entity.NewAvailabilitiId = request.NewAvailabilitiId.Value;
                }

                // Update Reason nếu có
                if (request.Reason != null)
                {
                    entity.Reason = request.Reason;
                }

                // Update Status nếu có
                if (request.Status.HasValue)
                {
                    entity.Status = (int)request.Status.Value;
                    if (request.Status.Value != ScheduleChangeRequestStatus.Pending)
                    {
                        entity.ProcessedAt = DateTime.UtcNow;
                    }
                }

                // Update ScheduleChangeRequest
                await _scheduleChangeRequestRepository.UpdateAsync(entity);

                // Map entity sang DTO
                return _mapper.Map<ScheduleChangeRequestDto>(entity);
            }
            catch (Exception ex)
            {
                throw new Exception($"UpdateAsync - Lỗi khi cập nhật ScheduleChangeRequest với Id: {request.Id}: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Cập nhật Status của ScheduleChangeRequest
        /// </summary>
        public async Task<ScheduleChangeRequestDto> UpdateStatusAsync(int id, ScheduleChangeRequestStatus status)
        {
            try
            {
                if (id <= 0)
                    throw new ArgumentException($"UpdateStatusAsync - ID phải lớn hơn 0, nhận được: {id}");

                var entity = await _scheduleChangeRequestRepository.GetByIdAsync(id)
                    ?? throw new Exception($"UpdateStatusAsync - ScheduleChangeRequest không tồn tại với Id: {id}");

                entity.Status = (int)status;
                if (status != ScheduleChangeRequestStatus.Pending)
                {
                    entity.ProcessedAt = DateTime.UtcNow;
                }

                await _scheduleChangeRequestRepository.UpdateAsync(entity);

                // Map entity sang DTO
                return _mapper.Map<ScheduleChangeRequestDto>(entity);
            }
            catch (Exception ex)
            {
                throw new Exception($"UpdateStatusAsync - Lỗi khi cập nhật Status của ScheduleChangeRequest với Id: {id}, Status: {status}: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Lấy danh sách ScheduleChangeRequest theo RequesterEmail
        /// </summary>
        public async Task<List<ScheduleChangeRequestDto>> GetAllByRequesterEmailAsync(string requesterEmail)
        {
            if (string.IsNullOrWhiteSpace(requesterEmail))
                throw new ArgumentException("RequesterEmail không được để trống");

            var entities = await _scheduleChangeRequestRepository.GetAllByRequesterEmailAsync(requesterEmail);
            return _mapper.Map<List<ScheduleChangeRequestDto>>(entities);
        }

        /// <summary>
        /// Lấy danh sách ScheduleChangeRequest theo RequestedToEmail
        /// </summary>
        public async Task<List<ScheduleChangeRequestDto>> GetAllByRequestedToEmailAsync(string requestedToEmail)
        {
            if (string.IsNullOrWhiteSpace(requestedToEmail))
                throw new ArgumentException("RequestedToEmail không được để trống");

            var entities = await _scheduleChangeRequestRepository.GetAllByRequestedToEmailAsync(requestedToEmail);
            return _mapper.Map<List<ScheduleChangeRequestDto>>(entities);
        }
    }
}


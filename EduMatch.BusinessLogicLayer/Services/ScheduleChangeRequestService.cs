using AutoMapper;
using EduMatch.BusinessLogicLayer.DTOs;
using EduMatch.BusinessLogicLayer.Interfaces;
using EduMatch.BusinessLogicLayer.Requests.ScheduleChangeRequest;
using EduMatch.DataAccessLayer.Entities;
using EduMatch.DataAccessLayer.Enum;
using EduMatch.DataAccessLayer.Interfaces;
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
        private readonly IMapper _mapper;

        public ScheduleChangeRequestService(
            IScheduleChangeRequestRepository scheduleChangeRequestRepository,
            IScheduleRepository scheduleRepository,
            IUserRepository userRepository,
            ITutorAvailabilityRepository tutorAvailabilityRepository,
            IMapper mapper)
        {
            _scheduleChangeRequestRepository = scheduleChangeRequestRepository;
            _scheduleRepository = scheduleRepository;
            _userRepository = userRepository;
            _tutorAvailabilityRepository = tutorAvailabilityRepository;
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
            // Check Schedule tồn tại
            var schedule = await _scheduleRepository.GetByIdAsync(request.ScheduleId)
                ?? throw new Exception("Schedule không tồn tại");

            // Check RequesterEmail tồn tại
            var requesterUser = await _userRepository.GetUserByEmailAsync(request.RequesterEmail)
                ?? throw new Exception($"RequesterEmail '{request.RequesterEmail}' không tồn tại");

            // Check RequestedToEmail tồn tại
            var requestedToUser = await _userRepository.GetUserByEmailAsync(request.RequestedToEmail)
                ?? throw new Exception($"RequestedToEmail '{request.RequestedToEmail}' không tồn tại");

            // Check OldAvailabiliti tồn tại
            var oldAvailability = await _tutorAvailabilityRepository.GetByIdFullAsync(request.OldAvailabilitiId)
                ?? throw new Exception("OldAvailabiliti không tồn tại");

            // Check NewAvailabiliti tồn tại và phải Available
            var newAvailability = await _tutorAvailabilityRepository.GetByIdFullAsync(request.NewAvailabilitiId)
                ?? throw new Exception("NewAvailabiliti không tồn tại");

            if (newAvailability.Status != (int)TutorAvailabilityStatus.Available)
                throw new Exception("NewAvailabiliti phải ở trạng thái Available");

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

            // Chuyển NewAvailabiliti sang Booked
            newAvailability.Status = (int)TutorAvailabilityStatus.Booked;
            await _tutorAvailabilityRepository.UpdateAsync(newAvailability);

            // Reload entity với đầy đủ thông tin
            var createdEntity = await _scheduleChangeRequestRepository.GetByIdAsync(entity.Id);
            return _mapper.Map<ScheduleChangeRequestDto>(createdEntity);
        }

        /// <summary>
        /// Cập nhật ScheduleChangeRequest
        /// </summary>
        public async Task<ScheduleChangeRequestDto> UpdateAsync(ScheduleChangeRequestUpdateRequest request)
        {
            // Check ScheduleChangeRequest tồn tại
            var entity = await _scheduleChangeRequestRepository.GetByIdAsync(request.Id)
                ?? throw new Exception("ScheduleChangeRequest không tồn tại");

            // Check Schedule tồn tại nếu có thay đổi
            if (request.ScheduleId.HasValue)
            {
                var schedule = await _scheduleRepository.GetByIdAsync(request.ScheduleId.Value)
                    ?? throw new Exception("Schedule không tồn tại");
                entity.ScheduleId = request.ScheduleId.Value;
            }

            // Check RequesterEmail tồn tại nếu có thay đổi
            if (!string.IsNullOrWhiteSpace(request.RequesterEmail))
            {
                var requesterUser = await _userRepository.GetUserByEmailAsync(request.RequesterEmail)
                    ?? throw new Exception($"RequesterEmail '{request.RequesterEmail}' không tồn tại");
                entity.RequesterEmail = request.RequesterEmail;
            }

            // Check RequestedToEmail tồn tại nếu có thay đổi
            if (!string.IsNullOrWhiteSpace(request.RequestedToEmail))
            {
                var requestedToUser = await _userRepository.GetUserByEmailAsync(request.RequestedToEmail)
                    ?? throw new Exception($"RequestedToEmail '{request.RequestedToEmail}' không tồn tại");
                entity.RequestedToEmail = request.RequestedToEmail;
            }

            // Check OldAvailabiliti tồn tại nếu có thay đổi
            if (request.OldAvailabilitiId.HasValue)
            {
                var oldAvailability = await _tutorAvailabilityRepository.GetByIdFullAsync(request.OldAvailabilitiId.Value)
                    ?? throw new Exception("OldAvailabiliti không tồn tại");
                entity.OldAvailabilitiId = request.OldAvailabilitiId.Value;
            }

            // Check NewAvailabiliti tồn tại và phải Available nếu có thay đổi
            if (request.NewAvailabilitiId.HasValue)
            {
                var newAvailability = await _tutorAvailabilityRepository.GetByIdFullAsync(request.NewAvailabilitiId.Value)
                    ?? throw new Exception("NewAvailabiliti không tồn tại");

                if (newAvailability.Status != (int)TutorAvailabilityStatus.Available)
                    throw new Exception("NewAvailabiliti phải ở trạng thái Available");

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

            // Reload entity với đầy đủ thông tin
            var updatedEntity = await _scheduleChangeRequestRepository.GetByIdAsync(entity.Id);
            return _mapper.Map<ScheduleChangeRequestDto>(updatedEntity);
        }

        /// <summary>
        /// Cập nhật Status của ScheduleChangeRequest
        /// </summary>
        public async Task<ScheduleChangeRequestDto> UpdateStatusAsync(int id, ScheduleChangeRequestStatus status)
        {
            if (id <= 0)
                throw new ArgumentException("ID phải lớn hơn 0");

            var entity = await _scheduleChangeRequestRepository.GetByIdAsync(id)
                ?? throw new Exception("ScheduleChangeRequest không tồn tại");

            entity.Status = (int)status;
            if (status != ScheduleChangeRequestStatus.Pending)
            {
                entity.ProcessedAt = DateTime.UtcNow;
            }

            await _scheduleChangeRequestRepository.UpdateAsync(entity);

            // Reload entity với đầy đủ thông tin
            var updatedEntity = await _scheduleChangeRequestRepository.GetByIdAsync(id);
            return _mapper.Map<ScheduleChangeRequestDto>(updatedEntity);
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


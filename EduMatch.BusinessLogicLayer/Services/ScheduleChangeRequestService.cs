using AutoMapper;
using EduMatch.BusinessLogicLayer.DTOs;
using EduMatch.BusinessLogicLayer.Interfaces;
using EduMatch.BusinessLogicLayer.Requests.ScheduleChangeRequest;
using EduMatch.BusinessLogicLayer.Requests.Schedule;
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
        private readonly IScheduleService _scheduleService;
        private readonly IUserRepository _userRepository;
        private readonly ITutorAvailabilityRepository _tutorAvailabilityRepository;
        private readonly EduMatchContext _context;
        private readonly IMapper _mapper;
        private readonly INotificationService _notificationService;
        private readonly EmailService _emailService;

        public ScheduleChangeRequestService(
            IScheduleChangeRequestRepository scheduleChangeRequestRepository,
            IScheduleService scheduleService,
            IUserRepository userRepository,
            ITutorAvailabilityRepository tutorAvailabilityRepository,
            EduMatchContext context,
            IMapper mapper,
            INotificationService notificationService,
            EmailService emailService)
        {
            _scheduleChangeRequestRepository = scheduleChangeRequestRepository;
            _scheduleService = scheduleService;
            _userRepository = userRepository;
            _tutorAvailabilityRepository = tutorAvailabilityRepository;
            _context = context;
            _mapper = mapper;
            _notificationService = notificationService;
            _emailService = emailService;
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
                var schedule = await _scheduleService.GetByIdAsync(request.ScheduleId)
                    ?? throw new Exception($"Schedule không tồn tại với ScheduleId: {request.ScheduleId}");

                // Check RequesterEmail tồn tại
                var requesterUser = await _userRepository.GetUserByEmailAsync(request.RequesterEmail)
                    ?? throw new Exception($"RequesterEmail '{request.RequesterEmail}' không tồn tại");

                // Check RequestedToEmail tồn tại
                var requestedToUser = await _userRepository.GetUserByEmailAsync(request.RequestedToEmail)
                    ?? throw new Exception($"RequestedToEmail '{request.RequestedToEmail}' không tồn tại");

                // Check OldAvailabiliti tồn tại
                var oldAvailability = await _tutorAvailabilityRepository.GetByIdFullAsync(request.OldAvailabilitiId)
                    ?? throw new Exception($"OldAvailabiliti không tồn tại với OldAvailabilitiId: {request.OldAvailabilitiId}");

                // Check NewAvailabiliti tồn tại và phải Available
                var newAvailability = await _tutorAvailabilityRepository.GetByIdFullAsync(request.NewAvailabilitiId)
                    ?? throw new Exception($"NewAvailabiliti không tồn tại với NewAvailabilitiId: {request.NewAvailabilitiId}");

                if (newAvailability.Status != (int)TutorAvailabilityStatus.Available)
                    throw new Exception($"NewAvailabiliti phải ở trạng thái Available, hiện tại: {newAvailability.Status}");

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
                    CreatedAt = DateTime.Now
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

               
                // Gửi notification cho người được yêu cầu (RequestedToEmail)
                if (!string.IsNullOrWhiteSpace(entity.RequestedToEmail))
                {
                    await _notificationService.CreateNotificationAsync(
                        entity.RequestedToEmail,
                        $"Bạn có yêu cầu thay đổi lịch học mới từ {entity.RequesterEmail}. Vui lòng xem xét và phản hồi.",
                        $"/schedule-change-requests/{entity.Id}");

                    // Gửi email thông báo cho người được yêu cầu
                    try
                    {
                        await _emailService.SendScheduleChangeRequestCreatedAsync(
                            entity.RequestedToEmail, 
                            entity.RequesterEmail,
                            entity.RequestedToEmail,
							oldAvailability.StartDate,
                            newAvailability.StartDate,
                            entity.Reason);
                    }
                    catch (Exception emailEx)
                    {
                        Console.WriteLine($"[ScheduleChangeRequest] Error sending email to {entity.RequestedToEmail}: {emailEx.Message}");
                    }
                }
				// Gửi notification cho người  yêu cầu (RequesterEmail)
				if (!string.IsNullOrWhiteSpace(entity.RequesterEmail))
				{
					await _notificationService.CreateNotificationAsync(
						entity.RequesterEmail,
						$"Yêu cầu chuyển lịch của bạn tạo thành công. Vui lòng chờ phản hồi từ {entity.RequestedToEmail}.",
						$"/schedule-change-requests/{entity.Id}");
				}

				// Map entity sang DTO
				return _mapper.Map<ScheduleChangeRequestDto>(entity);
            }
            catch (Exception ex)
            {
                await dbTransaction.RollbackAsync();
                throw new Exception($"Lỗi khi tạo ScheduleChangeRequest: {ex.Message}", ex);
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
                    ?? throw new Exception($"ScheduleChangeRequest không tồn tại với Id: {request.Id}");

                // Check Schedule tồn tại nếu có thay đổi
                if (request.ScheduleId.HasValue)
                {
                    var schedule = await _scheduleService.GetByIdAsync(request.ScheduleId.Value)
                        ?? throw new Exception($"Schedule không tồn tại với ScheduleId: {request.ScheduleId.Value}");
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
                        ?? throw new Exception($"OldAvailabiliti không tồn tại với OldAvailabilitiId: {request.OldAvailabilitiId.Value}");
                    entity.OldAvailabilitiId = request.OldAvailabilitiId.Value;
                }

                // Check NewAvailabiliti tồn tại và phải Available nếu có thay đổi
                if (request.NewAvailabilitiId.HasValue)
                {
                    var newAvailability = await _tutorAvailabilityRepository.GetByIdFullAsync(request.NewAvailabilitiId.Value)
                        ?? throw new Exception($"NewAvailabiliti không tồn tại với NewAvailabilitiId: {request.NewAvailabilitiId.Value}");

                    if (newAvailability.Status != (int)TutorAvailabilityStatus.Available)
                        throw new Exception($"NewAvailabiliti phải ở trạng thái Available, hiện tại: {newAvailability.Status}");

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

                // Update ScheduleChangeRequest
                await _scheduleChangeRequestRepository.UpdateAsync(entity);

                // Map entity sang DTO
                return _mapper.Map<ScheduleChangeRequestDto>(entity);
            }
            catch (Exception ex)
            {
                throw new Exception($"Lỗi khi cập nhật ScheduleChangeRequest với Id: {request.Id}: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Cập nhật Status của ScheduleChangeRequest
        /// </summary>
        public async Task<ScheduleChangeRequestDto> UpdateStatusAsync(int id, ScheduleChangeRequestStatus status)
        {
            using var dbTransaction = await _context.Database.BeginTransactionAsync();
            try
            {
                if (id <= 0)
                    throw new ArgumentException($"ID phải lớn hơn 0, nhận được: {id}");

                var entity = await _scheduleChangeRequestRepository.GetByIdAsync(id)
                    ?? throw new Exception($"ScheduleChangeRequest không tồn tại với Id: {id}");

                var oldStatus = (ScheduleChangeRequestStatus)entity.Status;

                // Không cho phép từ Approved sang Rejected
                if (oldStatus == ScheduleChangeRequestStatus.Approved && status == ScheduleChangeRequestStatus.Rejected)
                {
                    throw new Exception("Không thể chuyển trạng thái từ Đã chấp nhận sang Đã từ chối");
                }

                // Không cho phép từ Rejected sang Approved
                if (oldStatus == ScheduleChangeRequestStatus.Rejected && status == ScheduleChangeRequestStatus.Approved)
                {
                    throw new Exception("Không thể chuyển trạng thái từ Đã từ chối sang Đã chấp nhận");
                }

                // Không cho phép update ngược lại: status mới phải >= status cũ (theo giá trị enum)
                if ((int)status < (int)oldStatus)
                {
                    throw new Exception($"Không thể cập nhật Status từ {oldStatus} về {status}. Chỉ cho phép chuyển từ status nhỏ hơn sang status lớn hơn");
                }

                // Nếu từ Pending sang Approved: update OldAvailabiliti về Available và update Schedule
                if (oldStatus == ScheduleChangeRequestStatus.Pending && status == ScheduleChangeRequestStatus.Approved)
                {
                    // Update OldAvailabiliti về Available
                    var oldAvailability = await _tutorAvailabilityRepository.GetByIdFullAsync(entity.OldAvailabilitiId);
                    if (oldAvailability != null)
                    {
                        oldAvailability.Status = (int)TutorAvailabilityStatus.Available;
                        await _tutorAvailabilityRepository.UpdateAsync(oldAvailability);
                    }

                    // Đặt NewAvailabiliti về Available trước khi gọi ScheduleService.UpdateAsync
                    // (vì ScheduleService.UpdateAsync yêu cầu availability mới phải ở trạng thái Available)
                    var newAvailability = await _tutorAvailabilityRepository.GetByIdFullAsync(entity.NewAvailabilitiId);
                    if (newAvailability != null && newAvailability.Status != (int)TutorAvailabilityStatus.Available)
                    {
                        newAvailability.Status = (int)TutorAvailabilityStatus.Available;
                        await _tutorAvailabilityRepository.UpdateAsync(newAvailability);
                    }

                    // Update Schedule: chuyển AvailabilitiId từ Old sang New (dùng service để đồng bộ MeetingSession)
                    // ScheduleService.UpdateAsync sẽ tự động set NewAvailabiliti về Booked
                    // Không truyền IsOnline để ScheduleService tự xử lý:
                    // - Nếu có MeetingSession: sẽ cập nhật nó
                    // - Nếu không có MeetingSession: không tạo mới (tránh yêu cầu SystemAccountEmail)
                    var scheduleUpdateRequest = new ScheduleUpdateRequest
                    {
                        Id = entity.ScheduleId,
                        AvailabilitiId = entity.NewAvailabilitiId
                        // Không truyền IsOnline: ScheduleService sẽ chỉ cập nhật MeetingSession nếu đã tồn tại
                    };
                    await _scheduleService.UpdateAsync(scheduleUpdateRequest);
                }

                // Nếu từ Pending sang Rejected hoặc Cancelled: update NewAvailabiliti về Available
                if (oldStatus == ScheduleChangeRequestStatus.Pending && 
                    (status == ScheduleChangeRequestStatus.Rejected || status == ScheduleChangeRequestStatus.Cancelled))
                {
                    var newAvailability = await _tutorAvailabilityRepository.GetByIdFullAsync(entity.NewAvailabilitiId);
                    if (newAvailability != null)
                    {
                        newAvailability.Status = (int)TutorAvailabilityStatus.Available;
                        await _tutorAvailabilityRepository.UpdateAsync(newAvailability);
                    }
                }

                entity.Status = (int)status;
                if (status != ScheduleChangeRequestStatus.Pending)
                {
                    entity.ProcessedAt = DateTime.Now;
                }

                await _scheduleChangeRequestRepository.UpdateAsync(entity);

                // Commit transaction
                await dbTransaction.CommitAsync();

                  // Gửi thông báo cho các bên liên quan
                if (status == ScheduleChangeRequestStatus.Approved)
                {
                    // Thông báo cho người yêu cầu (RequesterEmail)
                    await _notificationService.CreateNotificationAsync(
                        entity.RequesterEmail,
                        "Yêu cầu thay đổi lịch học của bạn đã được chấp nhận. Lịch học đã được cập nhật.",
                        $"/schedule/{entity.ScheduleId}");

                    // Gửi email thông báo cho người yêu cầu
                    if (entity.OldAvailabiliti != null && entity.NewAvailabiliti != null)
                    {
                        try
                        {
                            await _emailService.SendScheduleChangeRequestApprovedAsync(
                                entity.RequesterEmail,
                                entity.RequestedToEmail,
								entity.OldAvailabiliti.StartDate,
                                entity.NewAvailabiliti.StartDate);
                        }
                        catch (Exception emailEx)
                        {
                            Console.WriteLine($"[ScheduleChangeRequest] Error sending approval email to {entity.RequesterEmail}: {emailEx.Message}");
                        }
                    }
                }
                else if (status == ScheduleChangeRequestStatus.Rejected)
                {
                    // Thông báo cho người yêu cầu (RequesterEmail)
                    var rejectMessage = !string.IsNullOrWhiteSpace(entity.Reason)
                        ? $"Yêu cầu thay đổi lịch học của bạn đã bị từ chối. Lý do: {entity.Reason}."
                        : "Yêu cầu thay đổi lịch học của bạn đã bị từ chối.";

                    await _notificationService.CreateNotificationAsync(
                        entity.RequesterEmail,
                        rejectMessage,
                        $"/schedule/{entity.ScheduleId}");


					// Gửi email thông báo cho người yêu cầu
					if (entity.OldAvailabiliti != null && entity.NewAvailabiliti != null)
					{
						try
						{
							await _emailService.SendScheduleChangeRequestRejectedAsync(
								entity.RequesterEmail,
								entity.RequestedToEmail,
								entity.OldAvailabiliti.StartDate,
								entity.NewAvailabiliti.StartDate);
						}
						catch (Exception emailEx)
						{
							Console.WriteLine($"[ScheduleChangeRequest] Error sending reject email to {entity.RequesterEmail}: {emailEx.Message}");
						}
					}
				}
                else if (status == ScheduleChangeRequestStatus.Cancelled)
                {
                    // Thông báo cho cả hai bên khi hủy
                    await _notificationService.CreateNotificationAsync(
                        entity.RequesterEmail,
                        "Yêu cầu thay đổi lịch học của bạn đã bị hủy.",
                        $"/schedule/{entity.ScheduleId}");
                   
                }

                // Map entity sang DTO
                return _mapper.Map<ScheduleChangeRequestDto>(entity);
            }
            catch (Exception ex)
            {
                await dbTransaction.RollbackAsync();
                throw new Exception($"Lỗi khi cập nhật Status của ScheduleChangeRequest với Id: {id}, Status: {status}: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Lấy danh sách ScheduleChangeRequest theo RequesterEmail
        /// </summary>
        public async Task<List<ScheduleChangeRequestDto>> GetAllByRequesterEmailAsync(string requesterEmail, ScheduleChangeRequestStatus? status = null)
        {
            if (string.IsNullOrWhiteSpace(requesterEmail))
                throw new ArgumentException("RequesterEmail không được để trống");

            int? statusInt = status.HasValue ? (int?)status.Value : null;
            var entities = await _scheduleChangeRequestRepository.GetAllByRequesterEmailAsync(requesterEmail, statusInt);
            return _mapper.Map<List<ScheduleChangeRequestDto>>(entities);
        }

        /// <summary>
        /// Lấy danh sách ScheduleChangeRequest theo RequestedToEmail
        /// </summary>
        public async Task<List<ScheduleChangeRequestDto>> GetAllByRequestedToEmailAsync(string requestedToEmail, ScheduleChangeRequestStatus? status = null)
        {
            if (string.IsNullOrWhiteSpace(requestedToEmail))
                throw new ArgumentException("RequestedToEmail không được để trống");

            int? statusInt = status.HasValue ? (int?)status.Value : null;
            var entities = await _scheduleChangeRequestRepository.GetAllByRequestedToEmailAsync(requestedToEmail, statusInt);
            return _mapper.Map<List<ScheduleChangeRequestDto>>(entities);
        }

        /// <summary>
        /// Lấy danh sách ScheduleChangeRequest theo ScheduleId (có thể lọc theo status)
        /// </summary>
        public async Task<List<ScheduleChangeRequestDto>> GetAllByScheduleIdAsync(int scheduleId, ScheduleChangeRequestStatus? status = null)
        {
            if (scheduleId <= 0)
                throw new ArgumentException("scheduleId phải lớn hơn 0");

            int? statusInt = null;
            if (status.HasValue)
            {
                if (!Enum.IsDefined(typeof(ScheduleChangeRequestStatus), status.Value))
                    throw new ArgumentException("Status không hợp lệ");
                statusInt = (int)status.Value;
            }

            var entities = await _scheduleChangeRequestRepository.GetAllByScheduleIdAsync(scheduleId, statusInt);
            return _mapper.Map<List<ScheduleChangeRequestDto>>(entities);
        }

        /// <summary>
        /// Tự động hủy các ScheduleChangeRequest Pending quá 3 ngày hoặc sắp đến giờ học
        /// </summary>
        public async Task<int> AutoCancelExpiredPendingRequestsAsync()
        {
            int cancelledCount = 0;
            try
            {
                var pendingRequests = await _scheduleChangeRequestRepository.GetAllPendingAsync();
                var systemNow = DateTime.Now; // Giờ hệ thống
                var utcNow = DateTime.UtcNow;
                var vietnamNow = utcNow.AddHours(7); // Giờ Việt Nam (UTC+7)

                foreach (var request in pendingRequests)
                {
                    bool shouldCancel = false;

                    // Check 1: Nếu Pending quá 3 ngày (dùng giờ hệ thống)
                    if (request.CreatedAt.AddDays(3) < systemNow)
                    {
                        shouldCancel = true;
                    }

                    // Check 2: Nếu gần sát giờ học 1 tiếng (dùng giờ Việt Nam)
                    if (!shouldCancel && request.OldAvailabiliti != null)
                    {
                        var oldAvailability = request.OldAvailabiliti;
                        
                        // StartDate đã bao gồm slot rồi, dùng trực tiếp
                        var classStartDateTime = oldAvailability.StartDate;
                        
                        // Nếu thời gian hiện tại (giờ VN) >= (thời gian bắt đầu học - 1 giờ) thì cancel
                        var oneHourBeforeClass = classStartDateTime.AddHours(-1);
                        
                        if (vietnamNow >= oneHourBeforeClass)
                        {
                            shouldCancel = true;
                        }
                    }

                    if (shouldCancel)
                    {
                        using var dbTransaction = await _context.Database.BeginTransactionAsync();
                        try
                        {
                            // Update status thành Cancelled
                            request.Status = (int)ScheduleChangeRequestStatus.Cancelled;
                            request.ProcessedAt = DateTime.Now;

                            // Update NewAvailabiliti về Available
                            if (request.NewAvailabiliti != null)
                            {
                                var newAvailability = await _tutorAvailabilityRepository.GetByIdFullAsync(request.NewAvailabilitiId);
                                if (newAvailability != null)
                                {
                                    newAvailability.Status = (int)TutorAvailabilityStatus.Available;
                                    await _tutorAvailabilityRepository.UpdateAsync(newAvailability);
                                }
                            }

                            await _scheduleChangeRequestRepository.UpdateAsync(request);
                            await dbTransaction.CommitAsync();
                            cancelledCount++;
                        }
                        catch (Exception ex)
                        {
                            await dbTransaction.RollbackAsync();
                            // Log error nhưng tiếp tục với các request khác
                            Console.WriteLine($"[AutoCancelExpiredPendingRequests] Error cancelling request {request.Id}: {ex.Message}");
                        }
                    }
                }

                return cancelledCount;
            }
            catch (Exception ex)
            {
                throw new Exception($"Lỗi khi tự động hủy các ScheduleChangeRequest Pending: {ex.Message}", ex);
            }
        }
    }
}


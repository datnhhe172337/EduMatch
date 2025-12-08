using EduMatch.BusinessLogicLayer.DTOs;
using EduMatch.BusinessLogicLayer.Interfaces;
using EduMatch.BusinessLogicLayer.Requests;
using EduMatch.DataAccessLayer.Entities;
using EduMatch.DataAccessLayer.Enum;
using EduMatch.DataAccessLayer.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Org.BouncyCastle.Crypto.Engines.SM2Engine;

namespace EduMatch.BusinessLogicLayer.Services
{
    public class ClassRequestService : IClassRequestService
    {
        private readonly IClassRequestRepository _repo;
        private readonly EmailService _emailService;

        public ClassRequestService(IClassRequestRepository repo, EmailService emailService)
        {
            _repo = repo;
            _emailService = emailService;
        }

        public async Task ApproveOrRejectClassRequestAsync(int classRequestId, string businessAdEmail, IsApprovedClassRequestDto dto)
        {
            var request = await _repo.GetClassRequestByIdAsync(classRequestId);
            if (request == null)
                throw new KeyNotFoundException("Class request not found.");
            if (request.Status != ClassRequestStatus.Pending)
                throw new InvalidOperationException("Only pending requests can be approved or rejected.");

            request.ApprovedBy = businessAdEmail;
            request.ApprovedAt = DateTime.UtcNow;

            if (dto.IsApproved)
            {
                request.Status = ClassRequestStatus.Open;
                request.RejectionReason = null;
            }
            else
            {
                request.Status = ClassRequestStatus.Rejected;
                request.RejectionReason = dto.RejectionReason ?? "No reason provided.";
            }
            await _repo.UpdateStatusAsync(request);

            var subject = "";
            var body = $@"";

            if (dto.IsApproved)
            {
                subject = "🎉 Yêu cầu mở lớp của bạn đã được phê duyệt!";
                body = $@"
                <p>Xin chào <b>{request.LearnerEmail}</b>,</p>

                <p>Chúng tôi rất vui thông báo rằng yêu cầu mở lớp <b>“{request.Title}”</b> của bạn 
                đã được <b>phê duyệt</b> bởi đội ngũ quản lý.</p>

                <p>Chi tiết lớp học:</p>
                <ul>
                    <li><b>Môn học:</b> {request.Subject?.SubjectName}</li>
                    <li><b>Cấp độ:</b> {request.Level?.Name}</li>
                    <li><b>Hình thức học:</b> {request.Mode}</li>
                    <li><b>Ngày bắt đầu dự kiến:</b> {request.ExpectedStartDate:dd/MM/yyyy}</li>
                </ul>

                <p>Bạn sẽ sớm nhận được thông báo khi có gia sư phù hợp đăng ký tham gia lớp học của bạn.</p>

                <p>Trân trọng,<br/>
                <b>Đội ngũ EduMatch</b></p>
                ";
            }
            else
            {
                subject = "⚠️ Yêu cầu mở lớp của bạn đã bị từ chối";
                body = $@"
                <p>Xin chào <b>{request.LearnerEmail}</b>,</p>

                <p>Chúng tôi rất tiếc thông báo rằng yêu cầu mở lớp <b>“{request.Title}”</b> của bạn 
                <b>chưa được phê duyệt</b> tại thời điểm này.</p>

                <p><b>Lý do từ chối:</b> {request.RejectionReason}</p>

                <p>Vui lòng xem lại thông tin yêu cầu và chỉnh sửa nếu cần thiết. 
                Sau khi cập nhật, bạn có thể gửi lại để được xem xét lại.</p>

                <p>Cảm ơn bạn đã sử dụng nền tảng của chúng tôi.</p>

                <p>Trân trọng,<br/>
                <b>Đội ngũ EduMatch</b></p>
                ";
            }

            await _emailService.SendMailAsync(new MailContent
            {
                To = request.LearnerEmail,
                Subject = subject,
                Body = body
            });

        }

        public async Task CancelClassRequestAsync(int classRequestId, string learnerEmail, CancelClassRequestDto dto)
        {
            try
            {
                var classRequest = await _repo.GetClassRequestByIdAsync(classRequestId);
                if (classRequest == null)
                    throw new KeyNotFoundException("Class request not found.");

                if (!classRequest.LearnerEmail.Equals(learnerEmail, StringComparison.OrdinalIgnoreCase))
                    throw new Exception("You do not have permission to delete this request.");

                if (classRequest.Status != ClassRequestStatus.Open)
                    throw new InvalidOperationException("Only open requests can be cancelled.");

                classRequest.Status = ClassRequestStatus.Cancelled;
                classRequest.CancelReason = dto.Reason ?? "Cancelled by learner.";

                await _repo.UpdateStatusAsync(classRequest);
                
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<ClassRequest> CreateRequestToOpenClassAsync(ClassCreateRequest dto, string learnerEmail)
        {
            try
            {
                var request = new ClassRequest
                {
                    LearnerEmail = learnerEmail,
                    SubjectId = dto.SubjectId,
                    Title = dto.Title,
                    LevelId = dto.LevelId,
                    LearningGoal = dto.LearningGoal,
                    TutorRequirement = dto.TutorRequirement,
                    Mode = (TeachingMode)dto.Mode,
                    ProvinceId = dto.ProvinceId,
                    SubDistrictId = dto.SubDistrictId,
                    AddressLine = dto.AddressLine,
                    Latitude = dto.Latitude,
                    Longitude = dto.Longitude,
                    ExpectedStartDate = dto.ExpectedStartDate,
                    ExpectedSessions = dto.ExpectedSessions,
                    TargetUnitPriceMin = dto.TargetUnitPriceMin,
                    TargetUnitPriceMax = dto.TargetUnitPriceMax,
                    Status = ClassRequestStatus.Pending
                };

                var slots = dto.Slots?.Select(s => new ClassRequestSlotsAvailability
                {
                    DayOfWeek = s.DayOfWeek,
                    SlotId = s.SlotId
                }).ToList() ?? new List<ClassRequestSlotsAvailability>();

                return await _repo.CreateRequestToOpenClassAsync(request, slots);
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
        }

        public async Task<bool> DeleteClassRequestAsync(int classRequestId, string learnerEmail)
        {
            try
            {
                var classRequest = await _repo.GetClassRequestByIdAsync(classRequestId);
                if (classRequest == null)
                    throw new Exception("Request does not exist.");

                if (!classRequest.LearnerEmail.Equals(learnerEmail, StringComparison.OrdinalIgnoreCase))
                    throw new Exception("You do not have permission to delete this request.");

                await _repo.DeleteRequestToOpenClassAsync(classRequest);
                return true;
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
        }

        public async Task ExpireClassRequestsAsync()
        {
             await _repo.GetRequestsToExpireAsync();
        }

        public async Task<ClassRequestDetailDto?> GetDetailsClassRequestByIdAsync(int classRequestId)
        {
            var request = await _repo.GetClassRequestByIdAsync(classRequestId);
            if (request == null)
                return null;

            return new ClassRequestDetailDto
            {
                Id = request.Id,
                LearnerEmail = request.LearnerEmail,
                SubjectName = request.Subject.SubjectName,
                Title = request.Title,
                Level = request.Level.Name,
                LearningGoal = request.LearningGoal,
                TutorRequirement = request.TutorRequirement,
                Mode = request.Mode.ToString(),
                ProvinceName = request.Province?.Name,
                SubDistrictName = request.SubDistrict?.Name,
                AddressLine = request.AddressLine,
                Latitude = request.Latitude,
                Longitude = request.Longitude,
                ExpectedStartDate = request.ExpectedStartDate,
                ExpectedSessions = request.ExpectedSessions,
                TargetUnitPriceMin = request.TargetUnitPriceMin,
                TargetUnitPriceMax = request.TargetUnitPriceMax,
                Status = request.Status.ToString(),
                CreatedAt = request.CreatedAt,
                UpdatedAt = request.UpdatedAt,
                ApprovedAt = request.ApprovedAt,
                ApprovedBy = request.ApprovedBy,
                RejectionReason = request.RejectionReason,
                CancelReason = request.CancelReason,
                Slots = request.ClassRequestSlotsAvailabilities?.Select(s => new ClassRequestSlotDto
                {
                    Id = s.Id,
                    DayOfWeek = s.DayOfWeek,
                    StartTime = s.Slot.StartTime,
                    EndTime = s.Slot.EndTime
                }).ToList()
            };
        }

        public async Task<List<ClassRequestItemDto>> GetExpiredClassRequestsByLearnerEmail(string learnerEmail)
        {
            var result = await _repo.GetClassRequestsByLearnerEmailandStatusAsync(learnerEmail, ClassRequestStatus.Expired);

            return result.Select(r => new ClassRequestItemDto
            {
                Id = r.Id,
                LearnerEmail = r.LearnerEmail,
                AvatarUrl = r.LearnerEmailNavigation.UserProfile?.AvatarUrl,
                LearnerName = r.LearnerEmailNavigation.UserName,
                Title = r.Title,
                SubjectName = r.Subject.SubjectName,
                Level = r.Level.Name,
                //LearningGoal = r.LearningGoal,
                //TutorRequirement = r.TutorRequirement,
                Mode = r.Mode.ToString(),
                //ProvinceName = r.Province.Name,
                //SubDistrictName = r.SubDistrict.Name,
                //AddressLine = r.AddressLine,
                //Latitude = r.Latitude,
                //Longitude = r.Longitude,
                ExpectedStartDate = r.ExpectedStartDate,
                ExpectedSessions = r.ExpectedSessions,
                TargetUnitPriceMin = r.TargetUnitPriceMin,
                TargetUnitPriceMax = r.TargetUnitPriceMax,
                Status = r.Status.ToString(),
                CreatedAt = r.CreatedAt
            }).ToList();
        }

        public async Task<List<ClassRequestItemDto>> GetCanceledClassRequestsByLearnerEmail(string learnerEmail)
        {
            var result = await _repo.GetClassRequestsByLearnerEmailandStatusAsync(learnerEmail, ClassRequestStatus.Cancelled);

            return result.Select(r => new ClassRequestItemDto
            {
                Id = r.Id,
                LearnerEmail = r.LearnerEmail,
                AvatarUrl = r.LearnerEmailNavigation.UserProfile?.AvatarUrl,
                LearnerName = r.LearnerEmailNavigation.UserName,
                SubjectName = r.Subject.SubjectName,
                Title = r.Title,
                Level = r.Level.Name,
                //LearningGoal = r.LearningGoal,
                //TutorRequirement = r.TutorRequirement,
                Mode = r.Mode.ToString(),
                //ProvinceName = r.Province.Name,
                //SubDistrictName = r.SubDistrict.Name,
                //AddressLine = r.AddressLine,
                //Latitude = r.Latitude,
                //Longitude = r.Longitude,
                ExpectedStartDate = r.ExpectedStartDate,
                ExpectedSessions = r.ExpectedSessions,
                TargetUnitPriceMin = r.TargetUnitPriceMin,
                TargetUnitPriceMax = r.TargetUnitPriceMax,
                Status = r.Status.ToString(),
                CreatedAt = r.CreatedAt
            }).ToList();
        }

        public async Task<List<ClassRequestItemDto>> GetOpenClassRequestsAsync()
        {
            var result = await _repo.GetListOfAllOpenClassRequestsAsync();
            return result.Select(r => new ClassRequestItemDto
            {
                Id = r.Id,
                LearnerEmail = r.LearnerEmail,
                AvatarUrl = r.LearnerEmailNavigation.UserProfile?.AvatarUrl,
                LearnerName = r.LearnerEmailNavigation.UserName,
                SubjectName = r.Subject.SubjectName,
                Title = r.Title,
                Level = r.Level.Name,
                //LearningGoal = r.LearningGoal,
                //TutorRequirement = r.TutorRequirement,
                Mode = r.Mode.ToString(),
                //ProvinceName = r.Province.Name,
                //SubDistrictName = r.SubDistrict.Name,
                //AddressLine = r.AddressLine,
                //Latitude = r.Latitude,
                //Longitude = r.Longitude,
                ExpectedStartDate = r.ExpectedStartDate,
                ExpectedSessions = r.ExpectedSessions,
                TargetUnitPriceMin = r.TargetUnitPriceMin,
                TargetUnitPriceMax = r.TargetUnitPriceMax,
                Status = r.Status.ToString(),
                CreatedAt = r.CreatedAt
            }).ToList();
        }

        public async Task<List<ClassRequestItemDto>> GetListAllClassRequestsByLearnerEmail(string learnerEmail)
        {
            var result = await _repo.GetClassRequestsByLearnerEmailAsync(learnerEmail);

            return result.Select(r => new ClassRequestItemDto
            {
                Id = r.Id,
                LearnerEmail = r.LearnerEmail,
                AvatarUrl = r.LearnerEmailNavigation.UserProfile?.AvatarUrl,
                LearnerName = r.LearnerEmailNavigation.UserName,
                SubjectName = r.Subject.SubjectName,
                Title = r.Title,
                Level = r.Level.Name,
                //LearningGoal = r.LearningGoal,
                //TutorRequirement = r.TutorRequirement,
                Mode = r.Mode.ToString(),
                //ProvinceName = r.Province.Name,
                //SubDistrictName = r.SubDistrict.Name,
                //AddressLine = r.AddressLine,
                //Latitude = r.Latitude,
                //Longitude = r.Longitude,
                ExpectedStartDate = r.ExpectedStartDate,
                ExpectedSessions = r.ExpectedSessions,
                TargetUnitPriceMin = r.TargetUnitPriceMin,
                TargetUnitPriceMax = r.TargetUnitPriceMax,
                Status = r.Status.ToString(),
                CreatedAt = r.CreatedAt
            }).ToList();
        }

        public async Task<List<ClassRequestItemDto>> GetOpenClassRequestsByLearnerEmail(string learnerEmail)
        {
            var result = await _repo.GetClassRequestsByLearnerEmailandStatusAsync(learnerEmail, ClassRequestStatus.Open);

            return result.Select(r => new ClassRequestItemDto
            {
                Id = r.Id,
                LearnerEmail = r.LearnerEmail,
                AvatarUrl = r.LearnerEmailNavigation.UserProfile?.AvatarUrl,
                LearnerName = r.LearnerEmailNavigation.UserName,
                SubjectName = r.Subject.SubjectName,
                Title = r.Title,
                Level = r.Level.Name,
                //LearningGoal = r.LearningGoal,
                //TutorRequirement = r.TutorRequirement,
                Mode = r.Mode.ToString(),
                //ProvinceName = r.Province.Name,
                //SubDistrictName = r.SubDistrict.Name,
                //AddressLine = r.AddressLine,
                //Latitude = r.Latitude,
                //Longitude = r.Longitude,
                ExpectedStartDate = r.ExpectedStartDate,
                ExpectedSessions = r.ExpectedSessions,
                TargetUnitPriceMin = r.TargetUnitPriceMin,
                TargetUnitPriceMax = r.TargetUnitPriceMax,
                Status = r.Status.ToString(),
                CreatedAt = r.CreatedAt
            }).ToList();
        }

        public async Task<List<ClassRequestItemDto>> GetPendingClassRequestsAsync()
        {
            var result = await _repo.GetPendingClassRequestsAsync();

            return result.Select(r => new ClassRequestItemDto
            {
                Id = r.Id,
                LearnerEmail = r.LearnerEmail,
                AvatarUrl = r.LearnerEmailNavigation.UserProfile?.AvatarUrl,
                LearnerName = r.LearnerEmailNavigation.UserName,
                SubjectName = r.Subject.SubjectName,
                Title = r.Title,
                Level = r.Level.Name,
                //LearningGoal = r.LearningGoal,
                //TutorRequirement = r.TutorRequirement,
                Mode = r.Mode.ToString(),
                //ProvinceName = r.Province.Name,
                //SubDistrictName = r.SubDistrict.Name,
                //AddressLine = r.AddressLine,
                //Latitude = r.Latitude,
                //Longitude = r.Longitude,
                ExpectedStartDate = r.ExpectedStartDate,
                ExpectedSessions = r.ExpectedSessions,
                TargetUnitPriceMin = r.TargetUnitPriceMin,
                TargetUnitPriceMax = r.TargetUnitPriceMax,
                Status = r.Status.ToString(),
                CreatedAt = r.CreatedAt
            }).ToList();
        }

        public async Task<List<ClassRequestItemDto>> GetPendingClassRequestsByLearnerEmail(string learnerEmail)
        {
            var result = await _repo.GetClassRequestsByLearnerEmailandStatusAsync(learnerEmail, ClassRequestStatus.Pending);

            return result.Select(r => new ClassRequestItemDto
            {
                Id = r.Id,
                LearnerEmail = r.LearnerEmail,
                AvatarUrl = r.LearnerEmailNavigation.UserProfile?.AvatarUrl,
                LearnerName = r.LearnerEmailNavigation.UserName,
                SubjectName = r.Subject.SubjectName,
                Title = r.Title,
                Level = r.Level.Name,
                //LearningGoal = r.LearningGoal,
                //TutorRequirement = r.TutorRequirement,
                Mode = r.Mode.ToString(),
                //ProvinceName = r.Province.Name,
                //SubDistrictName = r.SubDistrict.Name,
                //AddressLine = r.AddressLine,
                //Latitude = r.Latitude,
                //Longitude = r.Longitude,
                ExpectedStartDate = r.ExpectedStartDate,
                ExpectedSessions = r.ExpectedSessions,
                TargetUnitPriceMin = r.TargetUnitPriceMin,
                TargetUnitPriceMax = r.TargetUnitPriceMax,
                Status = r.Status.ToString(),
                CreatedAt = r.CreatedAt
            }).ToList();
        }

        public async Task<List<ClassRequestItemDto>> GetRejectedClassRequestsByLearnerEmail(string learnerEmail)
        {
            var result = await _repo.GetClassRequestsByLearnerEmailandStatusAsync(learnerEmail, ClassRequestStatus.Rejected);

            return result.Select(r => new ClassRequestItemDto
            {
                Id = r.Id,
                LearnerEmail = r.LearnerEmail,
                AvatarUrl = r.LearnerEmailNavigation.UserProfile?.AvatarUrl,
                LearnerName = r.LearnerEmailNavigation.UserName,
                SubjectName = r.Subject.SubjectName,
                Title = r.Title,
                Level = r.Level.Name,
                //LearningGoal = r.LearningGoal,
                //TutorRequirement = r.TutorRequirement,
                Mode = r.Mode.ToString(),
                //ProvinceName = r.Province.Name,
                //SubDistrictName = r.SubDistrict.Name,
                //AddressLine = r.AddressLine,
                //Latitude = r.Latitude,
                //Longitude = r.Longitude,
                ExpectedStartDate = r.ExpectedStartDate,
                ExpectedSessions = r.ExpectedSessions,
                TargetUnitPriceMin = r.TargetUnitPriceMin,
                TargetUnitPriceMax = r.TargetUnitPriceMax,
                Status = r.Status.ToString(),
                CreatedAt = r.CreatedAt
            }).ToList();
        }

        public async Task<bool> UpdateClassRequestAsync(int classRequestId, UpdateClassRequest request, string learnerEmail)
        {
            var existing = await _repo.GetClassRequestByIdAsync(classRequestId);
            if (existing == null)
                throw new KeyNotFoundException("Class request not found");

            if (!string.Equals(existing.LearnerEmail, learnerEmail, StringComparison.OrdinalIgnoreCase))
                throw new UnauthorizedAccessException("You are not authorized to update this class request.");

            existing.SubjectId = request.SubjectId;
            existing.Title = request.Title ?? existing.Title;
            existing.LevelId = request.LevelId;
            existing.LearningGoal = request.LearningGoal ?? existing.LearningGoal;
            existing.TutorRequirement = request.TutorRequirement ?? existing.TutorRequirement;
            existing.Mode = (TeachingMode)request.Mode;
            existing.ProvinceId = request.ProvinceId;
            existing.SubDistrictId = request.SubDistrictId;
            existing.AddressLine = request.AddressLine;
            existing.Latitude = request.Latitude;
            existing.Longitude = request.Longitude;
            existing.ExpectedStartDate = request.ExpectedStartDate;
            existing.ExpectedSessions = request.ExpectedSessions;
            existing.TargetUnitPriceMin = request.TargetUnitPriceMin;
            existing.TargetUnitPriceMax = request.TargetUnitPriceMax;
            existing.Status = ClassRequestStatus.Pending;
            existing.UpdatedAt = DateTime.UtcNow;

            var newSlots = request.Slots?.Select(s => new ClassRequestSlotsAvailability
            {
                ClassRequestId = existing.Id,
                DayOfWeek = s.DayOfWeek,
                SlotId = s.SlotId
            }).ToList() ?? new List<ClassRequestSlotsAvailability>();

            await _repo.UpdateRequestToOpenClassAsync(existing, newSlots);

            return true;

        }
    }
}

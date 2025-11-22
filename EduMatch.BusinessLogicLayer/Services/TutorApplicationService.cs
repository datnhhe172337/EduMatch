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

namespace EduMatch.BusinessLogicLayer.Services
{
    public class TutorApplicationService : ITutorApplicationService
    {
        private readonly ITutorApplicationRepository _repo;
        private readonly EmailService _emailService;

        public TutorApplicationService(ITutorApplicationRepository repo, EmailService emailService)
        {
            _repo = repo;
            _emailService = emailService;
        }

        public async Task TutorApplyAsync(int classRequestId, string userEmail, string message)
        {
            var classRequest = await _repo.GetClassRequestByIdAsync(classRequestId)
                ?? throw new KeyNotFoundException("Class request not found.");

            if (classRequest.Status != ClassRequestStatus.Open)
                throw new InvalidOperationException("This class request is not accepting applications.");

            var tutor = await _repo.GetTutorByEmailAsync(userEmail);
            if (tutor == null)
                throw new Exception("Tutor not found or not yet approved as tutor");

            var alreadyApplied = await _repo.HasAppliedAsync(classRequestId, tutor.Id);

            if (alreadyApplied)
                throw new InvalidOperationException("You have already applied to this class request.");

            var application = new TutorApplication
            {
                ClassRequestId = classRequestId,
                TutorId = tutor.Id,
                Message = message,
                Status = 0, // default "Applied"
                CreatedAt = DateTime.UtcNow
            };

            await _repo.AddApplicationAsync(application);

            var subject = "Bạn có một gia sư mới đã ứng tuyển vào yêu cầu lớp học của bạn!";

            var body = $@"
<p>Xin chào <b>{classRequest.LearnerEmail}</b>,</p>

<p>Chúng tôi xin thông báo rằng đã có một gia sư quan tâm và ứng tuyển vào yêu cầu mở lớp của bạn với tiêu đề:</p>

<p><b>""{classRequest.Title}""</b></p>

<p>🎓 <b>Thông tin gia sư ứng tuyển:</b></p>
<ul>
  <li>Email gia sư: <b>{tutor.UserEmail}</b></li>
  <li>Tin nhắn từ gia sư:</li>
</ul>

<blockquote>{message}</blockquote>

<p>Vui lòng đăng nhập hệ thống để xem hồ sơ và lựa chọn gia sư phù hợp.</p>

<hr />

<p>Nếu bạn không yêu cầu dịch vụ này, vui lòng liên hệ với đội ngũ hỗ trợ ngay để được kiểm tra.</p>

<p>Trân trọng,<br>
<b>Đội ngũ hỗ trợ EduMatch</b></p>
";

            await _emailService.SendMailAsync(new MailContent
            {
                To = classRequest.LearnerEmail,
                Subject = subject,
                Body = body
            });

        }

        public async Task<IEnumerable<TutorApplicationItemDto>> GetTutorApplicationsByClassRequestAsync(int classRequestId, string learnerEmail)
        {
            var classRequest = await _repo.GetClassRequestByIdAsync(classRequestId);

            if (classRequest == null)
                throw new KeyNotFoundException("Class request not found.");

            //if (!classRequest.LearnerEmail.Equals(learnerEmail, StringComparison.OrdinalIgnoreCase))
            //    throw new UnauthorizedAccessException("You are not allowed to view applications for this class request.");

            var applications = await _repo.GetApplicationsByClassRequestAsync(classRequestId);

            return applications.Select(a => new TutorApplicationItemDto
            {
                ApplicationId = a.Id,
                TutorId = a.TutorId,
                TutorName = a.Tutor.UserEmailNavigation.UserName,
                AvatarUrl = a.Tutor.UserEmailNavigation.UserProfile?.AvatarUrl,
                Message = a.Message,
                AppliedAt = a.CreatedAt
            }).ToList();
        }

        public async Task<List<TutorAppliedItemDto?>> GetTutorApplicationsByTutorAsync(string tutorEmail)
        {
            var apps = await _repo.GetApplicationsByTutorAsync(tutorEmail);

            return apps.Select(a => new TutorAppliedItemDto
            {
                Id = a.Id,
                ClassRequestId = a.ClassRequestId,
                LearnerName = a.ClassRequest.LearnerEmailNavigation.UserName,
                AvatarUrl = a.ClassRequest.LearnerEmailNavigation.UserProfile?.AvatarUrl,
                Title = a.ClassRequest.Title,
                SubjectName = a.ClassRequest.Subject.SubjectName,
                Level = a.ClassRequest.Level.Name,
                Mode = a.ClassRequest.Mode.ToString(),
                ExpectedStartDate = a.ClassRequest.ExpectedStartDate,
                ExpectedSessions = a.ClassRequest.ExpectedSessions,
                TargetUnitPriceMin = a.ClassRequest.TargetUnitPriceMin,
                TargetUnitPriceMax = a.ClassRequest.TargetUnitPriceMax,
                Message = a.Message,
                ClassRequestStatus = a.ClassRequest.Status.ToString(),
                TutorApplicationStatus = a.Status, // 0 -> Đang apply
                AppliedAt = a.CreatedAt
            }).ToList();
        }

        public async Task<List<TutorAppliedItemDto?>> GetCanceledApplicationsByTutorAsync(string tutorEmail)
        {
            var apps = await _repo.GetCanceledApplicationsByTutorAsync(tutorEmail);

            return apps.Select(a => new TutorAppliedItemDto
            {
                Id = a.Id,
                ClassRequestId = a.ClassRequestId,
                LearnerName = a.ClassRequest.LearnerEmailNavigation.UserName,
                AvatarUrl = a.ClassRequest.LearnerEmailNavigation.UserProfile?.AvatarUrl,
                Title = a.ClassRequest.Title,
                SubjectName = a.ClassRequest.Subject.SubjectName,
                Level = a.ClassRequest.Level.Name,
                Mode = a.ClassRequest.Mode.ToString(),
                ExpectedStartDate = a.ClassRequest.ExpectedStartDate,
                ExpectedSessions = a.ClassRequest.ExpectedSessions,
                TargetUnitPriceMin = a.ClassRequest.TargetUnitPriceMin,
                TargetUnitPriceMax = a.ClassRequest.TargetUnitPriceMax,
                Message = a.Message,
                ClassRequestStatus = a.ClassRequest.Status.ToString(),
                TutorApplicationStatus = a.Status, // 1 -> Đã hủy apply
                AppliedAt = a.CreatedAt
            }).ToList();
        }

        public async Task EditTutorApplicationAsync(string tutorEmail, TutorApplicationEditRequest request)
        {
            var application = await _repo.GetApplicationByIdAsync(request.TutorApplicationId)
        ?? throw new KeyNotFoundException("Application not found");

            if (application.Tutor.UserEmail != tutorEmail)
                throw new UnauthorizedAccessException("You can only update your own application");

            if (application.Status != 0)
                throw new InvalidOperationException("You can only edit an application that is still currently applying");

            var classRequest = await _repo.GetClassRequestByIdAsync(application.ClassRequestId);

            if (classRequest == null)
                throw new KeyNotFoundException("Class request not found.");

            var oldMessage = application.Message;

            application.Message = request.Message;
            application.UpdatedAt = DateTime.UtcNow;

            await _repo.UpdateAsync(application);

            var subject = "Có một gia sư đã cập nhật yêu cầu ứng tuyển của họ!";

            var body = $@"
<p>Xin chào <b>{classRequest.LearnerEmail}</b>,</p>

<p>Chúng tôi xin thông báo rằng đã có một gia sư cập nhật ứng tuyển vào yêu cầu mở lớp của bạn với tiêu đề:</p>

<p><b>""{classRequest.Title}""</b></p>

<p>🎓 <b>Thông tin cập nhật:</b></p>
<ul>
  <li>Email gia sư: <b>{tutorEmail}</b></li>
</ul>

<ul>
<li>Tin nhắn cũ từ gia sư:</li>
<blockquote>{oldMessage}</blockquote>
</ul>

<ul>
<li>Tin nhắn đã được cập nhật từ gia sư:</li>
<blockquote>{application.Message}</blockquote>
</ul>

<p>Vui lòng đăng nhập hệ thống để xem chi tiết và lựa chọn gia sư phù hợp.</p>

<hr />

<p>Nếu bạn có vấn đề gì vui lòng liên hệ với đội ngũ hỗ trợ ngay để được kiểm tra.</p>

<p>Trân trọng,<br>
<b>Đội ngũ hỗ trợ EduMatch</b></p>
";

            await _emailService.SendMailAsync(new MailContent
            {
                To = classRequest.LearnerEmail,
                Subject = subject,
                Body = body
            });
        }

        public async Task CancelApplicationAsync(string tutorEmail, int tutorApplicationId)
        {
            var application = await _repo.GetApplicationByIdAsync(tutorApplicationId)
                ?? throw new KeyNotFoundException("Application not found");

            if (application.Tutor.UserEmail != tutorEmail)
                throw new UnauthorizedAccessException("You can only withdraw your own application");

            if (application.Status != 0)
                throw new InvalidOperationException("Only currently applying can be cancel");

            application.Status = 1;

            await _repo.UpdateAsync(application);

        }

    }
}

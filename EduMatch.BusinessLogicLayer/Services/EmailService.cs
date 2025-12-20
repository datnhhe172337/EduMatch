using EduMatch.BusinessLogicLayer.Settings;
using MailKit.Security;
using Microsoft.Extensions.Options;
using MimeKit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EduMatch.BusinessLogicLayer.Services
{
    public class MailContent
    {
        public string To { get; set; }

        public string Subject { get; set; }
        public string Body { get; set; }
    }

    public class EmailService
    {
        private readonly MailSettings _mailSettings;
        public EmailService(IOptions<MailSettings> mailSettings)
        {
            _mailSettings = mailSettings.Value;
        }

        public virtual async Task<string> SendMailAsync(MailContent mailContent)
        {
            var email = new MimeMessage();
            email.Sender = new MailboxAddress(_mailSettings.DisplayName, _mailSettings.Mail);
            email.From.Add(new MailboxAddress(_mailSettings.DisplayName, _mailSettings.Mail));

            email.To.Add(new MailboxAddress(mailContent.To, mailContent.To));
            email.Subject = mailContent.Subject;
            var builder = new BodyBuilder();
            builder.HtmlBody = mailContent.Body;
            email.Body = builder.ToMessageBody();

            using var smtp = new MailKit.Net.Smtp.SmtpClient();

            try
            {
                await smtp.ConnectAsync(_mailSettings.Host, _mailSettings.Port, SecureSocketOptions.StartTls);
                await smtp.AuthenticateAsync(_mailSettings.Mail, _mailSettings.Password);
                await smtp.SendAsync(email);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return "LOI " + ex.Message;
            }


            await smtp.DisconnectAsync(true);
            return "Gui Thanh cong";
        }



		//  Gửi mail chúc mừng trở thành Gia sư 
		public Task<string> SendBecomeTutorWelcomeAsync(string toEmail, string toName = null)
		{
			var recipient = string.IsNullOrWhiteSpace(toName) ? toEmail : toName;
			var html = BuildBecomeTutorHtml(recipient, _mailSettings.DisplayName ?? "EduMatch");

			return SendMailAsync(new MailContent
			{
				To = toEmail,
				Subject = "Chúc mừng! Bạn đã đăng ký trở thành Gia sư trên EduMatch",
				Body = html
			});
		}

        // html trở thành gia sư
		private static string BuildBecomeTutorHtml(string recipientName, string brand)
		{
			var today = DateTime.UtcNow.AddHours(7).ToString("dd/MM/yyyy HH:mm");

			return $@"
                    <!DOCTYPE html>
                    <html lang=""vi"">
                    <head>
                      <meta charset=""UTF-8"">
                      <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
                      <title>{brand} - Đăng ký Gia sư thành công</title>
                      <style>
                        body {{ margin:0; padding:0; background:#f7fafc; font-family:Segoe UI,Roboto,Helvetica,Arial,sans-serif; color:#1a202c; }}
                        .container {{ max-width:640px; margin:0 auto; padding:32px 20px; }}
                        .card {{ background:#ffffff; border-radius:16px; box-shadow:0 6px 18px rgba(0,0,0,0.06); overflow:hidden; }}
                        .header {{ background:linear-gradient(135deg,#4f46e5,#06b6d4); padding:28px 24px; color:#fff; }}
                        .brand {{ font-size:22px; font-weight:700; letter-spacing:.3px; }}
                        .content {{ padding:28px 24px; line-height:1.6; }}
                        h1 {{ font-size:20px; margin:0 0 10px; color:#111827; }}
                        p {{ margin:10px 0; }}
                        .pill {{ display:inline-block; padding:6px 10px; border-radius:999px; background:#eef2ff; color:#4338ca; font-size:12px; font-weight:600; }}
                        .muted {{ color:#6b7280; font-size:12px; }}
                        .footer {{ padding:16px 24px 24px; color:#6b7280; font-size:12px; border-top:1px solid #f3f4f6; }}
                      </style>
                    </head>
                    <body>
                      <div class=""container"">
                        <div class=""card"">
                          <div class=""header"">
                            <div class=""brand"">{brand}</div>
                            <div class=""muted"" style=""color:rgba(255,255,255,.9); margin-top:6px;"">Thông báo đăng ký Gia sư thành công — {today} (GMT+7)</div>
                          </div>
                          <div class=""content"">
                            <div class=""pill"">Chúc mừng</div>
                            <h1>Xin chào {recipientName},</h1>
                            <p>Cảm ơn bạn đã tin tưởng {brand}. Hồ sơ Gia sư của bạn đã được tạo thành công và đang <b>chờ phê duyệt</b>.</p>
                            <p>Chúng tôi sẽ sớm xem xét hồ sơ của bạn. Vui lòng kiểm tra email để nhận thông báo kết quả.</p>
                            <p class=""muted"">Chúc bạn một ngày tuyệt vời 🌟</p>
                          </div>
                          <div class=""footer"">
                            Email này được gửi từ hệ thống {brand}. Vui lòng không trả lời trực tiếp. 
                            Cần hỗ trợ? Liên hệ <a href=""support@edumatch.vn"">support@edumatch.vn</a>.
                            <br/>© {DateTime.UtcNow.Year} {brand}.
                          </div>
                        </div>
                      </div>
                    </body>
                    </html>";
		}

		// Gửi mail thông báo kết quả đơn đăng ký gia sư (đã duyệt hoặc bị từ chối)
		public Task<string> SendTutorApplicationResultAsync(string toEmail, bool isApproved, string toName = null, string rejectionReason = null)
		{
			var recipient = string.IsNullOrWhiteSpace(toName) ? toEmail : toName;
			var html = BuildTutorApplicationResultHtml(recipient, isApproved, rejectionReason, _mailSettings.DisplayName ?? "EduMatch");

			var subject = isApproved 
				? "Chúc mừng! Đơn đăng ký Gia sư của bạn đã được duyệt"
				: "Thông báo: Đơn đăng ký Gia sư của bạn đã bị từ chối";

			return SendMailAsync(new MailContent
			{
				To = toEmail,
				Subject = subject,
				Body = html
			});
		}

		// HTML thông báo kết quả đơn đăng ký gia sư
		private static string BuildTutorApplicationResultHtml(string recipientName, bool isApproved, string rejectionReason, string brand)
		{
			var today = DateTime.UtcNow.AddHours(7).ToString("dd/MM/yyyy HH:mm");
			var headerColor = isApproved ? "linear-gradient(135deg,#10b981,#059669)" : "linear-gradient(135deg,#ef4444,#dc2626)";
			var pillText = isApproved ? "Đã duyệt" : "Bị từ chối";
			var pillColor = isApproved ? "#d1fae5" : "#fee2e2";
			var pillTextColor = isApproved ? "#065f46" : "#991b1b";

			var content = isApproved
				? $@"
					<p>Chúc mừng bạn! Đơn đăng ký trở thành Gia sư của bạn đã được <b>phê duyệt thành công</b>.</p>
					<p>Bạn có thể bắt đầu nhận đơn đặt lịch học từ học viên ngay bây giờ. Chúc bạn có những trải nghiệm tuyệt vời trên {brand}!</p>
					<p class=""muted"">Chúc bạn một ngày tuyệt vời 🌟</p>"
				: $@"
					<p>Rất tiếc, đơn đăng ký trở thành Gia sư của bạn đã bị <b>từ chối</b>.</p>
					{(string.IsNullOrWhiteSpace(rejectionReason) 
						? "<p>Vui lòng kiểm tra lại hồ sơ và đăng ký lại sau.</p>"
						: $"<p><b>Lý do từ chối:</b> {rejectionReason}</p><p>Vui lòng cập nhật hồ sơ theo yêu cầu và đăng ký lại.</p>")}
					<p class=""muted"">Nếu bạn có thắc mắc, vui lòng liên hệ với chúng tôi.</p>";

			return $@"
                    <!DOCTYPE html>
                    <html lang=""vi"">
                    <head>
                      <meta charset=""UTF-8"">
                      <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
                      <title>{brand} - Kết quả đơn đăng ký Gia sư</title>
                      <style>
                        body {{ margin:0; padding:0; background:#f7fafc; font-family:Segoe UI,Roboto,Helvetica,Arial,sans-serif; color:#1a202c; }}
                        .container {{ max-width:640px; margin:0 auto; padding:32px 20px; }}
                        .card {{ background:#ffffff; border-radius:16px; box-shadow:0 6px 18px rgba(0,0,0,0.06); overflow:hidden; }}
                        .header {{ background:{headerColor}; padding:28px 24px; color:#fff; }}
                        .brand {{ font-size:22px; font-weight:700; letter-spacing:.3px; }}
                        .content {{ padding:28px 24px; line-height:1.6; }}
                        h1 {{ font-size:20px; margin:0 0 10px; color:#111827; }}
                        p {{ margin:10px 0; }}
                        .pill {{ display:inline-block; padding:6px 10px; border-radius:999px; background:{pillColor}; color:{pillTextColor}; font-size:12px; font-weight:600; }}
                        .muted {{ color:#6b7280; font-size:12px; }}
                        .footer {{ padding:16px 24px 24px; color:#6b7280; font-size:12px; border-top:1px solid #f3f4f6; }}
                        .reason-box {{ background:#fef2f2; border-left:4px solid #ef4444; padding:12px 16px; margin:16px 0; border-radius:4px; }}
                      </style>
                    </head>
                    <body>
                      <div class=""container"">
                        <div class=""card"">
                          <div class=""header"">
                            <div class=""brand"">{brand}</div>
                            <div class=""muted"" style=""color:rgba(255,255,255,.9); margin-top:6px;"">Thông báo kết quả đơn đăng ký — {today} (GMT+7)</div>
                          </div>
                          <div class=""content"">
                            <div class=""pill"">{pillText}</div>
                            <h1>Xin chào {recipientName},</h1>
                            {content}
                          </div>
                          <div class=""footer"">
                            Email này được gửi từ hệ thống {brand}. Vui lòng không trả lời trực tiếp. 
                            Cần hỗ trợ? Liên hệ <a href=""support@edumatch.vn"">support@edumatch.vn</a>.
                            <br/>© {DateTime.UtcNow.Year} {brand}.
                          </div>
                        </div>
                      </div>
                    </body>
                    </html>";
		}

		// Gửi mail thông báo đơn booking tạo thành công
		public Task<string> SendBookingCreatedNotificationAsync(string toEmail, string subjectName, string levelName, decimal totalAmount, string tutorEmail, string tutorName = null, string learnerName = null)
		{
			var recipient = string.IsNullOrWhiteSpace(learnerName) ? toEmail : learnerName;
			var tutorDisplayName = string.IsNullOrWhiteSpace(tutorName) ? tutorEmail : tutorName;
			var html = BuildBookingCreatedHtml(recipient, subjectName, levelName, totalAmount, tutorDisplayName, _mailSettings.DisplayName ?? "EduMatch");

			return SendMailAsync(new MailContent
			{
				To = toEmail,
				Subject = "Đơn đặt lịch học của bạn đã được tạo thành công",
				Body = html
			});
		}

		// HTML thông báo booking tạo thành công
		private static string BuildBookingCreatedHtml(string recipientName, string subjectName, string levelName, decimal totalAmount, string tutorName, string brand)
		{
			var today = DateTime.UtcNow.AddHours(7).ToString("dd/MM/yyyy HH:mm");
			var formattedAmount = totalAmount.ToString("N0").Replace(",", ".") + " VNĐ";

			return $@"
                    <!DOCTYPE html>
                    <html lang=""vi"">
                    <head>
                      <meta charset=""UTF-8"">
                      <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
                      <title>{brand} - Đơn đặt lịch học thành công</title>
                      <style>
                        body {{ margin:0; padding:0; background:#f7fafc; font-family:Segoe UI,Roboto,Helvetica,Arial,sans-serif; color:#1a202c; }}
                        .container {{ max-width:640px; margin:0 auto; padding:32px 20px; }}
                        .card {{ background:#ffffff; border-radius:16px; box-shadow:0 6px 18px rgba(0,0,0,0.06); overflow:hidden; }}
                        .header {{ background:linear-gradient(135deg,#10b981,#059669); padding:28px 24px; color:#fff; }}
                        .brand {{ font-size:22px; font-weight:700; letter-spacing:.3px; }}
                        .content {{ padding:28px 24px; line-height:1.6; }}
                        h1 {{ font-size:20px; margin:0 0 10px; color:#111827; }}
                        p {{ margin:10px 0; }}
                        .info-box {{ background:#f0fdf4; border-left:4px solid #10b981; padding:16px; margin:16px 0; border-radius:4px; }}
                        .info-row {{ display:flex; justify-content:space-between; margin:8px 0; }}
                        .info-label {{ color:#6b7280; font-weight:600; }}
                        .info-value {{ color:#111827; font-weight:500; }}
                        .amount {{ font-size:18px; color:#059669; font-weight:700; }}
                        .pill {{ display:inline-block; padding:6px 10px; border-radius:999px; background:#d1fae5; color:#065f46; font-size:12px; font-weight:600; }}
                        .muted {{ color:#6b7280; font-size:12px; }}
                        .footer {{ padding:16px 24px 24px; color:#6b7280; font-size:12px; border-top:1px solid #f3f4f6; }}
                      </style>
                    </head>
                    <body>
                      <div class=""container"">
                        <div class=""card"">
                          <div class=""header"">
                            <div class=""brand"">{brand}</div>
                            <div class=""muted"" style=""color:rgba(255,255,255,.9); margin-top:6px;"">Thông báo đơn đặt lịch học — {today} (GMT+7)</div>
                          </div>
                          <div class=""content"">
                            <div class=""pill"">Thành công</div>
                            <h1>Xin chào {recipientName},</h1>
                            <p>Đơn đặt lịch học của bạn đã được tạo thành công và đang chờ gia sư xác nhận.</p>
                            <div class=""info-box"">
                              <div class=""info-row"">
                                <span class=""info-label"">Môn học:</span>
                                <span class=""info-value"">{subjectName}</span>
                              </div>
                              <div class=""info-row"">
                                <span class=""info-label"">Level:</span>
                                <span class=""info-value"">{levelName}</span>
                              </div>
                              <div class=""info-row"">
                                <span class=""info-label"">Gia sư:</span>
                                <span class=""info-value"">{tutorName}</span>
                              </div>
                              <div class=""info-row"">
                                <span class=""info-label"">Tổng tiền:</span>
                                <span class=""info-value amount"">{formattedAmount}</span>
                              </div>
                            </div>
                            <p>Bạn sẽ nhận được thông báo khi gia sư xác nhận đơn đặt lịch học.</p>
                            <p class=""muted"">Chúc bạn một ngày tuyệt vời 🌟</p>
                          </div>
                          <div class=""footer"">
                            Email này được gửi từ hệ thống {brand}. Vui lòng không trả lời trực tiếp. 
                            Cần hỗ trợ? Liên hệ <a href=""support@edumatch.vn"">support@edumatch.vn</a>.
                            <br/>© {DateTime.UtcNow.Year} {brand}.
                          </div>
                        </div>
                      </div>
                    </body>
                    </html>";
		}

		// Gửi mail thông báo yêu cầu chuyển lịch được tạo
		public Task<string> SendScheduleChangeRequestCreatedAsync(
			string toEmail, 
			string requesterName, 
			string requestedToName,
			DateTime oldScheduleTime,
			DateTime newScheduleTime,
			string? reason = null,
			string recipientName = null)
		{
			var recipient = string.IsNullOrWhiteSpace(recipientName) ? toEmail : recipientName;
			var html = BuildScheduleChangeRequestCreatedHtml(
				recipient, 
				requesterName, 
				requestedToName,
				oldScheduleTime,
				newScheduleTime,
				reason,
				_mailSettings.DisplayName ?? "EduMatch");

			return SendMailAsync(new MailContent
			{
				To = toEmail,
				Subject = "Yêu cầu thay đổi lịch học mới",
				Body = html
			});
		}

		// HTML thông báo yêu cầu chuyển lịch được tạo
		private static string BuildScheduleChangeRequestCreatedHtml(
			string recipientName, 
			string requesterName, 
			string requestedToName,
			DateTime oldScheduleTime,
			DateTime newScheduleTime,
			string? reason,
			string brand)
		{
			var today = DateTime.UtcNow.AddHours(7).ToString("dd/MM/yyyy HH:mm");
			var oldTimeStr = oldScheduleTime.ToString("dd/MM/yyyy HH:mm");
			var newTimeStr = newScheduleTime.ToString("dd/MM/yyyy HH:mm");

			var reasonSection = !string.IsNullOrWhiteSpace(reason)
				? $@"
					<div class=""info-box"">
						<div class=""info-label"">Lý do yêu cầu:</div>
						<div class=""info-value"">{reason}</div>
					</div>"
				: "";

			return $@"
                    <!DOCTYPE html>
                    <html lang=""vi"">
                    <head>
                      <meta charset=""UTF-8"">
                      <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
                      <title>{brand} - Yêu cầu thay đổi lịch học</title>
                      <style>
                        body {{ margin:0; padding:0; background:#f7fafc; font-family:Segoe UI,Roboto,Helvetica,Arial,sans-serif; color:#1a202c; }}
                        .container {{ max-width:640px; margin:0 auto; padding:32px 20px; }}
                        .card {{ background:#ffffff; border-radius:16px; box-shadow:0 6px 18px rgba(0,0,0,0.06); overflow:hidden; }}
                        .header {{ background:linear-gradient(135deg,#f59e0b,#d97706); padding:28px 24px; color:#fff; }}
                        .brand {{ font-size:22px; font-weight:700; letter-spacing:.3px; }}
                        .content {{ padding:28px 24px; line-height:1.6; }}
                        h1 {{ font-size:20px; margin:0 0 10px; color:#111827; }}
                        p {{ margin:10px 0; }}
                        .info-box {{ background:#fffbeb; border-left:4px solid #f59e0b; padding:16px; margin:16px 0; border-radius:4px; }}
                        .info-row {{ display:flex; justify-content:space-between; margin:8px 0; }}
                        .info-label {{ color:#6b7280; font-weight:600; }}
                        .info-value {{ color:#111827; font-weight:500; }}
                        .time-comparison {{ display:flex; align-items:center; gap:16px; margin:16px 0; }}
                        .time-box {{ flex:1; padding:12px; background:#f3f4f6; border-radius:8px; text-align:center; }}
                        .time-label {{ font-size:12px; color:#6b7280; margin-bottom:4px; }}
                        .time-value {{ font-size:16px; font-weight:600; color:#111827; }}
                        .arrow {{ font-size:20px; color:#f59e0b; }}
                        .pill {{ display:inline-block; padding:6px 10px; border-radius:999px; background:#fef3c7; color:#92400e; font-size:12px; font-weight:600; }}
                        .muted {{ color:#6b7280; font-size:12px; }}
                        .footer {{ padding:16px 24px 24px; color:#6b7280; font-size:12px; border-top:1px solid #f3f4f6; }}
                      </style>
                    </head>
                    <body>
                      <div class=""container"">
                        <div class=""card"">
                          <div class=""header"">
                            <div class=""brand"">{brand}</div>
                            <div class=""muted"" style=""color:rgba(255,255,255,.9); margin-top:6px;"">Thông báo yêu cầu thay đổi lịch học — {today} (GMT+7)</div>
                          </div>
                          <div class=""content"">
                            <div class=""pill"">Yêu cầu mới</div>
                            <h1>Xin chào {recipientName},</h1>
                            <p><b>{requesterName}</b> đã gửi yêu cầu thay đổi lịch học cho bạn.</p>
                            <div class=""info-box"">
                              <div class=""time-comparison"">
                                <div class=""time-box"">
                                  <div class=""time-label"">Lịch cũ</div>
                                  <div class=""time-value"">{oldTimeStr}</div>
                                </div>
                                <div class=""arrow"">→</div>
                                <div class=""time-box"">
                                  <div class=""time-label"">Lịch mới</div>
                                  <div class=""time-value"">{newTimeStr}</div>
                                </div>
                              </div>
                              {reasonSection}
                            </div>
                            <p>Vui lòng đăng nhập hệ thống để xem xét và phản hồi yêu cầu này.</p>
                            <p class=""muted"">Chúc bạn một ngày tuyệt vời 🌟</p>
                          </div>
                          <div class=""footer"">
                            Email này được gửi từ hệ thống {brand}. Vui lòng không trả lời trực tiếp. 
                            Cần hỗ trợ? Liên hệ <a href=""support@edumatch.vn"">support@edumatch.vn</a>.
                            <br/>© {DateTime.UtcNow.Year} {brand}.
                          </div>
                        </div>
                      </div>
                    </body>
                    </html>";
		}

		// Gửi mail thông báo yêu cầu chuyển lịch được chấp nhận
		public Task<string> SendScheduleChangeRequestApprovedAsync(
			string toEmail,
			string requesterName,
			DateTime oldScheduleTime,
			DateTime newScheduleTime,
			string recipientName = null)
		{
			var recipient = string.IsNullOrWhiteSpace(recipientName) ? toEmail : recipientName;
			var html = BuildScheduleChangeRequestApprovedHtml(
				recipient,
				requesterName,
				oldScheduleTime,
				newScheduleTime,
				_mailSettings.DisplayName ?? "EduMatch");

			return SendMailAsync(new MailContent
			{
				To = toEmail,
				Subject = "Yêu cầu thay đổi lịch học của bạn đã được chấp nhận",
				Body = html
			});
		}

		// HTML thông báo yêu cầu chuyển lịch được chấp nhận
		private static string BuildScheduleChangeRequestApprovedHtml(
			string recipientName,
			string requesterName,
			DateTime oldScheduleTime,
			DateTime newScheduleTime,
			string brand)
		{
			var today = DateTime.UtcNow.AddHours(7).ToString("dd/MM/yyyy HH:mm");
			var oldTimeStr = oldScheduleTime.ToString("dd/MM/yyyy HH:mm");
			var newTimeStr = newScheduleTime.ToString("dd/MM/yyyy HH:mm");

			return $@"
                    <!DOCTYPE html>
                    <html lang=""vi"">
                    <head>
                      <meta charset=""UTF-8"">
                      <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
                      <title>{brand} - Yêu cầu thay đổi lịch học đã được chấp nhận</title>
                      <style>
                        body {{ margin:0; padding:0; background:#f7fafc; font-family:Segoe UI,Roboto,Helvetica,Arial,sans-serif; color:#1a202c; }}
                        .container {{ max-width:640px; margin:0 auto; padding:32px 20px; }}
                        .card {{ background:#ffffff; border-radius:16px; box-shadow:0 6px 18px rgba(0,0,0,0.06); overflow:hidden; }}
                        .header {{ background:linear-gradient(135deg,#10b981,#059669); padding:28px 24px; color:#fff; }}
                        .brand {{ font-size:22px; font-weight:700; letter-spacing:.3px; }}
                        .content {{ padding:28px 24px; line-height:1.6; }}
                        h1 {{ font-size:20px; margin:0 0 10px; color:#111827; }}
                        p {{ margin:10px 0; }}
                        .info-box {{ background:#f0fdf4; border-left:4px solid #10b981; padding:16px; margin:16px 0; border-radius:4px; }}
                        .time-comparison {{ display:flex; align-items:center; gap:16px; margin:16px 0; }}
                        .time-box {{ flex:1; padding:12px; background:#f3f4f6; border-radius:8px; text-align:center; }}
                        .time-label {{ font-size:12px; color:#6b7280; margin-bottom:4px; }}
                        .time-value {{ font-size:16px; font-weight:600; color:#111827; }}
                        .arrow {{ font-size:20px; color:#10b981; }}
                        .pill {{ display:inline-block; padding:6px 10px; border-radius:999px; background:#d1fae5; color:#065f46; font-size:12px; font-weight:600; }}
                        .muted {{ color:#6b7280; font-size:12px; }}
                        .footer {{ padding:16px 24px 24px; color:#6b7280; font-size:12px; border-top:1px solid #f3f4f6; }}
                      </style>
                    </head>
                    <body>
                      <div class=""container"">
                        <div class=""card"">
                          <div class=""header"">
                            <div class=""brand"">{brand}</div>
                            <div class=""muted"" style=""color:rgba(255,255,255,.9); margin-top:6px;"">Thông báo yêu cầu thay đổi lịch học — {today} (GMT+7)</div>
                          </div>
                          <div class=""content"">
                            <div class=""pill"">Đã chấp nhận</div>
                            <h1>Xin chào {recipientName},</h1>
                            <p>Yêu cầu thay đổi lịch học của bạn đã được <b>chấp nhận</b>.</p>
                            <div class=""info-box"">
                              <div class=""time-comparison"">
                                <div class=""time-box"">
                                  <div class=""time-label"">Lịch cũ</div>
                                  <div class=""time-value"">{oldTimeStr}</div>
                                </div>
                                <div class=""arrow"">→</div>
                                <div class=""time-box"">
                                  <div class=""time-label"">Lịch mới</div>
                                  <div class=""time-value"">{newTimeStr}</div>
                                </div>
                              </div>
                            </div>
                            <p>Lịch học của bạn đã được cập nhật thành công. Vui lòng kiểm tra lại lịch học mới trên hệ thống.</p>
                            <p class=""muted"">Chúc bạn một ngày tuyệt vời 🌟</p>
                          </div>
                          <div class=""footer"">
                            Email này được gửi từ hệ thống {brand}. Vui lòng không trả lời trực tiếp. 
                            Cần hỗ trợ? Liên hệ <a href=""support@edumatch.vn"">support@edumatch.vn</a>.
                            <br/>© {DateTime.UtcNow.Year} {brand}.
                          </div>
                        </div>
                      </div>
                    </body>
                    </html>";
		}

		// Gửi mail thông báo yêu cầu chuyển lịch bị từ chối
		public Task<string> SendScheduleChangeRequestRejectedAsync(
			string toEmail,
			string requesterName,
			DateTime oldScheduleTime,
			DateTime newScheduleTime,
			string? rejectionReason = null,
			string recipientName = null)
		{
			var recipient = string.IsNullOrWhiteSpace(recipientName) ? toEmail : recipientName;
			var html = BuildScheduleChangeRequestRejectedHtml(
				recipient,
				requesterName,
				oldScheduleTime,
				newScheduleTime,
				rejectionReason,
				_mailSettings.DisplayName ?? "EduMatch");

			return SendMailAsync(new MailContent
			{
				To = toEmail,
				Subject = "Yêu cầu thay đổi lịch học của bạn đã bị từ chối",
				Body = html
			});
		}

		// HTML thông báo yêu cầu chuyển lịch bị từ chối
		private static string BuildScheduleChangeRequestRejectedHtml(
			string recipientName,
			string requesterName,
			DateTime oldScheduleTime,
			DateTime newScheduleTime,
			string? rejectionReason,
			string brand)
		{
			var today = DateTime.UtcNow.AddHours(7).ToString("dd/MM/yyyy HH:mm");
			var oldTimeStr = oldScheduleTime.ToString("dd/MM/yyyy HH:mm");
			var newTimeStr = newScheduleTime.ToString("dd/MM/yyyy HH:mm");

			var reasonSection = !string.IsNullOrWhiteSpace(rejectionReason)
				? $@"
					<div class=""reason-box"">
						<div class=""info-label"">Lý do từ chối:</div>
						<div class=""info-value"">{rejectionReason}</div>
					</div>"
				: "";

			return $@"
                    <!DOCTYPE html>
                    <html lang=""vi"">
                    <head>
                      <meta charset=""UTF-8"">
                      <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
                      <title>{brand} - Yêu cầu thay đổi lịch học bị từ chối</title>
                      <style>
                        body {{ margin:0; padding:0; background:#f7fafc; font-family:Segoe UI,Roboto,Helvetica,Arial,sans-serif; color:#1a202c; }}
                        .container {{ max-width:640px; margin:0 auto; padding:32px 20px; }}
                        .card {{ background:#ffffff; border-radius:16px; box-shadow:0 6px 18px rgba(0,0,0,0.06); overflow:hidden; }}
                        .header {{ background:linear-gradient(135deg,#ef4444,#dc2626); padding:28px 24px; color:#fff; }}
                        .brand {{ font-size:22px; font-weight:700; letter-spacing:.3px; }}
                        .content {{ padding:28px 24px; line-height:1.6; }}
                        h1 {{ font-size:20px; margin:0 0 10px; color:#111827; }}
                        p {{ margin:10px 0; }}
                        .info-box {{ background:#fef2f2; border-left:4px solid #ef4444; padding:16px; margin:16px 0; border-radius:4px; }}
                        .reason-box {{ background:#fee2e2; padding:12px 16px; margin:12px 0; border-radius:4px; }}
                        .time-comparison {{ display:flex; align-items:center; gap:16px; margin:16px 0; }}
                        .time-box {{ flex:1; padding:12px; background:#f3f4f6; border-radius:8px; text-align:center; }}
                        .time-label {{ font-size:12px; color:#6b7280; margin-bottom:4px; }}
                        .time-value {{ font-size:16px; font-weight:600; color:#111827; }}
                        .arrow {{ font-size:20px; color:#ef4444; }}
                        .info-label {{ color:#6b7280; font-weight:600; font-size:12px; }}
                        .info-value {{ color:#111827; font-weight:500; margin-top:4px; }}
                        .pill {{ display:inline-block; padding:6px 10px; border-radius:999px; background:#fee2e2; color:#991b1b; font-size:12px; font-weight:600; }}
                        .muted {{ color:#6b7280; font-size:12px; }}
                        .footer {{ padding:16px 24px 24px; color:#6b7280; font-size:12px; border-top:1px solid #f3f4f6; }}
                      </style>
                    </head>
                    <body>
                      <div class=""container"">
                        <div class=""card"">
                          <div class=""header"">
                            <div class=""brand"">{brand}</div>
                            <div class=""muted"" style=""color:rgba(255,255,255,.9); margin-top:6px;"">Thông báo yêu cầu thay đổi lịch học — {today} (GMT+7)</div>
                          </div>
                          <div class=""content"">
                            <div class=""pill"">Bị từ chối</div>
                            <h1>Xin chào {recipientName},</h1>
                            <p>Rất tiếc, yêu cầu thay đổi lịch học của bạn đã bị <b>từ chối</b>.</p>
                            <div class=""info-box"">
                              <div class=""time-comparison"">
                                <div class=""time-box"">
                                  <div class=""time-label"">Lịch cũ</div>
                                  <div class=""time-value"">{oldTimeStr}</div>
                                </div>
                                <div class=""arrow"">→</div>
                                <div class=""time-box"">
                                  <div class=""time-label"">Lịch mới (yêu cầu)</div>
                                  <div class=""time-value"">{newTimeStr}</div>
                                </div>
                              </div>
                              {reasonSection}
                            </div>
                            <p>Lịch học của bạn vẫn giữ nguyên như ban đầu. Vui lòng kiểm tra lại lịch học trên hệ thống.</p>
                            <p class=""muted"">Nếu bạn có thắc mắc, vui lòng liên hệ với chúng tôi.</p>
                          </div>
                          <div class=""footer"">
                            Email này được gửi từ hệ thống {brand}. Vui lòng không trả lời trực tiếp. 
                            Cần hỗ trợ? Liên hệ <a href=""support@edumatch.vn"">support@edumatch.vn</a>.
                            <br/>© {DateTime.UtcNow.Year} {brand}.
                          </div>
                        </div>
                      </div>
                    </body>
                    </html>";
		}

	}
  
}

    
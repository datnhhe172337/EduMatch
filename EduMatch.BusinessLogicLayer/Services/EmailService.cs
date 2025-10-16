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

        public async Task<string> SendMailAsync(MailContent mailContent)
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
				Subject = "🎉 Chúc mừng! Bạn đã đăng ký trở thành Gia sư trên EduMatch",
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
                            Cần hỗ trợ? Liên hệ <a href=""mailto:support@edumatch.vn"">support@edumatch.vn</a>.
                            <br/>© {DateTime.UtcNow.Year} {brand}.
                          </div>
                        </div>
                      </div>
                    </body>
                    </html>";
		}





	}
}

    
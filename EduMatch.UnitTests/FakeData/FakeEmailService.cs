using EduMatch.BusinessLogicLayer.Services;
using EduMatch.BusinessLogicLayer.Settings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EduMatch.UnitTests.FakeData
{
    public class FakeEmailService : EmailService
    {
        public FakeEmailService() : base(Microsoft.Extensions.Options.Options.Create(new MailSettings())) { }

        public override async Task<string> SendMailAsync(MailContent mailContent)
        {
            // Không gửi mail thật, chỉ trả về thành công giả
            return await Task.FromResult("FAKE_OK");
        }
    }

}

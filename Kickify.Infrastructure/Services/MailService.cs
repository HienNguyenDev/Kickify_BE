using Kickify.Application.Abstractions.Services;
using Kickify.Infrastructure.Mail;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;

namespace Kickify.Infrastructure.Services
{
    public class MailService : IMailService
    {
        private readonly EmailSettings _options;
        private readonly SmtpClient _smtp;
        private readonly EmailTemplateService _emailTemplateService;

        public MailService(IOptions<EmailSettings> options, EmailTemplateService emailTemplateService)
        {
            _options = options.Value;
            _emailTemplateService = emailTemplateService;
            _smtp = new SmtpClient
            {
                Host = _options.SmtpHost,
                Port = _options.SmtpPort,
                Credentials = new NetworkCredential(_options.User, _options.Password),
                EnableSsl = _options.EnableSsl
            };
        }

        public async Task SendOtpAsync(string toEmail, string otp)
        {
            var htmlBody = await _emailTemplateService.RenderOtpEmailAsync(otp);

            var msg = new MailMessage(_options.From, toEmail)
            {
                Subject = "[Kickify] Xác thực tài khoản",
                Body = htmlBody,
                IsBodyHtml = true
            };

            await _smtp.SendMailAsync(msg);
        }
    }
}

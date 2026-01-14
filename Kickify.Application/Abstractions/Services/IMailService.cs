using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kickify.Application.Abstractions.Services
{
    public interface IMailService
    {
        Task SendOtpAsync(string toEmail, string otp);
        Task SendResetPasswordAsync(string toEmail, string newPassword);
    }
}

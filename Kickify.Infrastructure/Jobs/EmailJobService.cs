using Hangfire;
using Kickify.Application.Abstractions.Jobs;
using Kickify.Application.Abstractions.Services;

namespace Kickify.Infrastructure.Jobs;

public class EmailJobService : IEmailJobService
{
    private readonly IBackgroundJobClient _backgroundJobClient;

    public EmailJobService(IBackgroundJobClient backgroundJobClient)
    {
        _backgroundJobClient = backgroundJobClient;
    }

    public void EnqueueSendOtpEmail(string toEmail, string otp)
    {
        _backgroundJobClient.Enqueue<IMailService>(mail => mail.SendOtpAsync(toEmail, otp));
    }

    public void EnqueueSendResetPasswordEmail(string toEmail, string newPassword)
    {
        _backgroundJobClient.Enqueue<IMailService>(mail => mail.SendResetPasswordAsync(toEmail, newPassword));
    }
}

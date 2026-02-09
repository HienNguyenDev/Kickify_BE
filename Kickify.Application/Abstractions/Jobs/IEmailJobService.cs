namespace Kickify.Application.Abstractions.Jobs;
public interface IEmailJobService
{
    void EnqueueSendOtpEmail(string toEmail, string otp);
    void EnqueueSendResetPasswordEmail(string toEmail, string newPassword);
}

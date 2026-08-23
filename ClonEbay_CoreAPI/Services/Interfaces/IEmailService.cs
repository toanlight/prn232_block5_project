namespace ClonEbay_CoreAPI.Services.Interfaces
{
    public interface IEmailService
    {
        Task<bool> SendVerificationEmailAsync(string toEmail, string username, string otpCode, string? verificationLink = null);
        Task<bool> SendPasswordResetEmailAsync(string toEmail, string username, string resetToken);
    }
}

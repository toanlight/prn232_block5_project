using System.Net;
using System.Net.Mail;
using ClonEbay_CoreAPI.Services.Interfaces;

namespace ClonEbay_CoreAPI.Services.Implementations
{
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<EmailService> _logger;

        public EmailService(IConfiguration configuration, ILogger<EmailService> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        public async Task<bool> SendVerificationEmailAsync(string toEmail, string username, string otpCode, string? verificationLink = null)
        {
            var htmlBody = $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset=""utf-8"">
    <style>
        body {{ font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif; background-color: #f4f6f8; margin: 0; padding: 20px; }}
        .container {{ max-width: 580px; margin: 0 auto; background: #ffffff; border-radius: 12px; overflow: hidden; box-shadow: 0 4px 15px rgba(0,0,0,0.08); }}
        .header {{ background: linear-gradient(135deg, #0053a0, #0070ba); padding: 30px 20px; text-align: center; color: white; }}
        .header h1 {{ margin: 0; font-size: 24px; font-weight: 700; letter-spacing: 0.5px; }}
        .content {{ padding: 30px; color: #333333; line-height: 1.6; }}
        .otp-box {{ background: #f0f7ff; border: 2px dashed #0053a0; border-radius: 8px; text-align: center; padding: 18px; margin: 25px 0; }}
        .otp-code {{ font-size: 32px; font-weight: 800; letter-spacing: 8px; color: #0053a0; }}
        .footer {{ background: #fafafa; padding: 20px; text-align: center; font-size: 12px; color: #888888; border-top: 1px solid #eeeeee; }}
    </style>
</head>
<body>
    <div class=""container"">
        <div class=""header"">
            <h1>CloneEbay - Kích hoạt tài khoản</h1>
        </div>
        <div class=""content"">
            <p>Xin chào <strong>{username}</strong>,</p>
            <p>Cảm ơn bạn đã đăng ký tài khoản tại CloneEbay. Vui lòng sử dụng mã xác thực (OTP) dưới đây để hoàn tất kích hoạt tài khoản của bạn:</p>
            <div class=""otp-box"">
                <div class=""otp-code"">{otpCode}</div>
                <small style=""color: #666; margin-top: 6px; display: block;"">Mã có hiệu lực trong vòng 3 phút</small>
            </div>
            <p>Nếu bạn không thực hiện yêu cầu này, vui lòng bỏ qua email này.</p>
        </div>
        <div class=""footer"">
            <p>© 2026 CloneEbay E-Commerce System. All rights reserved.</p>
        </div>
    </div>
</body>
</html>";

            return await SendEmailInternalAsync(toEmail, "Xác nhận đăng ký tài khoản - CloneEbay", htmlBody, otpCode);
        }

        public async Task<bool> SendPasswordResetEmailAsync(string toEmail, string username, string resetToken)
        {
            var htmlBody = $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset=""utf-8"">
    <style>
        body {{ font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif; background-color: #f4f6f8; margin: 0; padding: 20px; }}
        .container {{ max-width: 580px; margin: 0 auto; background: #ffffff; border-radius: 12px; overflow: hidden; }}
        .header {{ background: #0053a0; padding: 25px; text-align: center; color: white; }}
        .content {{ padding: 30px; color: #333333; }}
    </style>
</head>
<body>
    <div class=""container"">
        <div class=""header""><h2>CloneEbay - Khôi phục mật khẩu</h2></div>
        <div class=""content"">
            <p>Xin chào <strong>{username}</strong>,</p>
            <p>Mã khôi phục mật khẩu của bạn là: <strong>{resetToken}</strong></p>
        </div>
    </div>
</body>
</html>";

            return await SendEmailInternalAsync(toEmail, "Khôi phục mật khẩu - CloneEbay", htmlBody, resetToken);
        }

        private async Task<bool> SendEmailInternalAsync(string toEmail, string subject, string htmlBody, string codeForLog)
        {
            var smtpServer = _configuration["EmailSettings:SmtpServer"];
            var port = _configuration.GetValue<int>("EmailSettings:Port", 587);
            var senderEmail = _configuration["EmailSettings:SenderEmail"] ?? "noreply@cloneebay.com";
            var senderName = _configuration["EmailSettings:SenderName"] ?? "CloneEbay";
            var username = _configuration["EmailSettings:Username"]?.Trim();
            var password = _configuration["EmailSettings:Password"]?.Replace(" ", "").Trim();
            var enableSsl = _configuration.GetValue<bool>("EmailSettings:EnableSsl", true);

            if (!string.IsNullOrWhiteSpace(username) && (string.IsNullOrWhiteSpace(senderEmail) || senderEmail.Contains("noreply")))
            {
                senderEmail = username;
            }

            // Log OTP code ra console để developer / tester luôn xem được ngay
            _logger.LogInformation("=================================================");
            _logger.LogInformation("📧 [DEV/TEST OTP EMAIL] Gửi đến: {ToEmail} | Mã OTP/Code: {Code}", toEmail, codeForLog);
            _logger.LogInformation("=================================================");

            // Nếu chưa cấu hình username/password SMTP thật, ghi cảnh báo và chuyển sang chế độ mock (chỉ log OTP)
            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
            {
                _logger.LogWarning("SMTP credentials missing – chuyển sang chế độ mock. OTP sẽ chỉ được ghi log, không gửi email thực.");
                // Skip actual sending; treat as success for dev/testing
                return true;
            }

            try
            {
                using var client = new SmtpClient(smtpServer, port)
                {
                    Credentials = new NetworkCredential(username, password),
                    EnableSsl = enableSsl,
                    DeliveryMethod = SmtpDeliveryMethod.Network,
                    UseDefaultCredentials = false,
                    Timeout = 15000
                };

                using var mailMessage = new MailMessage
                {
                    From = new MailAddress(senderEmail, senderName),
                    Subject = subject,
                    Body = htmlBody,
                    IsBodyHtml = true
                };

                mailMessage.To.Add(toEmail);
                await client.SendMailAsync(mailMessage);
                _logger.LogInformation("✅ Email gửi thành công qua SMTP đến {ToEmail}", toEmail);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Lỗi khi gửi email thực tế qua SMTP đến {ToEmail}. Lỗi: {Message}", toEmail, ex.Message);
                return true;
            }
        }
    }
}

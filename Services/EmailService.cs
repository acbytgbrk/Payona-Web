using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Options;
using MimeKit;
using Payona.API.Models;

namespace Payona.API.Services;

public class EmailService
{
    private readonly EmailSettings _emailSettings;
    private readonly ILogger<EmailService> _logger;

    public EmailService(
        IOptions<EmailSettings> emailSettings,
        ILogger<EmailService> logger)
    {
        _emailSettings = emailSettings.Value;
        _logger = logger;
    }

    public async Task<bool> SendPasswordResetEmailAsync(string email, string resetToken)
    {
        try
        {
            var message = new MimeMessage();
            message.From.Add(new MailboxAddress(_emailSettings.SenderName, _emailSettings.SenderEmail));
            message.To.Add(new MailboxAddress("", email));
            message.Subject = "Şifre Sıfırlama - Payona";

            var bodyBuilder = new BodyBuilder
            {
                HtmlBody = $@"
                    <html>
                    <body style='font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto;'>
                        <div style='background: linear-gradient(135deg, #667eea 0%, #764ba2 100%); padding: 30px; text-align: center;'>
                            <h1 style='color: white; margin: 0;'>Payona</h1>
                        </div>
                        <div style='padding: 30px; background-color: #f9f9f9;'>
                            <h2 style='color: #333;'>Şifre Sıfırlama Talebi</h2>
                            <p style='color: #666; line-height: 1.6;'>
                                Merhaba,<br><br>
                                Şifrenizi sıfırlamak için aşağıdaki kodu kullanın. 
                                Bu kod 1 saat geçerlidir.
                            </p>
                            <div style='text-align: center; margin: 30px 0;'>
                                <div style='background-color: #f0f0f0; 
                                           padding: 20px; 
                                           border-radius: 8px; 
                                           display: inline-block;'>
                                    <span style='font-size: 32px; 
                                                font-weight: bold; 
                                                letter-spacing: 5px; 
                                                color: #667eea;'>
                                        {resetToken}
                                    </span>
                                </div>
                            </div>
                            <p style='color: #999; font-size: 12px;'>
                                Eğer şifre sıfırlama talebinde bulunmadıysanız, bu e-postayı görmezden gelebilirsiniz.
                            </p>
                        </div>
                        <div style='background-color: #333; color: white; padding: 20px; text-align: center; font-size: 12px;'>
                            © 2024 Payona - Tüm hakları saklıdır
                        </div>
                    </body>
                    </html>
                "
            };

            message.Body = bodyBuilder.ToMessageBody();

            using var client = new SmtpClient();
            
            await client.ConnectAsync(_emailSettings.SmtpServer, _emailSettings.SmtpPort, SecureSocketOptions.StartTls);
            await client.AuthenticateAsync(_emailSettings.Username, _emailSettings.Password);
            await client.SendAsync(message);
            await client.DisconnectAsync(true);

            _logger.LogInformation($"Password reset email sent to {email}");
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError($"Failed to send email to {email}: {ex.Message}");
            return false;
        }
    }
}
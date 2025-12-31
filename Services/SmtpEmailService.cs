using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Configuration;

namespace Application.Services;

public class SmtpEmailService : IEmailService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<SmtpEmailService> _logger;

    public SmtpEmailService(IConfiguration configuration, ILogger<SmtpEmailService> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

    public async Task<bool> SendOtpEmailAsync(string email, string otpCode)
    {
        try
        {
            var smtpHost = _configuration["Smtp:Host"] ?? "smtp.gmail.com";
            var smtpPort = int.Parse(_configuration["Smtp:Port"] ?? "587");
            var smtpUsername = _configuration["Smtp:Username"] ?? string.Empty;
            var smtpPassword = _configuration["Smtp:Password"] ?? string.Empty;
            var fromEmail = _configuration["Smtp:FromEmail"] ?? smtpUsername;
            var fromName = _configuration["Smtp:FromName"] ?? "Grab Simulator";

            using var client = new SmtpClient(smtpHost, smtpPort)
            {
                EnableSsl = true,
                Credentials = new NetworkCredential(smtpUsername, smtpPassword)
            };

            var mailMessage = new MailMessage
            {
                From = new MailAddress(fromEmail, fromName),
                Subject = "Your OTP Code - Grab Simulator",
                Body = $"Your OTP code is: {otpCode}\n\nThis code will expire in 5 minutes.\n\nIf you didn't request this code, please ignore this email.",
                IsBodyHtml = false
            };

            mailMessage.To.Add(email);

            await client.SendMailAsync(mailMessage);
            _logger.LogInformation("OTP email sent successfully to {Email}", email);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send OTP email to {Email}", email);
            return false;
        }
    }
}


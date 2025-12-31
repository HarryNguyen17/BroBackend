using Resend;

namespace Application.Services;

public class ResendEmailService : IEmailService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<ResendEmailService> _logger;

    public ResendEmailService(IConfiguration configuration, ILogger<ResendEmailService> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

    public async Task<bool> SendOtpEmailAsync(string email, string otpCode)
    {
        try
        {
            var apiKey = _configuration["Resend:ApiKey"] ?? throw new InvalidOperationException("Resend API Key is not configured");
            var fromEmail = _configuration["Resend:FromEmail"] ?? "onboarding@resend.dev";
            var fromName = _configuration["Resend:FromName"] ?? "Grab Simulator";

            var client = new ResendClient(apiKey);

            var message = new EmailMessage
            {
                From = $"{fromName} <{fromEmail}>",
                To = email,
                Subject = "Your OTP Code - Grab Simulator",
                TextBody = $"Your OTP code is: {otpCode}\n\nThis code will expire in 5 minutes.\n\nIf you didn't request this code, please ignore this email."
            };

            var response = await client.EmailSendAsync(message);
            
            _logger.LogInformation("OTP email sent successfully to {Email} via Resend", email);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send OTP email to {Email} via Resend", email);
            return false;
        }
    }
}


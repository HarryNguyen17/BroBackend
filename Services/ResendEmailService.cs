using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace Application.Services;

public class ResendEmailService : IEmailService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<ResendEmailService> _logger;
    private readonly HttpClient _httpClient;

    public ResendEmailService(IConfiguration configuration, ILogger<ResendEmailService> logger)
    {
        _configuration = configuration;
        _logger = logger;
        _httpClient = new HttpClient();
    }

    public async Task<bool> SendOtpEmailAsync(string email, string otpCode)
    {
        try
        {
            var apiKey = _configuration["Resend:ApiKey"] ?? throw new InvalidOperationException("Resend API Key is not configured");
            var fromEmail = _configuration["Resend:FromEmail"] ?? "onboarding@resend.dev";
            var fromName = _configuration["Resend:FromName"] ?? "Grab Simulator";

            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);

            var payload = new
            {
                from = $"{fromName} <{fromEmail}>",
                to = new[] { email },
                subject = "Your OTP Code - Grab Simulator",
                text = $"Your OTP code is: {otpCode}\n\nThis code will expire in 5 minutes.\n\nIf you didn't request this code, please ignore this email."
            };

            var json = JsonSerializer.Serialize(payload);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync("https://api.resend.com/emails", content);

            if (response.IsSuccessStatusCode)
            {
                _logger.LogInformation("OTP email sent successfully to {Email} via Resend", email);
                return true;
            }
            else
            {
                var errorBody = await response.Content.ReadAsStringAsync();
                _logger.LogError("Resend API error: {StatusCode} - {Error}", response.StatusCode, errorBody);
                return false;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send OTP email to {Email} via Resend", email);
            return false;
        }
    }
}


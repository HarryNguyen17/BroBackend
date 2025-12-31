using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace Application.Services;

public class BrevoEmailService : IEmailService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<BrevoEmailService> _logger;
    private readonly HttpClient _httpClient;

    public BrevoEmailService(IConfiguration configuration, ILogger<BrevoEmailService> logger)
    {
        _configuration = configuration;
        _logger = logger;
        _httpClient = new HttpClient();
    }

    public async Task<bool> SendOtpEmailAsync(string email, string otpCode)
    {
        try
        {
            var apiKey = _configuration["Brevo:ApiKey"] ?? throw new InvalidOperationException("Brevo API Key is not configured");
            var senderEmail = _configuration["Brevo:SenderEmail"] ?? throw new InvalidOperationException("Brevo Sender Email is not configured");
            var senderName = _configuration["Brevo:SenderName"] ?? "Grab Simulator";

            _httpClient.DefaultRequestHeaders.Clear();
            _httpClient.DefaultRequestHeaders.Add("api-key", apiKey);
            _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            var payload = new
            {
                sender = new { name = senderName, email = senderEmail },
                to = new[] { new { email = email } },
                subject = "Your OTP Code - Grab Simulator",
                textContent = $"Your OTP code is: {otpCode}\n\nThis code will expire in 5 minutes.\n\nIf you didn't request this code, please ignore this email."
            };

            var json = JsonSerializer.Serialize(payload);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync("https://api.brevo.com/v3/smtp/email", content);

            if (response.IsSuccessStatusCode)
            {
                _logger.LogInformation("OTP email sent successfully to {Email} via Brevo", email);
                return true;
            }
            else
            {
                var errorBody = await response.Content.ReadAsStringAsync();
                _logger.LogError("Brevo API error: {StatusCode} - {Error}", response.StatusCode, errorBody);
                return false;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send OTP email to {Email} via Brevo", email);
            return false;
        }
    }
}


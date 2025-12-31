namespace Application.Models.DTOs;

public class RequestOtpRequest
{
    public string Email { get; set; } = string.Empty;
}

public class RequestOtpResponse
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
}

public class VerifyOtpRequest
{
    public string Email { get; set; } = string.Empty;
    public string Otp { get; set; } = string.Empty;
}

public class VerifyOtpResponse
{
    public bool Success { get; set; }
    public string? Token { get; set; }
    public string Message { get; set; } = string.Empty;
}


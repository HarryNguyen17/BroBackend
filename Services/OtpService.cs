using Application.Data;
using Application.Models;
using Microsoft.EntityFrameworkCore;

namespace Application.Services;

public class OtpService : IOtpService
{
    private readonly AppDbContext _context;
    private readonly ILogger<OtpService> _logger;
    private const int OtpExpiryMinutes = 5;

    public OtpService(AppDbContext context, ILogger<OtpService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<string> GenerateOtpAsync(string email)
    {
        // Generate 6-digit OTP
        var random = new Random();
        var otpCode = random.Next(100000, 999999).ToString();

        // Mark old OTPs as used
        var oldOtps = await _context.OtpCodes
            .Where(o => o.Email == email && !o.IsUsed && o.ExpiresAt > DateTime.UtcNow)
            .ToListAsync();

        foreach (var oldOtp in oldOtps)
        {
            oldOtp.IsUsed = true;
        }

        // Create new OTP
        var otp = new OtpCode
        {
            Email = email,
            Code = otpCode,
            ExpiresAt = DateTime.UtcNow.AddMinutes(OtpExpiryMinutes),
            IsUsed = false
        };

        _context.OtpCodes.Add(otp);
        await _context.SaveChangesAsync();

        _logger.LogInformation("OTP generated for {Email}", email);
        return otpCode;
    }

    public async Task<bool> ValidateOtpAsync(string email, string otp)
    {
        var otpCode = await _context.OtpCodes
            .Where(o => o.Email == email && o.Code == otp && !o.IsUsed && o.ExpiresAt > DateTime.UtcNow)
            .FirstOrDefaultAsync();

        if (otpCode == null)
        {
            _logger.LogWarning("Invalid or expired OTP attempt for {Email}", email);
            return false;
        }

        // Mark OTP as used
        otpCode.IsUsed = true;
        await _context.SaveChangesAsync();

        _logger.LogInformation("OTP validated successfully for {Email}", email);
        return true;
    }
}


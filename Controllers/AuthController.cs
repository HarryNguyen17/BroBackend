using Application.Data;
using Application.Models;
using Application.Models.DTOs;
using Application.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace Application.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IOtpService _otpService;
    private readonly IEmailService _emailService;
    private readonly IAuthService _authService;
    private readonly AppDbContext _context;
    private readonly ILogger<AuthController> _logger;

    public AuthController(
        IOtpService otpService,
        IEmailService emailService,
        IAuthService authService,
        AppDbContext context,
        ILogger<AuthController> logger)
    {
        _otpService = otpService;
        _emailService = emailService;
        _authService = authService;
        _context = context;
        _logger = logger;
    }

    [HttpPost("request-otp")]
    public async Task<ActionResult<RequestOtpResponse>> RequestOtp([FromBody] RequestOtpRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Email) || !IsValidEmail(request.Email))
        {
            return BadRequest(new RequestOtpResponse
            {
                Success = false,
                Message = "Invalid email address"
            });
        }

        try
        {
            var otpCode = await _otpService.GenerateOtpAsync(request.Email);
            var emailSent = await _emailService.SendOtpEmailAsync(request.Email, otpCode);

            if (!emailSent)
            {
                return StatusCode(500, new RequestOtpResponse
                {
                    Success = false,
                    Message = "Failed to send OTP email. Please try again later."
                });
            }

            return Ok(new RequestOtpResponse
            {
                Success = true,
                Message = "OTP sent successfully to your email"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error requesting OTP for {Email}", request.Email);
            return StatusCode(500, new RequestOtpResponse
            {
                Success = false,
                Message = "An error occurred while processing your request"
            });
        }
    }

    [HttpPost("verify-otp")]
    public async Task<ActionResult<VerifyOtpResponse>> VerifyOtp([FromBody] VerifyOtpRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Otp))
        {
            return BadRequest(new VerifyOtpResponse
            {
                Success = false,
                Message = "Email and OTP are required"
            });
        }

        try
        {
            var isValid = await _otpService.ValidateOtpAsync(request.Email, request.Otp);
            if (!isValid)
            {
                return Unauthorized(new VerifyOtpResponse
                {
                    Success = false,
                    Message = "Invalid or expired OTP"
                });
            }

            // Get or create user
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Email == request.Email);

            if (user == null)
            {
                user = new User
                {
                    Email = request.Email,
                    Coins = 0,
                    CreatedAt = DateTime.UtcNow,
                    LastLoginAt = DateTime.UtcNow
                };
                _context.Users.Add(user);
                await _context.SaveChangesAsync();
            }
            else
            {
                user.LastLoginAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();
            }

            var token = _authService.GenerateToken(user.Id, user.Email);

            return Ok(new VerifyOtpResponse
            {
                Success = true,
                Token = token,
                Message = "Login successful"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error verifying OTP for {Email}", request.Email);
            return StatusCode(500, new VerifyOtpResponse
            {
                Success = false,
                Message = "An error occurred while processing your request"
            });
        }
    }

    private static bool IsValidEmail(string email)
    {
        try
        {
            var addr = new System.Net.Mail.MailAddress(email);
            return addr.Address == email;
        }
        catch
        {
            return false;
        }
    }
}


using Application.Data;
using Application.Models.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace Application.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class CoinController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly ILogger<CoinController> _logger;

    public CoinController(AppDbContext context, ILogger<CoinController> logger)
    {
        _context = context;
        _logger = logger;
    }

    [HttpGet]
    public async Task<ActionResult<CoinResponse>> GetCoins()
    {
        try
        {
            var userId = GetUserId();
            if (userId == null)
            {
                return Unauthorized();
            }

            var user = await _context.Users.FindAsync(userId.Value);
            if (user == null)
            {
                return NotFound(new CoinResponse { Coins = 0 });
            }

            return Ok(new CoinResponse { Coins = user.Coins });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting coins for user");
            return StatusCode(500, new CoinResponse { Coins = 0 });
        }
    }

    [HttpPut]
    public async Task<ActionResult<UpdateCoinResponse>> UpdateCoins([FromBody] UpdateCoinRequest request)
    {
        try
        {
            var userId = GetUserId();
            if (userId == null)
            {
                return Unauthorized();
            }

            if (request.Coins < 0)
            {
                return BadRequest(new UpdateCoinResponse
                {
                    Success = false,
                    Coins = 0,
                    Message = "Coins cannot be negative"
                });
            }

            var user = await _context.Users.FindAsync(userId.Value);
            if (user == null)
            {
                return NotFound(new UpdateCoinResponse
                {
                    Success = false,
                    Coins = 0,
                    Message = "User not found"
                });
            }

            user.Coins = request.Coins;
            await _context.SaveChangesAsync();

            return Ok(new UpdateCoinResponse
            {
                Success = true,
                Coins = user.Coins,
                Message = "Coins updated successfully"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating coins for user");
            return StatusCode(500, new UpdateCoinResponse
            {
                Success = false,
                Coins = 0,
                Message = "An error occurred while updating coins"
            });
        }
    }

    private int? GetUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (int.TryParse(userIdClaim, out var userId))
        {
            return userId;
        }
        return null;
    }
}


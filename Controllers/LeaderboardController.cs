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
public class LeaderboardController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly ILogger<LeaderboardController> _logger;
    private const int DefaultTopCount = 100;
    private const int MaxTopCount = 1000;

    public LeaderboardController(AppDbContext context, ILogger<LeaderboardController> logger)
    {
        _context = context;
        _logger = logger;
    }

    [HttpGet]
    public async Task<ActionResult<LeaderboardResponse>> GetLeaderboard([FromQuery] int top = DefaultTopCount)
    {
        try
        {
            // Limit the top count to prevent abuse
            if (top <= 0 || top > MaxTopCount)
            {
                top = DefaultTopCount;
            }

            // value = TotalShipmentDelivered * TotalIncome
            var entries = await _context.Users
                .OrderByDescending(u => (long)u.TotalShipmentDelivered * u.TotalIncome)
                .ThenBy(u => u.CreatedAt) // Tie-breaker for same value
                .Take(top)
                .Select(u => new LeaderboardEntry
                {
                    Email = u.Email,
                    TotalShipmentDelivered = u.TotalShipmentDelivered,
                    TotalIncome = u.TotalIncome,
                    Value = (long)u.TotalShipmentDelivered * u.TotalIncome
                })
                .ToListAsync();

            // Assign ranks
            for (int i = 0; i < entries.Count; i++)
            {
                entries[i].Rank = i + 1;
            }

            var totalCount = await _context.Users.CountAsync();

            return Ok(new LeaderboardResponse
            {
                Entries = entries,
                TotalCount = totalCount
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting leaderboard");
            return StatusCode(500, new LeaderboardResponse
            {
                Entries = new List<LeaderboardEntry>(),
                TotalCount = 0
            });
        }
    }
}


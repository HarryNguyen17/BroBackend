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

            var entries = await _context.Users
                .OrderByDescending(u => u.Coins)
                .ThenBy(u => u.CreatedAt) // Tie-breaker for same coin count
                .Take(top)
                .Select(u => new LeaderboardEntry
                {
                    Email = u.Email,
                    Coins = u.Coins
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


using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FaithFlow.Infrastructure.Data;
using FaithFlow.Application.Common.Interfaces;

namespace FaithFlow.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class DashboardController : ControllerBase
{
    private readonly FaithFlowDbContext _context;

    public DashboardController(FaithFlowDbContext context)
    {
        _context = context;
    }

    [HttpGet("feed")]
    public async Task<IActionResult> GetDashboardFeed()
    {
        var recentActivity = await _context.PrayerRequests
            .Include(p => p.User)
            .OrderByDescending(p => p.CreatedAt)
            .Take(5)
            .Select(p => new { type = "prayer", title = p.Title, user = p.User.FirstName + " " + p.User.LastName, time = p.CreatedAt })
            .ToListAsync();

        var upcomingEvents = await _context.VolunteerOpportunities
            .Where(v => v.Date >= DateTime.UtcNow)
            .OrderBy(v => v.Date)
            .Take(5)
            .Select(v => new { title = v.Title, date = v.Date, location = v.Location })
            .ToListAsync();

        return Ok(new { recentActivity, upcomingEvents });
    }
}

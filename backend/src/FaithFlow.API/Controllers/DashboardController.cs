using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using FaithFlow.Application.Common.Interfaces;

namespace FaithFlow.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class DashboardController : ControllerBase
{
    private readonly IPrayerRequestService _prayerService;
    private readonly IVolunteerService _volunteerService;

    public DashboardController(IPrayerRequestService prayerService, IVolunteerService volunteerService)
    {
        _prayerService = prayerService;
        _volunteerService = volunteerService;
    }

    [HttpGet("feed")]
    public async Task<IActionResult> GetDashboardFeed()
    {
        var recentActivity = (await _prayerService.GetPublicRequestsAsync())
            .Take(5)
            .Select(p => new { type = "prayer", title = p.Title, user = p.UserName ?? "Anonymous", time = p.CreatedAt });

        var upcomingEvents = (await _volunteerService.GetOpportunitiesAsync(null, null, null))
            .Where(v => v.Date >= DateTime.UtcNow)
            .OrderBy(v => v.Date)
            .Take(5)
            .Select(v => new { title = v.Title, date = v.Date, location = v.Location });

        return Ok(new { recentActivity, upcomingEvents });
    }
}

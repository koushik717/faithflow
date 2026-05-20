using System.Security.Claims;
using FaithFlow.Application.Common.DTOs.Groups;
using FaithFlow.Application.Common.DTOs.Volunteer;
using FaithFlow.Application.Common.DTOs.SermonSeries;
using FaithFlow.Application.Common.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FaithFlow.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class GroupsController : ControllerBase
{
    private readonly IGroupService _groupService;
    public GroupsController(IGroupService groupService) => _groupService = groupService;
    private Guid UserId => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] string? q, [FromQuery] string? category)
        => Ok(await _groupService.GetAllGroupsAsync(UserId, q, category));

    [HttpPost]
    [Authorize(Policy = "RequireGroupLeader")]
    public async Task<IActionResult> Create([FromBody] CreateGroupRequest request)
        => Ok(await _groupService.CreateAsync(UserId, request));

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(Guid id) => Ok(await _groupService.GetByIdAsync(id, UserId));

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateGroupRequest request)
        => Ok(await _groupService.UpdateAsync(UserId, id, request));

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(Guid id) { await _groupService.DeleteAsync(UserId, id); return NoContent(); }

    [HttpPost("{id}/join")]
    public async Task<IActionResult> Join(Guid id) { await _groupService.JoinAsync(UserId, id); return Ok(new { message = "Joined group" }); }

    [HttpDelete("{id}/leave")]
    public async Task<IActionResult> Leave(Guid id) { await _groupService.LeaveAsync(UserId, id); return Ok(new { message = "Left group" }); }

    [HttpGet("{id}/members")]
    public async Task<IActionResult> GetMembers(Guid id) => Ok(await _groupService.GetMembersAsync(id));

    [HttpPost("{id}/attendance")]
    public async Task<IActionResult> RecordAttendance(Guid id, [FromBody] RecordAttendanceRequest request)
        => Ok(await _groupService.RecordAttendanceAsync(UserId, id, request));

    [HttpGet("{id}/attendance")]
    public async Task<IActionResult> GetAttendance(Guid id) => Ok(await _groupService.GetAttendanceAsync(id));
}

[ApiController]
[Route("api/volunteer")]
[Authorize]
public class VolunteerController : ControllerBase
{
    private readonly IVolunteerService _volunteerService;
    public VolunteerController(IVolunteerService volunteerService) => _volunteerService = volunteerService;
    private Guid UserId => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    [HttpGet("opportunities")]
    public async Task<IActionResult> GetOpportunities([FromQuery] string? category, [FromQuery] DateTime? date)
        => Ok(await _volunteerService.GetOpportunitiesAsync(UserId, category, date));

    [HttpPost("opportunities")]
    [Authorize(Policy = "RequireAdmin")]
    public async Task<IActionResult> CreateOpportunity([FromBody] CreateOpportunityRequest request)
        => Ok(await _volunteerService.CreateOpportunityAsync(request));

    [HttpPut("opportunities/{id}")]
    [Authorize(Policy = "RequireAdmin")]
    public async Task<IActionResult> UpdateOpportunity(Guid id, [FromBody] CreateOpportunityRequest request)
        => Ok(await _volunteerService.UpdateOpportunityAsync(id, request));

    [HttpDelete("opportunities/{id}")]
    [Authorize(Policy = "RequireAdmin")]
    public async Task<IActionResult> DeleteOpportunity(Guid id) { await _volunteerService.DeleteOpportunityAsync(id); return NoContent(); }

    [HttpPost("opportunities/{id}/signup")]
    public async Task<IActionResult> Signup(Guid id) { await _volunteerService.SignupAsync(UserId, id); return Ok(new { message = "Signed up" }); }

    [HttpDelete("opportunities/{id}/cancel")]
    public async Task<IActionResult> Cancel(Guid id) { await _volunteerService.CancelSignupAsync(UserId, id); return Ok(new { message = "Cancelled" }); }

    [HttpGet("my-signups")]
    public async Task<IActionResult> GetMySignups() => Ok(await _volunteerService.GetMySignupsAsync(UserId));

    [HttpGet("coverage-report")]
    [Authorize(Policy = "RequireAdmin")]
    public async Task<IActionResult> GetCoverageReport() => Ok(await _volunteerService.GetCoverageReportAsync());
}

[ApiController]
[Route("api/sermon-series")]
[Authorize]
public class SermonSeriesController : ControllerBase
{
    private readonly ISermonSeriesService _seriesService;
    public SermonSeriesController(ISermonSeriesService seriesService) => _seriesService = seriesService;

    [HttpGet]
    public async Task<IActionResult> GetAll() => Ok(await _seriesService.GetAllAsync());

    [HttpPost]
    [Authorize(Policy = "RequireAdmin")]
    public async Task<IActionResult> Create([FromBody] CreateSermonSeriesRequest request)
        => Ok(await _seriesService.CreateAsync(request));

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(Guid id) => Ok(await _seriesService.GetByIdAsync(id));

    [HttpPut("{id}")]
    [Authorize(Policy = "RequireAdmin")]
    public async Task<IActionResult> Update(Guid id, [FromBody] CreateSermonSeriesRequest request)
        => Ok(await _seriesService.UpdateAsync(id, request));

    [HttpPost("{id}/weeks")]
    [Authorize(Policy = "RequireAdmin")]
    public async Task<IActionResult> CreateWeek(Guid id, [FromBody] CreateSermonWeekRequest request)
        => Ok(await _seriesService.CreateWeekAsync(id, request));

    [HttpGet("{id}/weeks")]
    public async Task<IActionResult> GetWeeks(Guid id) => Ok(await _seriesService.GetWeeksAsync(id));

    [HttpPost("{id}/weeks/{weekId}/discussion-guide")]
    public async Task<IActionResult> GenerateDiscussionGuide(Guid id, Guid weekId)
        => Ok(await _seriesService.GenerateDiscussionGuideAsync(id, weekId));
}

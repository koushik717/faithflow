using System.Security.Claims;
using FaithFlow.Application.Common.DTOs.SermonNotes;
using FaithFlow.Application.Common.DTOs.PrayerRequests;
using FaithFlow.Application.Common.Interfaces;
using FaithFlow.API.Hubs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;

namespace FaithFlow.API.Controllers;

[ApiController]
[Route("api/sermon-notes")]
[Authorize]
public class SermonNotesController : ControllerBase
{
    private readonly ISermonNoteService _noteService;
    public SermonNotesController(ISermonNoteService noteService) => _noteService = noteService;
    private Guid UserId => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    /// <summary>Get all sermon notes for current user.</summary>
    [HttpGet]
    public async Task<IActionResult> GetMyNotes() => Ok(await _noteService.GetMyNotesAsync(UserId));

    /// <summary>Create a new sermon note.</summary>
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateSermonNoteRequest request)
        => Ok(await _noteService.CreateAsync(UserId, request));

    /// <summary>Get a specific sermon note.</summary>
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(Guid id) => Ok(await _noteService.GetByIdAsync(UserId, id));

    /// <summary>Update a sermon note.</summary>
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateSermonNoteRequest request)
        => Ok(await _noteService.UpdateAsync(UserId, id, request));

    /// <summary>Delete a sermon note.</summary>
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(Guid id) { await _noteService.DeleteAsync(UserId, id); return NoContent(); }

    /// <summary>Search sermon notes.</summary>
    [HttpGet("search")]
    public async Task<IActionResult> Search([FromQuery] string q) => Ok(await _noteService.SearchAsync(UserId, q ?? ""));

    /// <summary>Look up scripture for a sermon note.</summary>
    [HttpPost("{id}/scripture-lookup")]
    public async Task<IActionResult> ScriptureLookup(Guid id)
        => Ok(await _noteService.ScriptureLookupAsync(id, UserId));
}

[ApiController]
[Route("api/prayer-requests")]
[Authorize]
public class PrayerRequestsController : ControllerBase
{
    private readonly IPrayerRequestService _prayerService;
    private readonly IHubContext<PrayerHub> _hubContext;

    public PrayerRequestsController(IPrayerRequestService prayerService, IHubContext<PrayerHub> hubContext)
    {
        _prayerService = prayerService; _hubContext = hubContext;
    }

    private Guid UserId => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    /// <summary>Get all public prayer requests.</summary>
    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] string? filter)
    {
        return filter switch
        {
            "mine" => Ok(await _prayerService.GetMyRequestsAsync(UserId)),
            "answered" => Ok(await _prayerService.GetAnsweredRequestsAsync()),
            _ => Ok(await _prayerService.GetPublicRequestsAsync())
        };
    }

    /// <summary>Create a new prayer request.</summary>
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreatePrayerRequestRequest request)
    {
        var result = await _prayerService.CreateAsync(UserId, request);
        await _hubContext.Clients.Group("PrayerWall").SendAsync("OnNewPrayerRequest", result);
        return Ok(result);
    }

    /// <summary>Update a prayer request.</summary>
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdatePrayerRequestRequest request)
        => Ok(await _prayerService.UpdateAsync(UserId, id, request));

    /// <summary>Delete a prayer request.</summary>
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(Guid id) { await _prayerService.DeleteAsync(UserId, id); return NoContent(); }

    /// <summary>Pray for a request — increments count and broadcasts via SignalR.</summary>
    [HttpPost("{id}/pray")]
    public async Task<IActionResult> Pray(Guid id)
    {
        var newCount = await _prayerService.PrayAsync(UserId, id);
        await _hubContext.Clients.Group("PrayerWall").SendAsync("OnPrayCountUpdated", id, newCount);
        return Ok(new { prayCount = newCount });
    }

    /// <summary>Get AI-generated prayer response (Gemini).</summary>
    [HttpPost("{id}/ai-prayer")]
    public async Task<IActionResult> AiPrayer(Guid id) => Ok(await _prayerService.GetAiPrayerAsync(id));

    /// <summary>Mark a prayer request as answered.</summary>
    [HttpPut("{id}/answered")]
    public async Task<IActionResult> MarkAnswered(Guid id)
    {
        await _prayerService.MarkAnsweredAsync(UserId, id);
        await _hubContext.Clients.Group("PrayerWall").SendAsync("OnRequestAnswered", id);
        return Ok(new { message = "Prayer marked as answered" });
    }
}

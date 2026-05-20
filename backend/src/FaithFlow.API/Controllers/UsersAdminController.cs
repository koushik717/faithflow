using System.Security.Claims;
using FaithFlow.Application.Common.DTOs.Auth;
using FaithFlow.Application.Common.DTOs.Users;
using FaithFlow.Application.Common.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FaithFlow.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class UsersController : ControllerBase
{
    private readonly IUserService _userService;
    public UsersController(IUserService userService) => _userService = userService;

    private Guid UserId => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    /// <summary>Get current user's profile.</summary>
    [HttpGet("profile")]
    public async Task<ActionResult<UserDto>> GetProfile() => Ok(await _userService.GetProfileAsync(UserId));

    /// <summary>Update current user's profile.</summary>
    [HttpPut("profile")]
    public async Task<ActionResult<UserDto>> UpdateProfile([FromBody] UpdateProfileRequest request)
        => Ok(await _userService.UpdateProfileAsync(UserId, request));
}

[ApiController]
[Route("api/admin")]
[Authorize(Policy = "RequireAdmin")]
public class AdminController : ControllerBase
{
    private readonly IUserService _userService;
    private readonly IAdminService _adminService;

    public AdminController(IUserService userService, IAdminService adminService)
    {
        _userService = userService; _adminService = adminService;
    }

    /// <summary>Get all users (admin only).</summary>
    [HttpGet("users")]
    public async Task<IActionResult> GetUsers([FromQuery] string? q, [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
        => Ok(await _userService.GetAllUsersAsync(q, page, pageSize));

    /// <summary>Change a user's role (admin only).</summary>
    [HttpPut("users/{id}/role")]
    public async Task<IActionResult> ChangeRole(Guid id, [FromBody] ChangeRoleRequest request)
        => Ok(await _userService.ChangeRoleAsync(id, request.Role));

    /// <summary>Get dashboard statistics.</summary>
    [HttpGet("stats")]
    public async Task<IActionResult> GetStats() => Ok(await _adminService.GetStatsAsync());

    /// <summary>Get searchable, paginated member list.</summary>
    [HttpGet("members")]
    public async Task<IActionResult> GetMembers([FromQuery] string? q, [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
        => Ok(await _userService.GetAllUsersAsync(q, page, pageSize));

    /// <summary>Get group analytics.</summary>
    [HttpGet("groups/analytics")]
    public async Task<IActionResult> GetGroupAnalytics() => Ok(await _adminService.GetGroupAnalyticsAsync());

    /// <summary>Get volunteer coverage report.</summary>
    [HttpGet("volunteer/report")]
    public async Task<IActionResult> GetVolunteerReport([FromServices] IVolunteerService volunteerService)
        => Ok(await volunteerService.GetCoverageReportAsync());
}

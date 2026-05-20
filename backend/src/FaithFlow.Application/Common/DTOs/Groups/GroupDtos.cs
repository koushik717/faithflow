using System.ComponentModel.DataAnnotations;

namespace FaithFlow.Application.Common.DTOs.Groups;

public class GroupDto
{
    public Guid Id { get; set; }
    public Guid LeaderId { get; set; }
    public string LeaderName { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string Schedule { get; set; } = string.Empty;
    public int MaxMembers { get; set; }
    public int MemberCount { get; set; }
    public string? MeetingLocation { get; set; }
    public bool IsOnline { get; set; }
    public string? ImageUrl { get; set; }
    public DateTime CreatedAt { get; set; }
    public bool IsMember { get; set; }
}

public class CreateGroupRequest
{
    [Required] public string Name { get; set; } = string.Empty;
    [Required] public string Description { get; set; } = string.Empty;
    [Required] public string Category { get; set; } = string.Empty;
    public string Schedule { get; set; } = string.Empty;
    public int MaxMembers { get; set; } = 20;
    public string? MeetingLocation { get; set; }
    public bool IsOnline { get; set; }
    public string? ImageUrl { get; set; }
}

public class UpdateGroupRequest
{
    public string? Name { get; set; }
    public string? Description { get; set; }
    public string? Category { get; set; }
    public string? Schedule { get; set; }
    public int? MaxMembers { get; set; }
    public string? MeetingLocation { get; set; }
    public bool? IsOnline { get; set; }
}

public class GroupMemberDto
{
    public Guid UserId { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public DateTime JoinedAt { get; set; }
}

public class AttendanceDto
{
    public Guid Id { get; set; }
    public DateTime Date { get; set; }
    public int AttendeeCount { get; set; }
    public string? Notes { get; set; }
}

public class RecordAttendanceRequest
{
    public DateTime Date { get; set; } = DateTime.UtcNow;
    [Required] public int AttendeeCount { get; set; }
    public string? Notes { get; set; }
}

using System.ComponentModel.DataAnnotations;

namespace FaithFlow.Application.Common.DTOs.PrayerRequests;

public class PrayerRequestDto
{
    public Guid Id { get; set; }
    public Guid? UserId { get; set; }
    public string? UserName { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public bool IsAnonymous { get; set; }
    public bool IsAnswered { get; set; }
    public int PrayCount { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? AnsweredAt { get; set; }
}

public class CreatePrayerRequestRequest
{
    [Required] public string Title { get; set; } = string.Empty;
    [Required] public string Description { get; set; } = string.Empty;
    public bool IsAnonymous { get; set; }
    public bool IsPublic { get; set; } = true;
}

public class UpdatePrayerRequestRequest
{
    public string? Title { get; set; }
    public string? Description { get; set; }
    public bool? IsPublic { get; set; }
}

public class AiPrayerResponse
{
    public string Prayer { get; set; } = string.Empty;
}

using System.ComponentModel.DataAnnotations;

namespace FaithFlow.Application.Common.DTOs.SermonSeries;

public class SermonSeriesDto
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public int TotalWeeks { get; set; }
    public string? ImageUrl { get; set; }
    public int WeekCount { get; set; }
}

public class CreateSermonSeriesRequest
{
    [Required] public string Title { get; set; } = string.Empty;
    [Required] public string Description { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public int TotalWeeks { get; set; }
    public string? ImageUrl { get; set; }
}

public class SermonWeekDto
{
    public Guid Id { get; set; }
    public int WeekNumber { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Scripture { get; set; } = string.Empty;
    public string Summary { get; set; } = string.Empty;
    public string? VideoUrl { get; set; }
    public DiscussionGuideDto? DiscussionGuide { get; set; }
}

public class CreateSermonWeekRequest
{
    public int WeekNumber { get; set; }
    [Required] public string Title { get; set; } = string.Empty;
    [Required] public string Scripture { get; set; } = string.Empty;
    [Required] public string Summary { get; set; } = string.Empty;
    public string? VideoUrl { get; set; }
}

public class DiscussionGuideDto
{
    public Guid Id { get; set; }
    public string GeneratedContent { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}

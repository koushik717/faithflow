namespace FaithFlow.Domain.Entities;

public class SermonWeek
{
    public Guid Id { get; set; }
    public Guid SeriesId { get; set; }
    public int WeekNumber { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Scripture { get; set; } = string.Empty;
    public string Summary { get; set; } = string.Empty;
    public string? VideoUrl { get; set; }

    // Navigation
    public SermonSeries Series { get; set; } = null!;
    public DiscussionGuide? DiscussionGuide { get; set; }
}

namespace FaithFlow.Domain.Entities;

public class DiscussionGuide
{
    public Guid Id { get; set; }
    public Guid SermonWeekId { get; set; }
    public string GeneratedContent { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation
    public SermonWeek SermonWeek { get; set; } = null!;
}

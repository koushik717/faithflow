namespace FaithFlow.Domain.Entities;

public class SermonSeries
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public int TotalWeeks { get; set; }
    public string? ImageUrl { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation
    public ICollection<SermonWeek> Weeks { get; set; } = new List<SermonWeek>();
    public ICollection<SermonNote> SermonNotes { get; set; } = new List<SermonNote>();
}

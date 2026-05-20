namespace FaithFlow.Domain.Entities;

public class SermonNote
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string PreacherName { get; set; } = string.Empty;
    public DateTime Date { get; set; }
    public string Location { get; set; } = string.Empty;
    public string? ScriptureReference { get; set; }
    public string? ScriptureText { get; set; }
    public string Notes { get; set; } = string.Empty;
    public Guid? SeriesId { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation
    public User User { get; set; } = null!;
    public SermonSeries? Series { get; set; }
}

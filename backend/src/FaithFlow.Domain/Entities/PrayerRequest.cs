namespace FaithFlow.Domain.Entities;

public class PrayerRequest
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public bool IsAnonymous { get; set; }
    public bool IsAnswered { get; set; }
    public int PrayCount { get; set; }
    public bool IsPublic { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? AnsweredAt { get; set; }

    // Navigation
    public User User { get; set; } = null!;
    public ICollection<PrayerInteraction> PrayerInteractions { get; set; } = new List<PrayerInteraction>();
}

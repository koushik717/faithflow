namespace FaithFlow.Domain.Entities;

public class PrayerInteraction
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public Guid PrayerRequestId { get; set; }
    public DateTime PrayedAt { get; set; } = DateTime.UtcNow;

    // Navigation
    public User User { get; set; } = null!;
    public PrayerRequest PrayerRequest { get; set; } = null!;
}

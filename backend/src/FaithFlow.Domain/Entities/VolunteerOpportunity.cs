using FaithFlow.Domain.Enums;

namespace FaithFlow.Domain.Entities;

public class VolunteerOpportunity
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DateTime Date { get; set; }
    public string Location { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public int TotalSpots { get; set; }
    public int FilledSpots { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation
    public ICollection<VolunteerSignup> Signups { get; set; } = new List<VolunteerSignup>();
}

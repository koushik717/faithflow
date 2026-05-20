using FaithFlow.Domain.Enums;

namespace FaithFlow.Domain.Entities;

public class VolunteerSignup
{
    public Guid Id { get; set; }
    public Guid OpportunityId { get; set; }
    public Guid UserId { get; set; }
    public DateTime SignedUpAt { get; set; } = DateTime.UtcNow;
    public VolunteerSignupStatus Status { get; set; } = VolunteerSignupStatus.Active;

    // Navigation
    public VolunteerOpportunity Opportunity { get; set; } = null!;
    public User User { get; set; } = null!;
}

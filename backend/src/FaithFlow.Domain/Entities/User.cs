using FaithFlow.Domain.Enums;

namespace FaithFlow.Domain.Entities;

public class User
{
    public Guid Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public UserRole Role { get; set; } = UserRole.Member;
    public string? ProfileImageUrl { get; set; }
    public string? Bio { get; set; }
    public DateTime JoinedAt { get; set; } = DateTime.UtcNow;
    public bool IsActive { get; set; } = true;

    // Navigation
    public ICollection<SermonNote> SermonNotes { get; set; } = new List<SermonNote>();
    public ICollection<PrayerRequest> PrayerRequests { get; set; } = new List<PrayerRequest>();
    public ICollection<PrayerInteraction> PrayerInteractions { get; set; } = new List<PrayerInteraction>();
    public ICollection<GroupMember> GroupMemberships { get; set; } = new List<GroupMember>();
    public ICollection<VolunteerSignup> VolunteerSignups { get; set; } = new List<VolunteerSignup>();
    public ICollection<RefreshToken> RefreshTokens { get; set; } = new List<RefreshToken>();
}

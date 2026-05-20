namespace FaithFlow.Domain.Entities;

public class Group
{
    public Guid Id { get; set; }
    public Guid LeaderId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string Schedule { get; set; } = string.Empty;
    public int MaxMembers { get; set; }
    public string? MeetingLocation { get; set; }
    public bool IsOnline { get; set; }
    public string? ImageUrl { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation
    public User Leader { get; set; } = null!;
    public ICollection<GroupMember> Members { get; set; } = new List<GroupMember>();
    public ICollection<GroupAttendance> Attendances { get; set; } = new List<GroupAttendance>();
}

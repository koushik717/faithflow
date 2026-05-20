using FaithFlow.Domain.Enums;

namespace FaithFlow.Domain.Entities;

public class GroupMember
{
    public Guid Id { get; set; }
    public Guid GroupId { get; set; }
    public Guid UserId { get; set; }
    public DateTime JoinedAt { get; set; } = DateTime.UtcNow;
    public GroupMemberRole Role { get; set; } = GroupMemberRole.Member;

    // Navigation
    public Group Group { get; set; } = null!;
    public User User { get; set; } = null!;
}

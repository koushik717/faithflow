namespace FaithFlow.Domain.Entities;

public class GroupAttendance
{
    public Guid Id { get; set; }
    public Guid GroupId { get; set; }
    public DateTime Date { get; set; }
    public int AttendeeCount { get; set; }
    public string? Notes { get; set; }

    // Navigation
    public Group Group { get; set; } = null!;
}

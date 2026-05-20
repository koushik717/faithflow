namespace FaithFlow.Application.Common.DTOs.Admin;

public class DashboardStatsDto
{
    public int TotalMembers { get; set; }
    public int NewMembersThisMonth { get; set; }
    public int ActiveGroups { get; set; }
    public int PrayerRequestsThisWeek { get; set; }
    public double VolunteerCoveragePercent { get; set; }
}

public class GroupAnalyticsDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public int MemberCount { get; set; }
    public int MaxMembers { get; set; }
    public double AverageAttendance { get; set; }
    public string LeaderName { get; set; } = string.Empty;
}

using FaithFlow.Application.Common.DTOs.Groups;
using FaithFlow.Application.Common.DTOs.Volunteer;
using FaithFlow.Application.Common.DTOs.SermonSeries;
using FaithFlow.Application.Common.DTOs.Admin;
using FaithFlow.Application.Common.Interfaces;
using FaithFlow.Domain.Entities;
using FaithFlow.Domain.Enums;
using FaithFlow.Domain.Exceptions;
using FaithFlow.Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace FaithFlow.Application.Services;

public class GroupService : IGroupService
{
    private readonly IGroupRepository _groupRepo;
    private readonly IUserRepository _userRepo;
    private readonly ILogger<GroupService> _logger;

    public GroupService(IGroupRepository groupRepo, IUserRepository userRepo, ILogger<GroupService> logger)
    {
        _groupRepo = groupRepo; _userRepo = userRepo; _logger = logger;
    }

    public async Task<IEnumerable<GroupDto>> GetAllGroupsAsync(Guid? currentUserId, string? query, string? category)
    {
        var groups = await _groupRepo.SearchAsync(query, category);
        return groups.Select(g => MapToDto(g, currentUserId));
    }

    public async Task<IEnumerable<GroupDto>> GetMyGroupsAsync(Guid userId)
    {
        var groups = await _groupRepo.GetByUserIdAsync(userId);
        return groups.Select(g => MapToDto(g, userId));
    }

    public async Task<GroupDto> GetByIdAsync(Guid groupId, Guid? currentUserId)
    {
        var group = await _groupRepo.GetWithMembersAsync(groupId)
            ?? throw new NotFoundException("Group", groupId);
        return MapToDto(group, currentUserId);
    }

    public async Task<GroupDto> CreateAsync(Guid leaderId, CreateGroupRequest request)
    {
        var group = new Group
        {
            Id = Guid.NewGuid(), LeaderId = leaderId, Name = request.Name,
            Description = request.Description, Category = request.Category,
            Schedule = request.Schedule, MaxMembers = request.MaxMembers,
            MeetingLocation = request.MeetingLocation, IsOnline = request.IsOnline,
            ImageUrl = request.ImageUrl, CreatedAt = DateTime.UtcNow
        };
        group.Members.Add(new GroupMember
        {
            Id = Guid.NewGuid(), GroupId = group.Id, UserId = leaderId,
            Role = GroupMemberRole.Leader, JoinedAt = DateTime.UtcNow
        });
        await _groupRepo.AddAsync(group);
        _logger.LogInformation("Group created: {Name}", group.Name);
        return MapToDto(group, leaderId);
    }

    public async Task<GroupDto> UpdateAsync(Guid userId, Guid groupId, UpdateGroupRequest request)
    {
        var group = await _groupRepo.GetByIdAsync(groupId)
            ?? throw new NotFoundException("Group", groupId);
        if (group.LeaderId != userId) throw new ForbiddenException("Only the group leader can update.");

        if (request.Name != null) group.Name = request.Name;
        if (request.Description != null) group.Description = request.Description;
        if (request.Category != null) group.Category = request.Category;
        if (request.Schedule != null) group.Schedule = request.Schedule;
        if (request.MaxMembers.HasValue) group.MaxMembers = request.MaxMembers.Value;
        if (request.MeetingLocation != null) group.MeetingLocation = request.MeetingLocation;
        if (request.IsOnline.HasValue) group.IsOnline = request.IsOnline.Value;

        await _groupRepo.UpdateAsync(group);
        return MapToDto(group, userId);
    }

    public async Task DeleteAsync(Guid userId, Guid groupId)
    {
        var group = await _groupRepo.GetByIdAsync(groupId)
            ?? throw new NotFoundException("Group", groupId);
        if (group.LeaderId != userId) throw new ForbiddenException();
        await _groupRepo.DeleteAsync(group);
    }

    public async Task JoinAsync(Guid userId, Guid groupId)
    {
        var group = await _groupRepo.GetWithMembersAsync(groupId)
            ?? throw new NotFoundException("Group", groupId);
        if (group.Members.Any(m => m.UserId == userId))
            throw new ConflictException("Already a member.");
        if (group.Members.Count >= group.MaxMembers)
            throw new BadRequestException("Group is full.");

        group.Members.Add(new GroupMember
        {
            Id = Guid.NewGuid(), GroupId = groupId, UserId = userId,
            Role = GroupMemberRole.Member, JoinedAt = DateTime.UtcNow
        });
        await _groupRepo.UpdateAsync(group);
    }

    public async Task LeaveAsync(Guid userId, Guid groupId)
    {
        var group = await _groupRepo.GetWithMembersAsync(groupId)
            ?? throw new NotFoundException("Group", groupId);
        var member = group.Members.FirstOrDefault(m => m.UserId == userId)
            ?? throw new BadRequestException("Not a member.");
        if (group.LeaderId == userId)
            throw new BadRequestException("Leader cannot leave. Transfer leadership first.");
        group.Members.Remove(member);
        await _groupRepo.UpdateAsync(group);
    }

    public async Task<IEnumerable<GroupMemberDto>> GetMembersAsync(Guid groupId)
    {
        var group = await _groupRepo.GetWithMembersAsync(groupId)
            ?? throw new NotFoundException("Group", groupId);
        var dtos = new List<GroupMemberDto>();
        foreach (var m in group.Members)
        {
            var user = m.User ?? await _userRepo.GetByIdAsync(m.UserId);
            dtos.Add(new GroupMemberDto
            {
                UserId = m.UserId, FirstName = user?.FirstName ?? "",
                LastName = user?.LastName ?? "", Role = m.Role.ToString(), JoinedAt = m.JoinedAt
            });
        }
        return dtos;
    }

    public async Task<AttendanceDto> RecordAttendanceAsync(Guid userId, Guid groupId, RecordAttendanceRequest request)
    {
        var group = await _groupRepo.GetByIdAsync(groupId)
            ?? throw new NotFoundException("Group", groupId);
        if (group.LeaderId != userId) throw new ForbiddenException("Only leader can record attendance.");

        var attendance = new GroupAttendance
        {
            Id = Guid.NewGuid(), GroupId = groupId, Date = request.Date,
            AttendeeCount = request.AttendeeCount, Notes = request.Notes
        };
        group.Attendances.Add(attendance);
        await _groupRepo.UpdateAsync(group);
        return new AttendanceDto { Id = attendance.Id, Date = attendance.Date, AttendeeCount = attendance.AttendeeCount, Notes = attendance.Notes };
    }

    public async Task<IEnumerable<AttendanceDto>> GetAttendanceAsync(Guid groupId)
    {
        var group = await _groupRepo.GetWithMembersAsync(groupId)
            ?? throw new NotFoundException("Group", groupId);
        return group.Attendances.OrderByDescending(a => a.Date)
            .Select(a => new AttendanceDto { Id = a.Id, Date = a.Date, AttendeeCount = a.AttendeeCount, Notes = a.Notes });
    }

    private static GroupDto MapToDto(Group g, Guid? currentUserId) => new()
    {
        Id = g.Id, LeaderId = g.LeaderId, LeaderName = g.Leader != null ? $"{g.Leader.FirstName} {g.Leader.LastName}" : "",
        Name = g.Name, Description = g.Description, Category = g.Category,
        Schedule = g.Schedule, MaxMembers = g.MaxMembers, MemberCount = g.Members.Count,
        MeetingLocation = g.MeetingLocation, IsOnline = g.IsOnline, ImageUrl = g.ImageUrl,
        CreatedAt = g.CreatedAt, IsMember = currentUserId.HasValue && g.Members.Any(m => m.UserId == currentUserId)
    };
}

public class VolunteerService : IVolunteerService
{
    private readonly IVolunteerRepository _volunteerRepo;
    private readonly ILogger<VolunteerService> _logger;

    public VolunteerService(IVolunteerRepository volunteerRepo, ILogger<VolunteerService> logger)
    {
        _volunteerRepo = volunteerRepo; _logger = logger;
    }

    public async Task<IEnumerable<VolunteerOpportunityDto>> GetOpportunitiesAsync(Guid? userId, string? category, DateTime? date)
    {
        var opps = await _volunteerRepo.SearchAsync(category, date);
        return opps.Select(o => MapToDto(o, userId));
    }

    public async Task<VolunteerOpportunityDto> CreateOpportunityAsync(CreateOpportunityRequest request)
    {
        var opp = new VolunteerOpportunity
        {
            Id = Guid.NewGuid(), Title = request.Title, Description = request.Description,
            Date = request.Date, Location = request.Location, Category = request.Category,
            TotalSpots = request.TotalSpots, CreatedAt = DateTime.UtcNow
        };
        await _volunteerRepo.AddAsync(opp);
        return MapToDto(opp, null);
    }

    public async Task<VolunteerOpportunityDto> UpdateOpportunityAsync(Guid id, CreateOpportunityRequest request)
    {
        var opp = await _volunteerRepo.GetByIdAsync(id) ?? throw new NotFoundException("Opportunity", id);
        opp.Title = request.Title; opp.Description = request.Description;
        opp.Date = request.Date; opp.Location = request.Location;
        opp.Category = request.Category; opp.TotalSpots = request.TotalSpots;
        await _volunteerRepo.UpdateAsync(opp);
        return MapToDto(opp, null);
    }

    public async Task DeleteOpportunityAsync(Guid id)
    {
        var opp = await _volunteerRepo.GetByIdAsync(id) ?? throw new NotFoundException("Opportunity", id);
        await _volunteerRepo.DeleteAsync(opp);
    }

    public async Task SignupAsync(Guid userId, Guid opportunityId)
    {
        var opp = await _volunteerRepo.GetByIdAsync(opportunityId)
            ?? throw new NotFoundException("Opportunity", opportunityId);
        if (opp.Signups.Any(s => s.UserId == userId && s.Status == VolunteerSignupStatus.Active))
            throw new ConflictException("Already signed up.");
        if (opp.FilledSpots >= opp.TotalSpots) throw new BadRequestException("No spots available.");

        opp.Signups.Add(new VolunteerSignup
        {
            Id = Guid.NewGuid(), OpportunityId = opportunityId, UserId = userId,
            SignedUpAt = DateTime.UtcNow, Status = VolunteerSignupStatus.Active
        });
        opp.FilledSpots++;
        await _volunteerRepo.UpdateAsync(opp);
    }

    public async Task CancelSignupAsync(Guid userId, Guid opportunityId)
    {
        var opp = await _volunteerRepo.GetByIdAsync(opportunityId)
            ?? throw new NotFoundException("Opportunity", opportunityId);
        var signup = opp.Signups.FirstOrDefault(s => s.UserId == userId && s.Status == VolunteerSignupStatus.Active)
            ?? throw new BadRequestException("No active signup found.");
        signup.Status = VolunteerSignupStatus.Cancelled;
        opp.FilledSpots = Math.Max(0, opp.FilledSpots - 1);
        await _volunteerRepo.UpdateAsync(opp);
    }

    public async Task<IEnumerable<VolunteerSignupDto>> GetMySignupsAsync(Guid userId)
    {
        var all = await _volunteerRepo.GetAllAsync();
        return all.SelectMany(o => o.Signups.Where(s => s.UserId == userId)
            .Select(s => new VolunteerSignupDto
            {
                Id = s.Id, OpportunityId = o.Id, OpportunityTitle = o.Title,
                Date = o.Date, Location = o.Location, Status = s.Status.ToString(),
                SignedUpAt = s.SignedUpAt
            }));
    }

    public async Task<CoverageReportDto> GetCoverageReportAsync()
    {
        var opps = await _volunteerRepo.GetUpcomingAsync();
        var list = opps.ToList();
        var total = list.Count;
        var full = list.Count(o => o.FilledSpots >= o.TotalSpots);
        var partial = list.Count(o => o.FilledSpots > 0 && o.FilledSpots < o.TotalSpots);
        var none = list.Count(o => o.FilledSpots == 0);
        return new CoverageReportDto
        {
            TotalOpportunities = total, FullyCovered = full, PartiallyCovered = partial,
            NoCoverage = none, CoveragePercentage = total > 0 ? Math.Round((double)full / total * 100, 1) : 0
        };
    }

    private static VolunteerOpportunityDto MapToDto(VolunteerOpportunity o, Guid? userId) => new()
    {
        Id = o.Id, Title = o.Title, Description = o.Description, Date = o.Date,
        Location = o.Location, Category = o.Category, TotalSpots = o.TotalSpots,
        FilledSpots = o.FilledSpots, CreatedAt = o.CreatedAt,
        IsSignedUp = userId.HasValue && o.Signups.Any(s => s.UserId == userId && s.Status == VolunteerSignupStatus.Active)
    };
}

public class SermonSeriesService : ISermonSeriesService
{
    private readonly ISermonSeriesRepository _seriesRepo;
    private readonly IGeminiService _geminiService;

    public SermonSeriesService(ISermonSeriesRepository seriesRepo, IGeminiService geminiService)
    {
        _seriesRepo = seriesRepo; _geminiService = geminiService;
    }

    public async Task<IEnumerable<SermonSeriesDto>> GetAllAsync()
    {
        var series = await _seriesRepo.GetAllAsync();
        return series.Select(MapToDto);
    }

    public async Task<SermonSeriesDto> GetByIdAsync(Guid id)
    {
        var s = await _seriesRepo.GetWithWeeksAsync(id) ?? throw new NotFoundException("SermonSeries", id);
        return MapToDto(s);
    }

    public async Task<SermonSeriesDto> CreateAsync(CreateSermonSeriesRequest request)
    {
        var s = new SermonSeries
        {
            Id = Guid.NewGuid(), Title = request.Title, Description = request.Description,
            StartDate = request.StartDate, EndDate = request.EndDate,
            TotalWeeks = request.TotalWeeks, ImageUrl = request.ImageUrl, CreatedAt = DateTime.UtcNow
        };
        await _seriesRepo.AddAsync(s);
        return MapToDto(s);
    }

    public async Task<SermonSeriesDto> UpdateAsync(Guid id, CreateSermonSeriesRequest request)
    {
        var s = await _seriesRepo.GetByIdAsync(id) ?? throw new NotFoundException("SermonSeries", id);
        s.Title = request.Title; s.Description = request.Description;
        s.StartDate = request.StartDate; s.EndDate = request.EndDate;
        s.TotalWeeks = request.TotalWeeks; s.ImageUrl = request.ImageUrl;
        await _seriesRepo.UpdateAsync(s);
        return MapToDto(s);
    }

    public async Task<SermonWeekDto> CreateWeekAsync(Guid seriesId, CreateSermonWeekRequest request)
    {
        var series = await _seriesRepo.GetWithWeeksAsync(seriesId) ?? throw new NotFoundException("SermonSeries", seriesId);
        var week = new SermonWeek
        {
            Id = Guid.NewGuid(), SeriesId = seriesId, WeekNumber = request.WeekNumber,
            Title = request.Title, Scripture = request.Scripture,
            Summary = request.Summary, VideoUrl = request.VideoUrl
        };
        series.Weeks.Add(week);
        await _seriesRepo.UpdateAsync(series);
        return MapWeekToDto(week);
    }

    public async Task<IEnumerable<SermonWeekDto>> GetWeeksAsync(Guid seriesId)
    {
        var series = await _seriesRepo.GetWithWeeksAsync(seriesId) ?? throw new NotFoundException("SermonSeries", seriesId);
        return series.Weeks.OrderBy(w => w.WeekNumber).Select(MapWeekToDto);
    }

    public async Task<DiscussionGuideDto> GenerateDiscussionGuideAsync(Guid seriesId, Guid weekId)
    {
        var series = await _seriesRepo.GetWithWeeksAsync(seriesId) ?? throw new NotFoundException("SermonSeries", seriesId);
        var week = series.Weeks.FirstOrDefault(w => w.Id == weekId) ?? throw new NotFoundException("SermonWeek", weekId);

        var content = await _geminiService.GenerateDiscussionGuideAsync(week.Title, week.Scripture, week.Summary);
        var guide = new DiscussionGuide
        {
            Id = Guid.NewGuid(), SermonWeekId = weekId, GeneratedContent = content, CreatedAt = DateTime.UtcNow
        };
        week.DiscussionGuide = guide;
        await _seriesRepo.UpdateAsync(series);
        return new DiscussionGuideDto { Id = guide.Id, GeneratedContent = guide.GeneratedContent, CreatedAt = guide.CreatedAt };
    }

    private static SermonSeriesDto MapToDto(SermonSeries s) => new()
    {
        Id = s.Id, Title = s.Title, Description = s.Description, StartDate = s.StartDate,
        EndDate = s.EndDate, TotalWeeks = s.TotalWeeks, ImageUrl = s.ImageUrl, WeekCount = s.Weeks.Count
    };

    private static SermonWeekDto MapWeekToDto(SermonWeek w) => new()
    {
        Id = w.Id, WeekNumber = w.WeekNumber, Title = w.Title, Scripture = w.Scripture,
        Summary = w.Summary, VideoUrl = w.VideoUrl,
        DiscussionGuide = w.DiscussionGuide != null ? new DiscussionGuideDto
        {
            Id = w.DiscussionGuide.Id, GeneratedContent = w.DiscussionGuide.GeneratedContent,
            CreatedAt = w.DiscussionGuide.CreatedAt
        } : null
    };
}

public class AdminService : IAdminService
{
    private readonly IUserRepository _userRepo;
    private readonly IGroupRepository _groupRepo;
    private readonly IPrayerRequestRepository _prayerRepo;
    private readonly IVolunteerService _volunteerService;

    public AdminService(IUserRepository userRepo, IGroupRepository groupRepo,
        IPrayerRequestRepository prayerRepo, IVolunteerService volunteerService)
    {
        _userRepo = userRepo; _groupRepo = groupRepo;
        _prayerRepo = prayerRepo; _volunteerService = volunteerService;
    }

    public async Task<DashboardStatsDto> GetStatsAsync()
    {
        var totalMembers = await _userRepo.CountAsync();
        var newThisMonth = await _userRepo.CountAsync(u => u.JoinedAt >= DateTime.UtcNow.AddDays(-30));
        var groups = await _groupRepo.GetAllAsync();
        var prayerCount = await _prayerRepo.GetWeeklyCountAsync();
        var coverage = await _volunteerService.GetCoverageReportAsync();

        return new DashboardStatsDto
        {
            TotalMembers = totalMembers, NewMembersThisMonth = newThisMonth,
            ActiveGroups = groups.Count(), PrayerRequestsThisWeek = prayerCount,
            VolunteerCoveragePercent = coverage.CoveragePercentage
        };
    }

    public async Task<IEnumerable<GroupAnalyticsDto>> GetGroupAnalyticsAsync()
    {
        var groups = await _groupRepo.GetAllAsync();
        return groups.Select(g => new GroupAnalyticsDto
        {
            Id = g.Id, Name = g.Name, Category = g.Category,
            MemberCount = g.Members.Count, MaxMembers = g.MaxMembers,
            AverageAttendance = g.Attendances.Any() ? g.Attendances.Average(a => a.AttendeeCount) : 0,
            LeaderName = g.Leader != null ? $"{g.Leader.FirstName} {g.Leader.LastName}" : ""
        });
    }
}

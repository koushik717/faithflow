using FaithFlow.Application.Common.DTOs.Auth;
using FaithFlow.Application.Common.DTOs.Users;
using FaithFlow.Application.Common.DTOs.SermonNotes;
using FaithFlow.Application.Common.DTOs.PrayerRequests;
using FaithFlow.Application.Common.DTOs.Groups;
using FaithFlow.Application.Common.DTOs.Volunteer;
using FaithFlow.Application.Common.DTOs.SermonSeries;
using FaithFlow.Application.Common.DTOs.Admin;
using FaithFlow.Domain.Entities;

namespace FaithFlow.Application.Common.Interfaces;

public interface ITokenService
{
    string GenerateAccessToken(User user);
    RefreshToken GenerateRefreshToken(Guid userId);
    DateTime GetAccessTokenExpiry();
}

public interface IGeminiService
{
    Task<string> GeneratePrayerAsync(string title, string description);
    Task<string> GenerateDiscussionGuideAsync(string title, string scripture, string summary);
}

public interface IBibleService
{
    Task<ScriptureLookupResponse?> LookupScriptureAsync(string reference);
}

public interface IAuthService
{
    Task<AuthResponse> RegisterAsync(RegisterRequest request);
    Task<AuthResponse> LoginAsync(LoginRequest request);
    Task<AuthResponse> RefreshTokenAsync(string refreshToken);
    Task LogoutAsync(Guid userId, string refreshToken);
}

public interface IUserService
{
    Task<UserDto> GetProfileAsync(Guid userId);
    Task<UserDto> UpdateProfileAsync(Guid userId, UpdateProfileRequest request);
    Task<IEnumerable<AdminUserDto>> GetAllUsersAsync(string? query, int page, int pageSize);
    Task<UserDto> ChangeRoleAsync(Guid userId, string role);
}

public interface ISermonNoteService
{
    Task<IEnumerable<SermonNoteDto>> GetMyNotesAsync(Guid userId);
    Task<SermonNoteDto> GetByIdAsync(Guid userId, Guid noteId);
    Task<SermonNoteDto> CreateAsync(Guid userId, CreateSermonNoteRequest request);
    Task<SermonNoteDto> UpdateAsync(Guid userId, Guid noteId, UpdateSermonNoteRequest request);
    Task DeleteAsync(Guid userId, Guid noteId);
    Task<IEnumerable<SermonNoteDto>> SearchAsync(Guid userId, string query);
    Task<ScriptureLookupResponse> ScriptureLookupAsync(Guid noteId, Guid userId);
}

public interface IPrayerRequestService
{
    Task<IEnumerable<PrayerRequestDto>> GetPublicRequestsAsync();
    Task<IEnumerable<PrayerRequestDto>> GetMyRequestsAsync(Guid userId);
    Task<IEnumerable<PrayerRequestDto>> GetAnsweredRequestsAsync();
    Task<PrayerRequestDto> CreateAsync(Guid userId, CreatePrayerRequestRequest request);
    Task<PrayerRequestDto> UpdateAsync(Guid userId, Guid requestId, UpdatePrayerRequestRequest request);
    Task DeleteAsync(Guid userId, Guid requestId);
    Task<int> PrayAsync(Guid userId, Guid requestId);
    Task<AiPrayerResponse> GetAiPrayerAsync(Guid requestId);
    Task MarkAnsweredAsync(Guid userId, Guid requestId);
}

public interface IGroupService
{
    Task<IEnumerable<GroupDto>> GetAllGroupsAsync(Guid? currentUserId, string? query, string? category);
    Task<IEnumerable<GroupDto>> GetMyGroupsAsync(Guid userId);
    Task<GroupDto> GetByIdAsync(Guid groupId, Guid? currentUserId);
    Task<GroupDto> CreateAsync(Guid leaderId, CreateGroupRequest request);
    Task<GroupDto> UpdateAsync(Guid userId, Guid groupId, UpdateGroupRequest request);
    Task DeleteAsync(Guid userId, Guid groupId);
    Task JoinAsync(Guid userId, Guid groupId);
    Task LeaveAsync(Guid userId, Guid groupId);
    Task<IEnumerable<GroupMemberDto>> GetMembersAsync(Guid groupId);
    Task<AttendanceDto> RecordAttendanceAsync(Guid userId, Guid groupId, RecordAttendanceRequest request);
    Task<IEnumerable<AttendanceDto>> GetAttendanceAsync(Guid groupId);
}

public interface IVolunteerService
{
    Task<IEnumerable<VolunteerOpportunityDto>> GetOpportunitiesAsync(Guid? userId, string? category, DateTime? date);
    Task<VolunteerOpportunityDto> CreateOpportunityAsync(CreateOpportunityRequest request);
    Task<VolunteerOpportunityDto> UpdateOpportunityAsync(Guid id, CreateOpportunityRequest request);
    Task DeleteOpportunityAsync(Guid id);
    Task SignupAsync(Guid userId, Guid opportunityId);
    Task CancelSignupAsync(Guid userId, Guid opportunityId);
    Task<IEnumerable<VolunteerSignupDto>> GetMySignupsAsync(Guid userId);
    Task<CoverageReportDto> GetCoverageReportAsync();
}

public interface ISermonSeriesService
{
    Task<IEnumerable<SermonSeriesDto>> GetAllAsync();
    Task<SermonSeriesDto> GetByIdAsync(Guid id);
    Task<SermonSeriesDto> CreateAsync(CreateSermonSeriesRequest request);
    Task<SermonSeriesDto> UpdateAsync(Guid id, CreateSermonSeriesRequest request);
    Task<SermonWeekDto> CreateWeekAsync(Guid seriesId, CreateSermonWeekRequest request);
    Task<IEnumerable<SermonWeekDto>> GetWeeksAsync(Guid seriesId);
    Task<DiscussionGuideDto> GenerateDiscussionGuideAsync(Guid seriesId, Guid weekId);
}

public interface IAdminService
{
    Task<DashboardStatsDto> GetStatsAsync();
    Task<IEnumerable<GroupAnalyticsDto>> GetGroupAnalyticsAsync();
}

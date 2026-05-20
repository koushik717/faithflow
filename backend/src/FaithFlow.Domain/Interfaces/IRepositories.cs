using FaithFlow.Domain.Entities;

namespace FaithFlow.Domain.Interfaces;

public interface IUserRepository : IRepository<User>
{
    Task<User?> GetByEmailAsync(string email);
    Task<IEnumerable<User>> SearchAsync(string? query, int page, int pageSize);
}

public interface ISermonNoteRepository : IRepository<SermonNote>
{
    Task<IEnumerable<SermonNote>> GetByUserIdAsync(Guid userId);
    Task<IEnumerable<SermonNote>> SearchAsync(Guid userId, string query);
}

public interface IPrayerRequestRepository : IRepository<PrayerRequest>
{
    Task<IEnumerable<PrayerRequest>> GetPublicRequestsAsync();
    Task<IEnumerable<PrayerRequest>> GetByUserIdAsync(Guid userId);
    Task<IEnumerable<PrayerRequest>> GetAnsweredAsync();
    Task<int> GetWeeklyCountAsync();
}

public interface IGroupRepository : IRepository<Group>
{
    Task<IEnumerable<Group>> SearchAsync(string? query, string? category);
    Task<IEnumerable<Group>> GetByUserIdAsync(Guid userId);
    Task<Group?> GetWithMembersAsync(Guid groupId);
}

public interface IVolunteerRepository : IRepository<VolunteerOpportunity>
{
    Task<IEnumerable<VolunteerOpportunity>> GetUpcomingAsync();
    Task<IEnumerable<VolunteerOpportunity>> SearchAsync(string? category, DateTime? date);
}

public interface ISermonSeriesRepository : IRepository<SermonSeries>
{
    Task<SermonSeries?> GetWithWeeksAsync(Guid id);
}

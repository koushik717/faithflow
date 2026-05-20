using System.Linq.Expressions;
using FaithFlow.Domain.Entities;
using FaithFlow.Domain.Interfaces;
using FaithFlow.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace FaithFlow.Infrastructure.Data.Repositories;

public class Repository<T> : IRepository<T> where T : class
{
    protected readonly FaithFlowDbContext _context;
    protected readonly DbSet<T> _dbSet;

    public Repository(FaithFlowDbContext context)
    {
        _context = context;
        _dbSet = context.Set<T>();
    }

    public virtual async Task<T?> GetByIdAsync(Guid id) => await _dbSet.FindAsync(id);
    public virtual async Task<IEnumerable<T>> GetAllAsync() => await _dbSet.ToListAsync();
    public async Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate) => await _dbSet.Where(predicate).ToListAsync();
    public async Task<T> AddAsync(T entity) { await _dbSet.AddAsync(entity); await _context.SaveChangesAsync(); return entity; }
    public async Task UpdateAsync(T entity) { _dbSet.Update(entity); await _context.SaveChangesAsync(); }
    public async Task DeleteAsync(T entity) { _dbSet.Remove(entity); await _context.SaveChangesAsync(); }
    public async Task<bool> ExistsAsync(Guid id) => await _dbSet.FindAsync(id) != null;
    public async Task<int> CountAsync(Expression<Func<T, bool>>? predicate = null) =>
        predicate != null ? await _dbSet.CountAsync(predicate) : await _dbSet.CountAsync();
}

public class UserRepository : Repository<User>, IUserRepository
{
    public UserRepository(FaithFlowDbContext context) : base(context) { }

    public override async Task<User?> GetByIdAsync(Guid id) =>
        await _dbSet.Include(u => u.RefreshTokens).FirstOrDefaultAsync(u => u.Id == id);

    public async Task<User?> GetByEmailAsync(string email) =>
        await _dbSet.Include(u => u.RefreshTokens).FirstOrDefaultAsync(u => u.Email == email.ToLowerInvariant());

    public async Task<User?> GetByRefreshTokenAsync(string token) =>
        await _dbSet.Include(u => u.RefreshTokens)
            .FirstOrDefaultAsync(u => u.RefreshTokens.Any(t => t.Token == token && !t.IsRevoked && t.ExpiryDate > DateTime.UtcNow));

    public async Task<IEnumerable<User>> SearchAsync(string? query, int page, int pageSize)
    {
        var q = _dbSet.AsQueryable();
        if (!string.IsNullOrWhiteSpace(query))
            q = q.Where(u => u.FirstName.Contains(query) || u.LastName.Contains(query) || u.Email.Contains(query));
        return await q.OrderBy(u => u.JoinedAt).Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();
    }
}

public class SermonNoteRepository : Repository<SermonNote>, ISermonNoteRepository
{
    public SermonNoteRepository(FaithFlowDbContext context) : base(context) { }

    public override async Task<SermonNote?> GetByIdAsync(Guid id) =>
        await _dbSet.Include(n => n.Series).FirstOrDefaultAsync(n => n.Id == id);

    public async Task<IEnumerable<SermonNote>> GetByUserIdAsync(Guid userId) =>
        await _dbSet.Include(n => n.Series).Where(n => n.UserId == userId).OrderByDescending(n => n.CreatedAt).ToListAsync();

    public async Task<IEnumerable<SermonNote>> SearchAsync(Guid userId, string query) =>
        await _dbSet.Include(n => n.Series)
            .Where(n => n.UserId == userId && (n.Title.Contains(query) || n.PreacherName.Contains(query) || n.Notes.Contains(query)))
            .OrderByDescending(n => n.CreatedAt).ToListAsync();
}

public class PrayerRequestRepository : Repository<PrayerRequest>, IPrayerRequestRepository
{
    public PrayerRequestRepository(FaithFlowDbContext context) : base(context) { }

    public override async Task<PrayerRequest?> GetByIdAsync(Guid id) =>
        await _dbSet.Include(p => p.User).Include(p => p.PrayerInteractions).FirstOrDefaultAsync(p => p.Id == id);

    public async Task<IEnumerable<PrayerRequest>> GetPublicRequestsAsync() =>
        await _dbSet.Include(p => p.User).Where(p => p.IsPublic).OrderByDescending(p => p.CreatedAt).ToListAsync();

    public async Task<IEnumerable<PrayerRequest>> GetByUserIdAsync(Guid userId) =>
        await _dbSet.Include(p => p.User).Where(p => p.UserId == userId).OrderByDescending(p => p.CreatedAt).ToListAsync();

    public async Task<IEnumerable<PrayerRequest>> GetAnsweredAsync() =>
        await _dbSet.Include(p => p.User).Where(p => p.IsAnswered).OrderByDescending(p => p.AnsweredAt).ToListAsync();

    public async Task<int> GetWeeklyCountAsync() =>
        await _dbSet.CountAsync(p => p.CreatedAt >= DateTime.UtcNow.AddDays(-7));
}

public class GroupRepository : Repository<Group>, IGroupRepository
{
    public GroupRepository(FaithFlowDbContext context) : base(context) { }

    public override async Task<IEnumerable<Group>> GetAllAsync() =>
        await _dbSet.Include(g => g.Leader).Include(g => g.Members).Include(g => g.Attendances).ToListAsync();

    public async Task<IEnumerable<Group>> SearchAsync(string? query, string? category)
    {
        var q = _dbSet.Include(g => g.Leader).Include(g => g.Members).AsQueryable();
        if (!string.IsNullOrWhiteSpace(query))
            q = q.Where(g => g.Name.Contains(query) || g.Description.Contains(query));
        if (!string.IsNullOrWhiteSpace(category))
            q = q.Where(g => g.Category == category);
        return await q.OrderByDescending(g => g.CreatedAt).ToListAsync();
    }

    public async Task<IEnumerable<Group>> GetByUserIdAsync(Guid userId) =>
        await _dbSet.Include(g => g.Leader).Include(g => g.Members)
            .Where(g => g.Members.Any(m => m.UserId == userId)).ToListAsync();

    public async Task<Group?> GetWithMembersAsync(Guid groupId) =>
        await _dbSet.Include(g => g.Leader).Include(g => g.Members).ThenInclude(m => m.User)
            .Include(g => g.Attendances).FirstOrDefaultAsync(g => g.Id == groupId);
}

public class VolunteerRepository : Repository<VolunteerOpportunity>, IVolunteerRepository
{
    public VolunteerRepository(FaithFlowDbContext context) : base(context) { }

    public override async Task<VolunteerOpportunity?> GetByIdAsync(Guid id) =>
        await _dbSet.Include(v => v.Signups).FirstOrDefaultAsync(v => v.Id == id);

    public override async Task<IEnumerable<VolunteerOpportunity>> GetAllAsync() =>
        await _dbSet.Include(v => v.Signups).OrderBy(v => v.Date).ToListAsync();

    public async Task<IEnumerable<VolunteerOpportunity>> GetUpcomingAsync() =>
        await _dbSet.Include(v => v.Signups).Where(v => v.Date >= DateTime.UtcNow).OrderBy(v => v.Date).ToListAsync();

    public async Task<IEnumerable<VolunteerOpportunity>> SearchAsync(string? category, DateTime? date)
    {
        var q = _dbSet.Include(v => v.Signups).AsQueryable();
        if (!string.IsNullOrWhiteSpace(category)) q = q.Where(v => v.Category == category);
        if (date.HasValue) q = q.Where(v => v.Date.Date == date.Value.Date);
        return await q.OrderBy(v => v.Date).ToListAsync();
    }
}

public class SermonSeriesRepository : Repository<SermonSeries>, ISermonSeriesRepository
{
    public SermonSeriesRepository(FaithFlowDbContext context) : base(context) { }

    public override async Task<IEnumerable<SermonSeries>> GetAllAsync() =>
        await _dbSet.Include(s => s.Weeks).OrderByDescending(s => s.StartDate).ToListAsync();

    public async Task<SermonSeries?> GetWithWeeksAsync(Guid id) =>
        await _dbSet.Include(s => s.Weeks).ThenInclude(w => w.DiscussionGuide).FirstOrDefaultAsync(s => s.Id == id);
}

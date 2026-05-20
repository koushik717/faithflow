using FaithFlow.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace FaithFlow.Infrastructure.Data;

public class FaithFlowDbContext : DbContext
{
    public FaithFlowDbContext(DbContextOptions<FaithFlowDbContext> options) : base(options) { }

    public DbSet<User> Users => Set<User>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
    public DbSet<SermonNote> SermonNotes => Set<SermonNote>();
    public DbSet<PrayerRequest> PrayerRequests => Set<PrayerRequest>();
    public DbSet<PrayerInteraction> PrayerInteractions => Set<PrayerInteraction>();
    public DbSet<Group> Groups => Set<Group>();
    public DbSet<GroupMember> GroupMembers => Set<GroupMember>();
    public DbSet<GroupAttendance> GroupAttendances => Set<GroupAttendance>();
    public DbSet<VolunteerOpportunity> VolunteerOpportunities => Set<VolunteerOpportunity>();
    public DbSet<VolunteerSignup> VolunteerSignups => Set<VolunteerSignup>();
    public DbSet<SermonSeries> SermonSeries => Set<SermonSeries>();
    public DbSet<SermonWeek> SermonWeeks => Set<SermonWeek>();
    public DbSet<DiscussionGuide> DiscussionGuides => Set<DiscussionGuide>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // User
        modelBuilder.Entity<User>(e =>
        {
            e.HasKey(u => u.Id);
            e.HasIndex(u => u.Email).IsUnique();
            e.Property(u => u.Email).HasMaxLength(256).IsRequired();
            e.Property(u => u.PasswordHash).IsRequired();
            e.Property(u => u.FirstName).HasMaxLength(100).IsRequired();
            e.Property(u => u.LastName).HasMaxLength(100).IsRequired();
            e.Property(u => u.Role).HasConversion<string>().HasMaxLength(20);
            e.Property(u => u.Bio).HasMaxLength(500);
        });

        // RefreshToken
        modelBuilder.Entity<RefreshToken>(e =>
        {
            e.HasKey(r => r.Id);
            e.HasIndex(r => r.Token).IsUnique();
            e.Property(r => r.Token).HasMaxLength(256).IsRequired();
            e.HasOne(r => r.User).WithMany(u => u.RefreshTokens).HasForeignKey(r => r.UserId).OnDelete(DeleteBehavior.Cascade);
        });

        // SermonNote
        modelBuilder.Entity<SermonNote>(e =>
        {
            e.HasKey(n => n.Id);
            e.Property(n => n.Title).HasMaxLength(256).IsRequired();
            e.Property(n => n.PreacherName).HasMaxLength(200);
            e.Property(n => n.Location).HasMaxLength(200);
            e.Property(n => n.ScriptureReference).HasMaxLength(200);
            e.HasOne(n => n.User).WithMany(u => u.SermonNotes).HasForeignKey(n => n.UserId).OnDelete(DeleteBehavior.Cascade);
            e.HasOne(n => n.Series).WithMany(s => s.SermonNotes).HasForeignKey(n => n.SeriesId).OnDelete(DeleteBehavior.SetNull);
            e.HasIndex(n => n.UserId);
        });

        // PrayerRequest
        modelBuilder.Entity<PrayerRequest>(e =>
        {
            e.HasKey(p => p.Id);
            e.Property(p => p.Title).HasMaxLength(256).IsRequired();
            e.HasOne(p => p.User).WithMany(u => u.PrayerRequests).HasForeignKey(p => p.UserId).OnDelete(DeleteBehavior.Cascade);
            e.HasIndex(p => p.IsPublic);
            e.HasIndex(p => p.CreatedAt);
        });

        // PrayerInteraction
        modelBuilder.Entity<PrayerInteraction>(e =>
        {
            e.HasKey(p => p.Id);
            e.HasOne(p => p.User).WithMany(u => u.PrayerInteractions).HasForeignKey(p => p.UserId).OnDelete(DeleteBehavior.Cascade);
            e.HasOne(p => p.PrayerRequest).WithMany(r => r.PrayerInteractions).HasForeignKey(p => p.PrayerRequestId).OnDelete(DeleteBehavior.Cascade);
            e.HasIndex(p => new { p.UserId, p.PrayerRequestId });
        });

        // Group
        modelBuilder.Entity<Group>(e =>
        {
            e.HasKey(g => g.Id);
            e.Property(g => g.Name).HasMaxLength(200).IsRequired();
            e.Property(g => g.Category).HasMaxLength(100);
            e.Property(g => g.Schedule).HasMaxLength(200);
            e.HasOne(g => g.Leader).WithMany().HasForeignKey(g => g.LeaderId).OnDelete(DeleteBehavior.Restrict);
            e.HasIndex(g => g.Category);
        });

        // GroupMember
        modelBuilder.Entity<GroupMember>(e =>
        {
            e.HasKey(m => m.Id);
            e.Property(m => m.Role).HasConversion<string>().HasMaxLength(20);
            e.HasOne(m => m.Group).WithMany(g => g.Members).HasForeignKey(m => m.GroupId).OnDelete(DeleteBehavior.Cascade);
            e.HasOne(m => m.User).WithMany(u => u.GroupMemberships).HasForeignKey(m => m.UserId).OnDelete(DeleteBehavior.Cascade);
            e.HasIndex(m => new { m.GroupId, m.UserId }).IsUnique();
        });

        // GroupAttendance
        modelBuilder.Entity<GroupAttendance>(e =>
        {
            e.HasKey(a => a.Id);
            e.HasOne(a => a.Group).WithMany(g => g.Attendances).HasForeignKey(a => a.GroupId).OnDelete(DeleteBehavior.Cascade);
        });

        // VolunteerOpportunity
        modelBuilder.Entity<VolunteerOpportunity>(e =>
        {
            e.HasKey(v => v.Id);
            e.Property(v => v.Title).HasMaxLength(256).IsRequired();
            e.Property(v => v.Category).HasMaxLength(100);
            e.Property(v => v.Location).HasMaxLength(200);
            e.HasIndex(v => v.Date);
        });

        // VolunteerSignup
        modelBuilder.Entity<VolunteerSignup>(e =>
        {
            e.HasKey(s => s.Id);
            e.Property(s => s.Status).HasConversion<string>().HasMaxLength(20);
            e.HasOne(s => s.Opportunity).WithMany(o => o.Signups).HasForeignKey(s => s.OpportunityId).OnDelete(DeleteBehavior.Cascade);
            e.HasOne(s => s.User).WithMany(u => u.VolunteerSignups).HasForeignKey(s => s.UserId).OnDelete(DeleteBehavior.Cascade);
            e.HasIndex(s => new { s.OpportunityId, s.UserId });
        });

        // SermonSeries
        modelBuilder.Entity<SermonSeries>(e =>
        {
            e.HasKey(s => s.Id);
            e.Property(s => s.Title).HasMaxLength(256).IsRequired();
        });

        // SermonWeek
        modelBuilder.Entity<SermonWeek>(e =>
        {
            e.HasKey(w => w.Id);
            e.Property(w => w.Title).HasMaxLength(256).IsRequired();
            e.Property(w => w.Scripture).HasMaxLength(500);
            e.HasOne(w => w.Series).WithMany(s => s.Weeks).HasForeignKey(w => w.SeriesId).OnDelete(DeleteBehavior.Cascade);
            e.HasIndex(w => new { w.SeriesId, w.WeekNumber }).IsUnique();
        });

        // DiscussionGuide
        modelBuilder.Entity<DiscussionGuide>(e =>
        {
            e.HasKey(d => d.Id);
            e.HasOne(d => d.SermonWeek).WithOne(w => w.DiscussionGuide).HasForeignKey<DiscussionGuide>(d => d.SermonWeekId).OnDelete(DeleteBehavior.Cascade);
        });
    }
}

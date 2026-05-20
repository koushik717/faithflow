using FaithFlow.Domain.Entities;
using FaithFlow.Application.Common.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace FaithFlow.Infrastructure.Data;

public static class DataSeeder
{
    public static async Task SeedDataAsync(FaithFlowDbContext context, IAuthService authService)
    {
        if (await context.Users.AnyAsync()) return;

        // Create Admin directly
        var adminId = Guid.NewGuid();
        var admin = new User
        {
            Id = adminId,
            FirstName = "Admin",
            LastName = "User",
            Email = "admin@faithflow.com",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("Admin123!"),
            Role = FaithFlow.Domain.Enums.UserRole.Admin,
            IsActive = true,
            JoinedAt = DateTime.UtcNow
        };
        await context.Users.AddAsync(admin);

        var guestId = Guid.NewGuid();
        var guest = new User
        {
            Id = guestId,
            FirstName = "Guest",
            LastName = "User",
            Email = "guest@faithflow.com",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("Guest123!"),
            Role = FaithFlow.Domain.Enums.UserRole.Member,
            IsActive = true,
            JoinedAt = DateTime.UtcNow
        };
        await context.Users.AddAsync(guest);

        // Groups
        var groups = new[]
        {
            new Group { Id = Guid.NewGuid(), Name = "Young Adults", Description = "A community for 20s and 30s.", Category = "Young Adults", Schedule = "Tuesdays 7:00 PM", MeetingLocation = "Main Campus", MaxMembers = 30, LeaderId = admin.Id, CreatedAt = DateTime.UtcNow },
            new Group { Id = Guid.NewGuid(), Name = "Men's Breakfast", Description = "Morning fellowship and study.", Category = "Men", Schedule = "Saturdays 8:00 AM", MeetingLocation = "Fellowship Hall", MaxMembers = 50, LeaderId = admin.Id, CreatedAt = DateTime.UtcNow },
            new Group { Id = Guid.NewGuid(), Name = "Women's Bible Study", Description = "Deep dive into scripture.", Category = "Women", Schedule = "Wednesdays 10:00 AM", MeetingLocation = "Room 201", MaxMembers = 25, LeaderId = admin.Id, CreatedAt = DateTime.UtcNow }
        };
        await context.Groups.AddRangeAsync(groups);

        // Prayer Requests
        var prayers = new[]
        {
            new PrayerRequest { Id = Guid.NewGuid(), Title = "Job Interview", Description = "Please pray for my job interview tomorrow.", IsPublic = true, PrayCount = 5, UserId = admin.Id, CreatedAt = DateTime.UtcNow },
            new PrayerRequest { Id = Guid.NewGuid(), Title = "Healing for mother", Description = "My mother is in the hospital.", IsPublic = true, PrayCount = 12, UserId = admin.Id, CreatedAt = DateTime.UtcNow },
            new PrayerRequest { Id = Guid.NewGuid(), Title = "Praise: New Baby!", Description = "We welcomed our son yesterday.", IsPublic = true, PrayCount = 25, IsAnswered = true, UserId = admin.Id, CreatedAt = DateTime.UtcNow },
            new PrayerRequest { Id = Guid.NewGuid(), Title = "Financial Guidance", Description = "Need wisdom for a big decision.", IsPublic = true, PrayCount = 2, UserId = admin.Id, CreatedAt = DateTime.UtcNow },
            new PrayerRequest { Id = Guid.NewGuid(), Title = "Marriage Restoration", Description = "Praying for healing in our relationship.", IsPublic = true, PrayCount = 8, UserId = admin.Id, CreatedAt = DateTime.UtcNow }
        };
        await context.PrayerRequests.AddRangeAsync(prayers);

        // Sermon Notes
        var notes = new[]
        {
            new SermonNote { Id = Guid.NewGuid(), Title = "The Power of Faith", Notes = "Faith moves mountains...", Date = DateTime.UtcNow.AddDays(-7), PreacherName = "Pastor John", ScriptureReference = "Hebrews 11:1", UserId = admin.Id },
            new SermonNote { Id = Guid.NewGuid(), Title = "Love Your Neighbor", Notes = "We are called to love...", Date = DateTime.UtcNow.AddDays(-14), PreacherName = "Pastor Sarah", ScriptureReference = "Mark 12:31", UserId = admin.Id },
            new SermonNote { Id = Guid.NewGuid(), Title = "Walking in Grace", Notes = "Grace is unmerited favor...", Date = DateTime.UtcNow.AddDays(-21), PreacherName = "Pastor John", ScriptureReference = "Ephesians 2:8", UserId = admin.Id }
        };
        await context.SermonNotes.AddRangeAsync(notes);

        // Volunteer
        var opps = new[]
        {
            new VolunteerOpportunity { Id = Guid.NewGuid(), Title = "Kids Ministry Team", Description = "Help with Sunday school classes.", Category = "Kids", Date = DateTime.UtcNow.AddDays(3), Location = "Kids Wing", TotalSpots = 10, FilledSpots = 8, CreatedAt = DateTime.UtcNow },
            new VolunteerOpportunity { Id = Guid.NewGuid(), Title = "Welcome Team", Description = "Greet guests at the front door.", Category = "Hospitality", Date = DateTime.UtcNow.AddDays(3), Location = "Main Entrance", TotalSpots = 15, FilledSpots = 15, CreatedAt = DateTime.UtcNow }
        };
        await context.VolunteerOpportunities.AddRangeAsync(opps);

        await context.SaveChangesAsync();
    }
}

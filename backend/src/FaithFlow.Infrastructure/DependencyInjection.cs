using FaithFlow.Application.Common.Interfaces;
using FaithFlow.Application.Services;
using FaithFlow.Domain.Interfaces;
using FaithFlow.Infrastructure.Data;
using FaithFlow.Infrastructure.Data.Repositories;
using FaithFlow.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace FaithFlow.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration config)
    {
        // Database — use PostgreSQL in production when connection string is set, InMemory otherwise
        var connectionString = config.GetConnectionString("DefaultConnection");
        if (!string.IsNullOrEmpty(connectionString))
        {
            services.AddDbContext<FaithFlowDbContext>(options =>
                options.UseNpgsql(connectionString));
        }
        else
        {
            services.AddDbContext<FaithFlowDbContext>(options =>
                options.UseInMemoryDatabase("FaithFlowDb"));
        }

        // Repositories
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<ISermonNoteRepository, SermonNoteRepository>();
        services.AddScoped<IPrayerRequestRepository, PrayerRequestRepository>();
        services.AddScoped<IGroupRepository, GroupRepository>();
        services.AddScoped<IVolunteerRepository, VolunteerRepository>();
        services.AddScoped<ISermonSeriesRepository, SermonSeriesRepository>();

        // Infrastructure Services
        services.AddScoped<ITokenService, TokenService>();
        services.AddHttpClient<IGeminiService, GeminiService>();
        services.AddHttpClient<IBibleService, BibleService>();

        // Application Services
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<ISermonNoteService, SermonNoteService>();
        services.AddScoped<IPrayerRequestService, PrayerRequestService>();
        services.AddScoped<IGroupService, GroupService>();
        services.AddScoped<IVolunteerService, VolunteerService>();
        services.AddScoped<ISermonSeriesService, SermonSeriesService>();
        services.AddScoped<IAdminService, AdminService>();

        return services;
    }
}

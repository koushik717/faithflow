using FaithFlow.Application.Common.DTOs.Auth;
using FaithFlow.Application.Common.DTOs.Users;
using FaithFlow.Application.Common.Interfaces;
using FaithFlow.Domain.Entities;
using FaithFlow.Domain.Enums;
using FaithFlow.Domain.Exceptions;
using FaithFlow.Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace FaithFlow.Application.Services;

public class AuthService : IAuthService
{
    private readonly IUserRepository _userRepo;
    private readonly ITokenService _tokenService;
    private readonly ILogger<AuthService> _logger;

    public AuthService(IUserRepository userRepo, ITokenService tokenService, ILogger<AuthService> logger)
    {
        _userRepo = userRepo;
        _tokenService = tokenService;
        _logger = logger;
    }

    public async Task<AuthResponse> RegisterAsync(RegisterRequest request)
    {
        var existing = await _userRepo.GetByEmailAsync(request.Email);
        if (existing != null) throw new ConflictException("Email already registered.");

        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = request.Email.ToLowerInvariant(),
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
            FirstName = request.FirstName,
            LastName = request.LastName,
            Role = UserRole.Member,
            JoinedAt = DateTime.UtcNow,
            IsActive = true
        };

        await _userRepo.AddAsync(user);
        _logger.LogInformation("User registered: {Email}", user.Email);

        return GenerateAuthResponse(user);
    }

    public async Task<AuthResponse> LoginAsync(LoginRequest request)
    {
        var user = await _userRepo.GetByEmailAsync(request.Email.ToLowerInvariant());
        if (user == null || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
            throw new UnauthorizedException("Invalid email or password.");

        if (!user.IsActive)
            throw new ForbiddenException("Account is deactivated.");

        _logger.LogInformation("User logged in: {Email}", user.Email);
        return GenerateAuthResponse(user);
    }

    public async Task<AuthResponse> RefreshTokenAsync(string refreshToken)
    {
        // Find user with matching non-revoked refresh token
        var users = await _userRepo.GetAllAsync();
        User? user = null;
        RefreshToken? token = null;

        foreach (var u in users)
        {
            token = u.RefreshTokens?.FirstOrDefault(t =>
                t.Token == refreshToken && !t.IsRevoked && t.ExpiryDate > DateTime.UtcNow);
            if (token != null) { user = u; break; }
        }

        if (user == null || token == null)
            throw new UnauthorizedException("Invalid or expired refresh token.");

        // Revoke old token
        token.IsRevoked = true;
        await _userRepo.UpdateAsync(user);

        return GenerateAuthResponse(user);
    }

    public async Task LogoutAsync(Guid userId, string refreshToken)
    {
        var user = await _userRepo.GetByIdAsync(userId);
        if (user == null) return;

        var token = user.RefreshTokens?.FirstOrDefault(t => t.Token == refreshToken);
        if (token != null)
        {
            token.IsRevoked = true;
            await _userRepo.UpdateAsync(user);
        }
    }

    private AuthResponse GenerateAuthResponse(User user)
    {
        var accessToken = _tokenService.GenerateAccessToken(user);
        var refreshToken = _tokenService.GenerateRefreshToken(user.Id);
        user.RefreshTokens ??= new List<RefreshToken>();
        user.RefreshTokens.Add(refreshToken);

        return new AuthResponse
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken.Token,
            ExpiresAt = DateTime.UtcNow.AddMinutes(15),
            User = MapToDto(user)
        };
    }

    private static UserDto MapToDto(User user) => new()
    {
        Id = user.Id,
        Email = user.Email,
        FirstName = user.FirstName,
        LastName = user.LastName,
        Role = user.Role.ToString(),
        ProfileImageUrl = user.ProfileImageUrl,
        Bio = user.Bio,
        JoinedAt = user.JoinedAt
    };
}

public class UserService : IUserService
{
    private readonly IUserRepository _userRepo;

    public UserService(IUserRepository userRepo) => _userRepo = userRepo;

    public async Task<UserDto> GetProfileAsync(Guid userId)
    {
        var user = await _userRepo.GetByIdAsync(userId)
            ?? throw new NotFoundException("User", userId);
        return MapToDto(user);
    }

    public async Task<UserDto> UpdateProfileAsync(Guid userId, UpdateProfileRequest request)
    {
        var user = await _userRepo.GetByIdAsync(userId)
            ?? throw new NotFoundException("User", userId);

        if (request.FirstName != null) user.FirstName = request.FirstName;
        if (request.LastName != null) user.LastName = request.LastName;
        if (request.Bio != null) user.Bio = request.Bio;
        if (request.ProfileImageUrl != null) user.ProfileImageUrl = request.ProfileImageUrl;

        await _userRepo.UpdateAsync(user);
        return MapToDto(user);
    }

    public async Task<IEnumerable<AdminUserDto>> GetAllUsersAsync(string? query, int page, int pageSize)
    {
        var users = await _userRepo.SearchAsync(query, page, pageSize);
        return users.Select(u => new AdminUserDto
        {
            Id = u.Id, Email = u.Email, FirstName = u.FirstName, LastName = u.LastName,
            Role = u.Role.ToString(), IsActive = u.IsActive, JoinedAt = u.JoinedAt
        });
    }

    public async Task<UserDto> ChangeRoleAsync(Guid userId, string role)
    {
        var user = await _userRepo.GetByIdAsync(userId)
            ?? throw new NotFoundException("User", userId);

        if (!Enum.TryParse<UserRole>(role, true, out var newRole))
            throw new BadRequestException($"Invalid role: {role}");

        user.Role = newRole;
        await _userRepo.UpdateAsync(user);
        return MapToDto(user);
    }

    private static UserDto MapToDto(User user) => new()
    {
        Id = user.Id, Email = user.Email, FirstName = user.FirstName, LastName = user.LastName,
        Role = user.Role.ToString(), ProfileImageUrl = user.ProfileImageUrl,
        Bio = user.Bio, JoinedAt = user.JoinedAt
    };
}

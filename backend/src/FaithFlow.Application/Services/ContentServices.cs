using FaithFlow.Application.Common.DTOs.SermonNotes;
using FaithFlow.Application.Common.DTOs.PrayerRequests;
using FaithFlow.Application.Common.Interfaces;
using FaithFlow.Domain.Entities;
using FaithFlow.Domain.Exceptions;
using FaithFlow.Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace FaithFlow.Application.Services;

public class SermonNoteService : ISermonNoteService
{
    private readonly ISermonNoteRepository _noteRepo;
    private readonly IBibleService _bibleService;
    private readonly ILogger<SermonNoteService> _logger;

    public SermonNoteService(ISermonNoteRepository noteRepo, IBibleService bibleService, ILogger<SermonNoteService> logger)
    {
        _noteRepo = noteRepo;
        _bibleService = bibleService;
        _logger = logger;
    }

    public async Task<IEnumerable<SermonNoteDto>> GetMyNotesAsync(Guid userId)
    {
        var notes = await _noteRepo.GetByUserIdAsync(userId);
        return notes.Select(MapToDto);
    }

    public async Task<SermonNoteDto> GetByIdAsync(Guid userId, Guid noteId)
    {
        var note = await _noteRepo.GetByIdAsync(noteId)
            ?? throw new NotFoundException("SermonNote", noteId);
        if (note.UserId != userId) throw new ForbiddenException();
        return MapToDto(note);
    }

    public async Task<SermonNoteDto> CreateAsync(Guid userId, CreateSermonNoteRequest request)
    {
        var note = new SermonNote
        {
            Id = Guid.NewGuid(), UserId = userId, Title = request.Title,
            PreacherName = request.PreacherName, Date = request.Date, Location = request.Location,
            ScriptureReference = request.ScriptureReference, Notes = request.Notes,
            SeriesId = request.SeriesId, CreatedAt = DateTime.UtcNow
        };

        if (!string.IsNullOrWhiteSpace(request.ScriptureReference))
        {
            var scripture = await _bibleService.LookupScriptureAsync(request.ScriptureReference);
            if (scripture != null) note.ScriptureText = scripture.Text;
        }

        await _noteRepo.AddAsync(note);
        _logger.LogInformation("Sermon note created: {Title} by user {UserId}", note.Title, userId);
        return MapToDto(note);
    }

    public async Task<SermonNoteDto> UpdateAsync(Guid userId, Guid noteId, UpdateSermonNoteRequest request)
    {
        var note = await _noteRepo.GetByIdAsync(noteId)
            ?? throw new NotFoundException("SermonNote", noteId);
        if (note.UserId != userId) throw new ForbiddenException();

        if (request.Title != null) note.Title = request.Title;
        if (request.PreacherName != null) note.PreacherName = request.PreacherName;
        if (request.Date.HasValue) note.Date = request.Date.Value;
        if (request.Location != null) note.Location = request.Location;
        if (request.ScriptureReference != null)
        {
            note.ScriptureReference = request.ScriptureReference;
            var scripture = await _bibleService.LookupScriptureAsync(request.ScriptureReference);
            if (scripture != null) note.ScriptureText = scripture.Text;
        }
        if (request.Notes != null) note.Notes = request.Notes;
        if (request.SeriesId.HasValue) note.SeriesId = request.SeriesId;

        await _noteRepo.UpdateAsync(note);
        return MapToDto(note);
    }

    public async Task DeleteAsync(Guid userId, Guid noteId)
    {
        var note = await _noteRepo.GetByIdAsync(noteId)
            ?? throw new NotFoundException("SermonNote", noteId);
        if (note.UserId != userId) throw new ForbiddenException();
        await _noteRepo.DeleteAsync(note);
    }

    public async Task<IEnumerable<SermonNoteDto>> SearchAsync(Guid userId, string query)
    {
        var notes = await _noteRepo.SearchAsync(userId, query);
        return notes.Select(MapToDto);
    }

    public async Task<ScriptureLookupResponse> ScriptureLookupAsync(Guid noteId, Guid userId)
    {
        var note = await _noteRepo.GetByIdAsync(noteId)
            ?? throw new NotFoundException("SermonNote", noteId);
        if (note.UserId != userId) throw new ForbiddenException();
        if (string.IsNullOrWhiteSpace(note.ScriptureReference))
            throw new BadRequestException("No scripture reference set on this note.");

        var result = await _bibleService.LookupScriptureAsync(note.ScriptureReference)
            ?? throw new BadRequestException("Could not look up scripture reference.");

        note.ScriptureText = result.Text;
        await _noteRepo.UpdateAsync(note);
        return result;
    }

    private static SermonNoteDto MapToDto(SermonNote n) => new()
    {
        Id = n.Id, Title = n.Title, PreacherName = n.PreacherName, Date = n.Date,
        Location = n.Location, ScriptureReference = n.ScriptureReference,
        ScriptureText = n.ScriptureText, Notes = n.Notes, SeriesId = n.SeriesId,
        SeriesTitle = n.Series?.Title, CreatedAt = n.CreatedAt
    };
}

public class PrayerRequestService : IPrayerRequestService
{
    private readonly IPrayerRequestRepository _prayerRepo;
    private readonly IUserRepository _userRepo;
    private readonly IGeminiService _geminiService;
    private readonly ILogger<PrayerRequestService> _logger;

    public PrayerRequestService(IPrayerRequestRepository prayerRepo, IUserRepository userRepo,
        IGeminiService geminiService, ILogger<PrayerRequestService> logger)
    {
        _prayerRepo = prayerRepo;
        _userRepo = userRepo;
        _geminiService = geminiService;
        _logger = logger;
    }

    public async Task<IEnumerable<PrayerRequestDto>> GetPublicRequestsAsync()
    {
        var requests = await _prayerRepo.GetPublicRequestsAsync();
        var dtos = new List<PrayerRequestDto>();
        foreach (var r in requests) dtos.Add(await MapToDto(r));
        return dtos;
    }

    public async Task<IEnumerable<PrayerRequestDto>> GetMyRequestsAsync(Guid userId)
    {
        var requests = await _prayerRepo.GetByUserIdAsync(userId);
        var dtos = new List<PrayerRequestDto>();
        foreach (var r in requests) dtos.Add(await MapToDto(r));
        return dtos;
    }

    public async Task<IEnumerable<PrayerRequestDto>> GetAnsweredRequestsAsync()
    {
        var requests = await _prayerRepo.GetAnsweredAsync();
        var dtos = new List<PrayerRequestDto>();
        foreach (var r in requests) dtos.Add(await MapToDto(r));
        return dtos;
    }

    public async Task<PrayerRequestDto> CreateAsync(Guid userId, CreatePrayerRequestRequest request)
    {
        var entity = new PrayerRequest
        {
            Id = Guid.NewGuid(), UserId = userId, Title = request.Title,
            Description = request.Description, IsAnonymous = request.IsAnonymous,
            IsPublic = request.IsPublic, CreatedAt = DateTime.UtcNow
        };
        await _prayerRepo.AddAsync(entity);
        _logger.LogInformation("Prayer request created: {Title}", entity.Title);
        return await MapToDto(entity);
    }

    public async Task<PrayerRequestDto> UpdateAsync(Guid userId, Guid requestId, UpdatePrayerRequestRequest request)
    {
        var entity = await _prayerRepo.GetByIdAsync(requestId)
            ?? throw new NotFoundException("PrayerRequest", requestId);
        if (entity.UserId != userId) throw new ForbiddenException();

        if (request.Title != null) entity.Title = request.Title;
        if (request.Description != null) entity.Description = request.Description;
        if (request.IsPublic.HasValue) entity.IsPublic = request.IsPublic.Value;

        await _prayerRepo.UpdateAsync(entity);
        return await MapToDto(entity);
    }

    public async Task DeleteAsync(Guid userId, Guid requestId)
    {
        var entity = await _prayerRepo.GetByIdAsync(requestId)
            ?? throw new NotFoundException("PrayerRequest", requestId);
        if (entity.UserId != userId) throw new ForbiddenException();
        await _prayerRepo.DeleteAsync(entity);
    }

    public async Task<int> PrayAsync(Guid userId, Guid requestId)
    {
        var entity = await _prayerRepo.GetByIdAsync(requestId)
            ?? throw new NotFoundException("PrayerRequest", requestId);

        entity.PrayCount++;
        entity.PrayerInteractions.Add(new PrayerInteraction
        {
            Id = Guid.NewGuid(), UserId = userId, PrayerRequestId = requestId, PrayedAt = DateTime.UtcNow
        });
        await _prayerRepo.UpdateAsync(entity);
        return entity.PrayCount;
    }

    public async Task<AiPrayerResponse> GetAiPrayerAsync(Guid requestId)
    {
        var entity = await _prayerRepo.GetByIdAsync(requestId)
            ?? throw new NotFoundException("PrayerRequest", requestId);

        var prayer = await _geminiService.GeneratePrayerAsync(entity.Title, entity.Description);
        return new AiPrayerResponse { Prayer = prayer };
    }

    public async Task MarkAnsweredAsync(Guid userId, Guid requestId)
    {
        var entity = await _prayerRepo.GetByIdAsync(requestId)
            ?? throw new NotFoundException("PrayerRequest", requestId);
        if (entity.UserId != userId) throw new ForbiddenException();

        entity.IsAnswered = true;
        entity.AnsweredAt = DateTime.UtcNow;
        await _prayerRepo.UpdateAsync(entity);
    }

    private async Task<PrayerRequestDto> MapToDto(PrayerRequest r)
    {
        string? userName = null;
        if (!r.IsAnonymous)
        {
            var user = r.User ?? await _userRepo.GetByIdAsync(r.UserId);
            userName = user != null ? $"{user.FirstName} {user.LastName}" : null;
        }
        return new PrayerRequestDto
        {
            Id = r.Id, UserId = r.IsAnonymous ? null : r.UserId,
            UserName = userName, Title = r.Title, Description = r.Description,
            IsAnonymous = r.IsAnonymous, IsAnswered = r.IsAnswered,
            PrayCount = r.PrayCount, CreatedAt = r.CreatedAt, AnsweredAt = r.AnsweredAt
        };
    }
}

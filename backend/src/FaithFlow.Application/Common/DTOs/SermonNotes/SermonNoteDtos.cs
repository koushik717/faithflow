using System.ComponentModel.DataAnnotations;

namespace FaithFlow.Application.Common.DTOs.SermonNotes;

public class SermonNoteDto
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string PreacherName { get; set; } = string.Empty;
    public DateTime Date { get; set; }
    public string Location { get; set; } = string.Empty;
    public string? ScriptureReference { get; set; }
    public string? ScriptureText { get; set; }
    public string Notes { get; set; } = string.Empty;
    public Guid? SeriesId { get; set; }
    public string? SeriesTitle { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class CreateSermonNoteRequest
{
    [Required] public string Title { get; set; } = string.Empty;
    [Required] public string PreacherName { get; set; } = string.Empty;
    public DateTime Date { get; set; } = DateTime.UtcNow;
    public string Location { get; set; } = string.Empty;
    public string? ScriptureReference { get; set; }
    public string Notes { get; set; } = string.Empty;
    public Guid? SeriesId { get; set; }
}

public class UpdateSermonNoteRequest
{
    public string? Title { get; set; }
    public string? PreacherName { get; set; }
    public DateTime? Date { get; set; }
    public string? Location { get; set; }
    public string? ScriptureReference { get; set; }
    public string? Notes { get; set; }
    public Guid? SeriesId { get; set; }
}

public class ScriptureLookupResponse
{
    public string Reference { get; set; } = string.Empty;
    public string Text { get; set; } = string.Empty;
    public string Translation { get; set; } = string.Empty;
}

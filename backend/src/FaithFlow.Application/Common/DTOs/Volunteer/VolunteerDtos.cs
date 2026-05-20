using System.ComponentModel.DataAnnotations;

namespace FaithFlow.Application.Common.DTOs.Volunteer;

public class VolunteerOpportunityDto
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DateTime Date { get; set; }
    public string Location { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public int TotalSpots { get; set; }
    public int FilledSpots { get; set; }
    public DateTime CreatedAt { get; set; }
    public bool IsSignedUp { get; set; }
}

public class CreateOpportunityRequest
{
    [Required] public string Title { get; set; } = string.Empty;
    [Required] public string Description { get; set; } = string.Empty;
    public DateTime Date { get; set; }
    [Required] public string Location { get; set; } = string.Empty;
    [Required] public string Category { get; set; } = string.Empty;
    [Required] public int TotalSpots { get; set; }
}

public class VolunteerSignupDto
{
    public Guid Id { get; set; }
    public Guid OpportunityId { get; set; }
    public string OpportunityTitle { get; set; } = string.Empty;
    public DateTime Date { get; set; }
    public string Location { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime SignedUpAt { get; set; }
}

public class CoverageReportDto
{
    public int TotalOpportunities { get; set; }
    public int FullyCovered { get; set; }
    public int PartiallyCovered { get; set; }
    public int NoCoverage { get; set; }
    public double CoveragePercentage { get; set; }
}

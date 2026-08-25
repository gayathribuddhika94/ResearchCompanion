namespace ResearchCompanion.Data.Entities;

// Persists the answers collected by the guided intake (Module 1) — the
// structured replacement for a free-text prompt. Every downstream module
// (topic discovery, document generation) reads from this profile.
public class ResearchProfile
{
    public int ProfileId { get; set; }
    public string UserId { get; set; } = default!;
    public ApplicationUser? User { get; set; }

    public string Purpose { get; set; } = default!;
    public string InterestArea { get; set; } = default!;
    public string NoveltyType { get; set; } = default!;
    public string ExperienceLevel { get; set; } = default!;
    public string? Constraints { get; set; }
    public string? TargetTimeframe { get; set; }
    public string DesiredOutput { get; set; } = default!;
    public bool HasMentor { get; set; }
    public string Status { get; set; } = "Active";
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public List<Topic> Topics { get; set; } = new();
}

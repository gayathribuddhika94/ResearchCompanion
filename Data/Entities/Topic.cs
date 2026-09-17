
using System.ComponentModel.DataAnnotations;

namespace ResearchCompanion.Data.Entities;

public class Topic
{
    [Key]
    public int TopicId { get; set; }
    public int ProfileId { get; set; }
    public ResearchProfile? Profile { get; set; }

    public string Title { get; set; } = default!;
    public string NoveltyRationale { get; set; } = default!;
    public string FeasibilityNote { get; set; } = default!;
    public string NoveltyScore { get; set; } = default!;
    public string FeasibilityScore { get; set; } = default!;
    public bool IsSelected { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public List<TopicSeedPaper> SeedPapers { get; set; } = new();
}


using System.ComponentModel.DataAnnotations;

namespace ResearchCompanion.Data.Entities;

// One row per unique paper after de-duplication (System Outline, Section 5.3).
public class Paper
{
    [Key]
    public int PaperId { get; set; }
    public string? Doi { get; set; }
    public string Title { get; set; } = default!;
    public string Authors { get; set; } = default!;
    public int? Year { get; set; }
    public string SourceDatabase { get; set; } = default!;
    public string? Abstract { get; set; }
    public int CitationCount { get; set; }
    public string? Url { get; set; }
}

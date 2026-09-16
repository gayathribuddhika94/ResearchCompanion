using System.ComponentModel.DataAnnotations;

namespace ResearchCompanion.Data.Entities;

public class ResearchDocument
{
    [Key]
    public int DocumentId { get; set; }
    public string UserId { get; set; } = default!;
    public int ProfileId { get; set; }
    public string Title { get; set; } = "Untitled document";
    public string DocumentType { get; set; } = "Proposal";
    public string Status { get; set; } = "Draft";
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Path (relative to ContentRoot) of the generated .docx on disk. Generation
    // saves the file here rather than streaming it straight to the browser, so
    // the researcher can review it in-system first and download it later from
    // the Documents library, rather than every "Generate" click forcing an
    // immediate browser download.
    public string? FilePath { get; set; }

    public List<DocumentSection> Sections { get; set; } = new();
}

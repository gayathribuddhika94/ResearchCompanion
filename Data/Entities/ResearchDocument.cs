namespace ResearchCompanion.Data.Entities;

public class ResearchDocument
{
    public int DocumentId { get; set; }
    public string UserId { get; set; } = default!;
    public int ProfileId { get; set; }
    public string DocumentType { get; set; } = "Proposal";
    public string Status { get; set; } = "Draft";
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public List<DocumentSection> Sections { get; set; } = new();
}

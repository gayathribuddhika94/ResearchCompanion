using System.ComponentModel.DataAnnotations;

namespace ResearchCompanion.Data.Entities;

public class DocumentSection
{
    [Key]
    public int SectionId { get; set; }

    public int DocumentId { get; set; }
    public ResearchDocument? Document { get; set; }

    public string SectionName { get; set; } = default!;
    public int OrderIndex { get; set; }
    public string Content { get; set; } = default!;
    public string Status { get; set; } = "Drafted";
}

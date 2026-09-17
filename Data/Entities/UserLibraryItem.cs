using System.ComponentModel.DataAnnotations;

namespace ResearchCompanion.Data.Entities;

// A researcher's personal saved-paper list (System Outline, Module 3).
public class UserLibraryItem
{
    [Key]
    public int LibraryId { get; set; }
    public string UserId { get; set; } = default!;

    public int PaperId { get; set; }
    public Paper? Paper { get; set; }

    public string? Tags { get; set; }

    // Free-text personal notes the researcher keeps against a saved paper —
    // requested directly by the user, separate from the paper's own abstract.
    public string? Notes { get; set; }

    // Lets the researcher curate which saved papers actually belong in the
    // literature review / generated References list, rather than every
    // saved paper automatically being cited. Defaults to true so existing
    // saved papers behave the same as before this field was added.
    public bool IncludeInLiteratureReview { get; set; } = true;

    public DateTime SavedAt { get; set; } = DateTime.UtcNow;
}

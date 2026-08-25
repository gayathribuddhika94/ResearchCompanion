namespace ResearchCompanion.Data.Entities;

// A researcher's personal saved-paper list (System Outline, Module 3).
public class UserLibraryItem
{
    public int LibraryId { get; set; }
    public string UserId { get; set; } = default!;

    public int PaperId { get; set; }
    public Paper? Paper { get; set; }

    public string? Tags { get; set; }
    public string? Notes { get; set; }
    public DateTime SavedAt { get; set; } = DateTime.UtcNow;
}

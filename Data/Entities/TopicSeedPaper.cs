namespace ResearchCompanion.Data.Entities;

// Join entity linking a suggested Topic to the papers that justify it
// (System Outline, Section 5.2 — "TopicSeedPapers").
public class TopicSeedPaper
{
    public int TopicId { get; set; }
    public Topic? Topic { get; set; }

    public int PaperId { get; set; }
    public Paper? Paper { get; set; }
}

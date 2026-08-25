using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using ResearchCompanion.Data;
using ResearchCompanion.Data.Entities;
using ResearchCompanion.Services;

namespace ResearchCompanion.Pages.Documents;

[Authorize]
public class GenerateProposalModel : PageModel
{
    private readonly AppDbContext _db;
    private readonly DocumentRenderer _renderer;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IWebHostEnvironment _env;

    public GenerateProposalModel(AppDbContext db, DocumentRenderer renderer, UserManager<ApplicationUser> userManager, IWebHostEnvironment env)
    {
        _db = db;
        _renderer = renderer;
        _userManager = userManager;
        _env = env;
    }

    public int ProfileId { get; set; }
    public string SelectedTopicTitle { get; set; } = "(no topic selected yet)";
    public int LibraryCount { get; set; }

    public async Task<IActionResult> OnGetAsync(int profileId)
    {
        ProfileId = profileId;
        var userId = _userManager.GetUserId(User)!;

        var topic = await _db.Topics.FirstOrDefaultAsync(t => t.ProfileId == profileId && t.IsSelected);
        if (topic != null) SelectedTopicTitle = topic.Title;

        LibraryCount = await _db.UserLibrary.CountAsync(l => l.UserId == userId);
        return Page();
    }

    public async Task<IActionResult> OnPostAsync(int profileId)
    {
        var userId = _userManager.GetUserId(User)!;
        var profile = await _db.ResearchProfiles.FirstOrDefaultAsync(p => p.ProfileId == profileId);
        if (profile == null) return RedirectToPage("/Index");

        var topic = await _db.Topics.FirstOrDefaultAsync(t => t.ProfileId == profileId && t.IsSelected);
        var savedPapers = await _db.UserLibrary
            .Include(l => l.Paper)
            .Where(l => l.UserId == userId)
            .Select(l => l.Paper!)
            .ToListAsync();

        var literatureReview = savedPapers.Count > 0
            ? string.Join(" ", savedPapers.Select((p, i) =>
                $"{p.Authors} ({p.Year}) found that {(string.IsNullOrWhiteSpace(p.Abstract) ? p.Title : p.Abstract!.Substring(0, Math.Min(180, p.Abstract.Length)))}... [{i + 1}]"))
            : "No papers have been saved to the library yet — add some from the Literature Library page before generating a full draft.";

        var tokens = new Dictionary<string, string>
        {
            ["Background"] = $"This research is motivated by {profile.InterestArea}, undertaken as a {profile.Purpose.ToLowerInvariant()}.",
            ["ProblemStatement"] = $"There is a need to address {profile.InterestArea}, particularly from a {profile.NoveltyType.ToLowerInvariant()} perspective.",
            ["AimObjectives"] = $"To investigate {topic?.Title ?? profile.InterestArea} and deliver: {profile.DesiredOutput.ToLowerInvariant()}.",
            ["LiteratureReview"] = literatureReview,
            ["Methodology"] = "A design science research approach will be followed, combining prototype development with a mixed-methods evaluation.",
            ["WorkPlan"] = $"Estimated timeframe: {profile.TargetTimeframe ?? "to be confirmed with supervisor"}.",
            ["References"] = savedPapers.Count > 0
                ? string.Join(" ", savedPapers.Select((p, i) => $"[{i + 1}] {p.Authors}, \"{p.Title}\", {p.Year}."))
                : "(no saved papers to cite yet)"
        };

        var templatePath = Path.Combine(_env.ContentRootPath, "Templates", "Proposal_Template.docx");
        var bytes = _renderer.Render(templatePath, tokens);

        var document = new ResearchDocument { UserId = userId, ProfileId = profileId, DocumentType = "Proposal", Status = "Drafted" };
        document.Sections = tokens.Select((kv, i) => new DocumentSection
        {
            SectionName = kv.Key,
            OrderIndex = i,
            Content = kv.Value,
            Status = "Drafted"
        }).ToList();

        _db.Documents.Add(document);
        await _db.SaveChangesAsync();

        return File(bytes, "application/vnd.openxmlformats-officedocument.wordprocessingml.document", "Proposal.docx");
    }
}

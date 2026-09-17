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
    private readonly CitationFormatterService _citations;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IWebHostEnvironment _env;

    public GenerateProposalModel(AppDbContext db, DocumentRenderer renderer, CitationFormatterService citations,
        UserManager<ApplicationUser> userManager, IWebHostEnvironment env)
    {
        _db = db;
        _renderer = renderer;
        _citations = citations;
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

        // Only papers the researcher has explicitly kept ticked for the
        // literature review are cited — everything else stays in the
        // library as reference material without being pulled into the
        // generated document (Library/Index.cshtml "Include in review?").
        var citedPapers = await _db.UserLibrary
            .Include(l => l.Paper)
            .Where(l => l.UserId == userId && l.IncludeInLiteratureReview)
            .Select(l => l.Paper!)
            .ToListAsync();

        var literatureReview = citedPapers.Count > 0
            ? string.Join(" ", citedPapers.Select((p, i) =>
                $"{p.Authors} ({p.Year}) found that {(string.IsNullOrWhiteSpace(p.Abstract) ? p.Title : p.Abstract!.Substring(0, Math.Min(180, p.Abstract.Length)))}... [{i + 1}]"))
            : "No papers are currently marked \"include in literature review\" — tick at least one saved paper on the Literature Library page before generating a full draft.";

        var referencesIeee = _citations.FormatBibliographyIeee(citedPapers);

        var topicTitle = topic?.Title ?? profile.InterestArea;
        var tokens = new Dictionary<string, string>
        {
            ["Background"] = $"This research is motivated by {profile.InterestArea}, undertaken as a {profile.Purpose.ToLowerInvariant()}.",
            ["ProblemStatement"] = $"There is a need to address {profile.InterestArea}, particularly from a {profile.NoveltyType.ToLowerInvariant()} perspective.",
            ["AimObjectives"] = $"To investigate {topicTitle} and deliver: {profile.DesiredOutput.ToLowerInvariant()}.",
            ["LiteratureReview"] = literatureReview,
            ["Methodology"] = "A design science research approach will be followed, combining prototype development with a mixed-methods evaluation.",
            ["WorkPlan"] = $"Estimated timeframe: {profile.TargetTimeframe ?? "to be confirmed with supervisor"}.",
            ["References"] = referencesIeee
        };

        var templatePath = Path.Combine(_env.ContentRootPath, "Templates", "Proposal_Template.docx");
        var bytes = _renderer.Render(templatePath, tokens);

        // Save the rendered file to disk under ContentRoot (outside wwwroot,
        // so it isn't directly browsable by URL) instead of streaming it
        // straight back as an immediate download. The researcher reviews and
        // downloads it from the Documents library instead — see
        // Pages/Documents/Index.cshtml.
        var storageDir = Path.Combine(_env.ContentRootPath, "GeneratedDocuments", userId);
        Directory.CreateDirectory(storageDir);
        var fileName = $"Proposal_{DateTime.UtcNow:yyyyMMdd_HHmmss}.docx";
        var absolutePath = Path.Combine(storageDir, fileName);
        await System.IO.File.WriteAllBytesAsync(absolutePath, bytes);
        var relativePath = Path.Combine("GeneratedDocuments", userId, fileName);

        var document = new ResearchDocument
        {
            UserId = userId,
            ProfileId = profileId,
            Title = $"Proposal — {topicTitle}",
            DocumentType = "Proposal",
            Status = "Drafted",
            FilePath = relativePath
        };
        document.Sections = tokens.Select((kv, i) => new DocumentSection
        {
            SectionName = kv.Key,
            OrderIndex = i,
            Content = kv.Value,
            Status = "Drafted"
        }).ToList();

        _db.Documents.Add(document);
        await _db.SaveChangesAsync();

        TempData["Message"] = "Your proposal has been generated and saved. Review it below, and download it whenever you're ready.";
        return RedirectToPage("/Documents/Index");
    }
}

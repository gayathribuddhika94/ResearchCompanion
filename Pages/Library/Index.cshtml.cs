using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using ResearchCompanion.Data;
using ResearchCompanion.Data.Entities;
using ResearchCompanion.Services;

namespace ResearchCompanion.Pages.Library;

[Authorize]
public class IndexModel : PageModel
{
    private readonly AppDbContext _db;
    private readonly LiteratureAggregationService _aggregation;
    private readonly UserManager<ApplicationUser> _userManager;

    public IndexModel(AppDbContext db, LiteratureAggregationService aggregation, UserManager<ApplicationUser> userManager)
    {
        _db = db;
        _aggregation = aggregation;
        _userManager = userManager;
    }

    public string? Query { get; set; }
    public List<Paper> Results { get; set; } = new();
    public List<UserLibraryItem> SavedItems { get; set; } = new();

    public async Task OnGetAsync(string? q)
    {
        Query = q;
        var userId = _userManager.GetUserId(User)!;

        if (!string.IsNullOrWhiteSpace(q))
        {
            Results = await _aggregation.SearchAndPersistAsync(q);
        }

        SavedItems = await _db.UserLibrary
            .Include(l => l.Paper)
            .Where(l => l.UserId == userId)
            .OrderByDescending(l => l.SavedAt)
            .ToListAsync();
    }

    public async Task<IActionResult> OnPostSaveAsync(int paperId, string? q)
    {
        var userId = _userManager.GetUserId(User)!;
        var alreadySaved = await _db.UserLibrary.AnyAsync(l => l.UserId == userId && l.PaperId == paperId);
        if (!alreadySaved)
        {
            _db.UserLibrary.Add(new UserLibraryItem { UserId = userId, PaperId = paperId });
            await _db.SaveChangesAsync();
        }
        return RedirectToPage(new { q });
    }

    // Updates a saved paper's personal notes and whether it should be pulled
    // into the generated literature review / References list. One handler
    // covers both fields since they're edited from the same row's form.
    public async Task<IActionResult> OnPostUpdateAsync(int libraryId, string? notes, bool includeInLiteratureReview, string? q)
    {
        var userId = _userManager.GetUserId(User)!;
        var item = await _db.UserLibrary.FirstOrDefaultAsync(l => l.LibraryId == libraryId && l.UserId == userId);
        if (item != null)
        {
            item.Notes = notes;
            item.IncludeInLiteratureReview = includeInLiteratureReview;
            await _db.SaveChangesAsync();
        }
        return RedirectToPage(new { q });
    }
}

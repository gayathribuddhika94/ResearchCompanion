using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using ResearchCompanion.Data;
using ResearchCompanion.Data.Entities;

namespace ResearchCompanion.Pages;

public class IndexModel : PageModel
{
    private readonly AppDbContext _db;
    private readonly UserManager<ApplicationUser> _userManager;

    public IndexModel(AppDbContext db, UserManager<ApplicationUser> userManager)
    {
        _db = db;
        _userManager = userManager;
    }

    public bool IsAuthenticated { get; set; }
    public string? FullName { get; set; }
    public int ProfileCount { get; set; }
    public int LibraryCount { get; set; }
    public int DocumentCount { get; set; }
    public List<ResearchProfile> Profiles { get; set; } = new();

    public async Task OnGetAsync()
    {
        IsAuthenticated = User.Identity?.IsAuthenticated ?? false;
        if (!IsAuthenticated) return;

        var userId = _userManager.GetUserId(User)!;
        FullName = User.Identity!.Name;

        Profiles = await _db.ResearchProfiles
            .Where(p => p.UserId == userId)
            .OrderByDescending(p => p.CreatedAt)
            .ToListAsync();

        ProfileCount = Profiles.Count;
        LibraryCount = await _db.UserLibrary.CountAsync(l => l.UserId == userId);
        DocumentCount = await _db.Documents.CountAsync(d => d.UserId == userId);
    }
}

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using ResearchCompanion.Data;
using ResearchCompanion.Data.Entities;
using ResearchCompanion.Services;

namespace ResearchCompanion.Pages.Topics;

[Authorize]
public class SuggestionsModel : PageModel
{
    private readonly AppDbContext _db;
    private readonly TopicDiscoveryService _discovery;

    public SuggestionsModel(AppDbContext db, TopicDiscoveryService discovery)
    {
        _db = db;
        _discovery = discovery;
    }

    public int ProfileId { get; set; }
    public ResearchProfile? Profile { get; set; }
    public List<Topic> Topics { get; set; } = new();

    public async Task<IActionResult> OnGetAsync(int profileId)
    {
        ProfileId = profileId;
        Profile = await _db.ResearchProfiles.FirstOrDefaultAsync(p => p.ProfileId == profileId);
        if (Profile == null) return RedirectToPage("/Index");

        Topics = await _db.Topics.Where(t => t.ProfileId == profileId).ToListAsync();
        if (!Topics.Any())
        {
            Topics = await _discovery.DiscoverAsync(Profile);
            _db.Topics.AddRange(Topics);
            await _db.SaveChangesAsync();
        }
        return Page();
    }

    public async Task<IActionResult> OnPostAsync(int topicId, int profileId)
    {
        var topics = await _db.Topics.Where(t => t.ProfileId == profileId).ToListAsync();
        foreach (var t in topics) t.IsSelected = t.TopicId == topicId;
        await _db.SaveChangesAsync();

        return RedirectToPage("/Documents/GenerateProposal", new { profileId });
    }
}

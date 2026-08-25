using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ResearchCompanion.Data;
using ResearchCompanion.Data.Entities;

namespace ResearchCompanion.Pages.Intake;

[Authorize]
public class IndexModel : PageModel
{
    private readonly AppDbContext _db;
    private readonly UserManager<ApplicationUser> _userManager;

    public IndexModel(AppDbContext db, UserManager<ApplicationUser> userManager)
    {
        _db = db;
        _userManager = userManager;
    }

    [BindProperty]
    public InputModel Input { get; set; } = new();

    public class InputModel
    {
        [Required] public string Purpose { get; set; } = "Degree requirement";
        [Required] public string InterestArea { get; set; } = default!;
        [Required] public string NoveltyType { get; set; } = "Not sure yet";
        [Required] public string ExperienceLevel { get; set; } = "Intermediate";
        public string? Constraints { get; set; }
        public string? TargetTimeframe { get; set; }
        [Required] public string DesiredOutput { get; set; } = "Just a topic";
        public bool HasMentor { get; set; }
    }

    public void OnGet() { }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid) return Page();

        var profile = new ResearchProfile
        {
            UserId = _userManager.GetUserId(User)!,
            Purpose = Input.Purpose,
            InterestArea = Input.InterestArea,
            NoveltyType = Input.NoveltyType,
            ExperienceLevel = Input.ExperienceLevel,
            Constraints = Input.Constraints,
            TargetTimeframe = Input.TargetTimeframe,
            DesiredOutput = Input.DesiredOutput,
            HasMentor = Input.HasMentor
        };

        _db.ResearchProfiles.Add(profile);
        await _db.SaveChangesAsync();

        return RedirectToPage("/Topics/Suggestions", new { profileId = profile.ProfileId });
    }
}

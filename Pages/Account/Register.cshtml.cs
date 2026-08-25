using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ResearchCompanion.Data.Entities;

namespace ResearchCompanion.Pages.Account;

public class RegisterModel : PageModel
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;

    public RegisterModel(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager)
    {
        _userManager = userManager;
        _signInManager = signInManager;
    }

    [BindProperty]
    public InputModel Input { get; set; } = new();

    public class InputModel
    {
        [Required] public string FullName { get; set; } = default!;
        [Required, EmailAddress] public string Email { get; set; } = default!;
        [Required, DataType(DataType.Password)] public string Password { get; set; } = default!;
        [Required, DataType(DataType.Password), Compare(nameof(Password))] public string ConfirmPassword { get; set; } = default!;
        [Required] public string ResearcherType { get; set; } = "Postgraduate (MSc/MPhil)";
    }

    public void OnGet() { }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid) return Page();

        var user = new ApplicationUser
        {
            UserName = Input.Email,
            Email = Input.Email,
            FullName = Input.FullName,
            ResearcherType = Input.ResearcherType
        };

        var result = await _userManager.CreateAsync(user, Input.Password);
        if (!result.Succeeded)
        {
            foreach (var error in result.Errors) ModelState.AddModelError(string.Empty, error.Description);
            return Page();
        }

        await _userManager.AddToRoleAsync(user, "Researcher");
        await _signInManager.SignInAsync(user, isPersistent: false);
        return RedirectToPage("/Intake/Index");
    }
}

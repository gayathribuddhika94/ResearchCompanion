using Microsoft.AspNetCore.Identity;

namespace ResearchCompanion.Data.Entities;

public class ApplicationUser : IdentityUser
{
    public string? FullName { get; set; }

    // Set at registration; drives how the Intake and Topic Discovery modules
    // calibrate suggestions (System Outline, Section 7, Module 1).
    public string? ResearcherType { get; set; }
}

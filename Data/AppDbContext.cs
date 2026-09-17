using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using ResearchCompanion.Data.Entities;

namespace ResearchCompanion.Data;

public class AppDbContext : IdentityDbContext<ApplicationUser>
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<ResearchProfile> ResearchProfiles => Set<ResearchProfile>();
    public DbSet<Topic> Topics => Set<Topic>();
    public DbSet<TopicSeedPaper> TopicSeedPapers => Set<TopicSeedPaper>();
    public DbSet<Paper> Papers => Set<Paper>();
    public DbSet<UserLibraryItem> UserLibrary => Set<UserLibraryItem>();
    public DbSet<ResearchDocument> Documents => Set<ResearchDocument>();
    public DbSet<DocumentSection> DocumentSections => Set<DocumentSection>();
    public DbSet<MendeleyAccount> MendeleyAccounts => Set<MendeleyAccount>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<ResearchProfile>()
            .HasMany(p => p.Topics)
            .WithOne(t => t.Profile!)
            .HasForeignKey(t => t.ProfileId);

        builder.Entity<TopicSeedPaper>()
            .HasKey(tsp => new { tsp.TopicId, tsp.PaperId });

        builder.Entity<TopicSeedPaper>()
            .HasOne(tsp => tsp.Topic)
            .WithMany(t => t.SeedPapers)
            .HasForeignKey(tsp => tsp.TopicId);

        builder.Entity<TopicSeedPaper>()
            .HasOne(tsp => tsp.Paper)
            .WithMany()
            .HasForeignKey(tsp => tsp.PaperId);

        builder.Entity<ResearchDocument>()
            .HasMany(d => d.Sections)
            .WithOne(s => s.Document!)
            .HasForeignKey(s => s.DocumentId);

        builder.Entity<Paper>()
            .HasIndex(p => p.Doi);

        // One Mendeley connection per researcher — connecting again updates
        // the same row rather than creating a duplicate.
        builder.Entity<MendeleyAccount>()
            .HasIndex(m => m.UserId)
            .IsUnique();
    }
}

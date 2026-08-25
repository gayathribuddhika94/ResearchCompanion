using ResearchCompanion.Data.Entities;

namespace ResearchCompanion.Services;

// Prototype-level heuristic scoring. Real clustering / semantic novelty
// modelling is listed as future work (System Outline, Section 12) — this
// produces evidence-backed candidate topics from the aggregated search
// results rather than from a free-text prompt.
public class TopicDiscoveryService
{
    private readonly LiteratureAggregationService _aggregation;

    public TopicDiscoveryService(LiteratureAggregationService aggregation)
    {
        _aggregation = aggregation;
    }

    public async Task<List<Topic>> DiscoverAsync(ResearchProfile profile, CancellationToken ct = default)
    {
        var papers = await _aggregation.SearchAndPersistAsync(profile.InterestArea, ct);

        var novelty = papers.Count <= 3 ? "High" : papers.Count <= 8 ? "Medium" : "Low";
        var feasibility = profile.ExperienceLevel == "Beginner" ? "Medium" : "High";
        var sources = papers.Select(p => p.SourceDatabase).Distinct().ToList();
        var seedPapers = papers.OrderByDescending(p => p.CitationCount).Take(5).ToList();

        var variants = new[]
        {
            $"{profile.NoveltyType}: {profile.InterestArea}",
            $"A comparative study of approaches to {profile.InterestArea}",
            $"{profile.InterestArea}: an application to an underexplored context"
        };

        return variants.Select(title => new Topic
        {
            ProfileId = profile.ProfileId,
            Title = title,
            NoveltyRationale = $"Based on {papers.Count} matching paper(s) found across " +
                $"{(sources.Count > 0 ? string.Join(", ", sources) : "no sources")}, " +
                $"this angle reflects a {novelty.ToLowerInvariant()}-novelty opportunity for the stated interest in {profile.InterestArea}.",
            FeasibilityNote = $"Estimated feasibility for a {profile.ExperienceLevel.ToLowerInvariant()}-level researcher " +
                $"with the stated constraints ({profile.Constraints ?? "none specified"}).",
            NoveltyScore = novelty,
            FeasibilityScore = feasibility,
            SeedPapers = seedPapers.Select(p => new TopicSeedPaper { PaperId = p.PaperId }).ToList()
        }).ToList();
    }
}

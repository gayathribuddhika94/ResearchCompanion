using Microsoft.EntityFrameworkCore;
using ResearchCompanion.Data;
using ResearchCompanion.Data.Entities;

namespace ResearchCompanion.Services;

// Fans a query out to every registered database, de-duplicates the results,
// and persists any new papers so the calling page always has a real PaperId
// to save against (System Outline, Module 3).
public class LiteratureAggregationService
{
    private readonly IEnumerable<IExternalSearchProvider> _providers;
    private readonly AppDbContext _db;

    public LiteratureAggregationService(IEnumerable<IExternalSearchProvider> providers, AppDbContext db)
    {
        _providers = providers;
        _db = db;
    }

    public async Task<List<Paper>> SearchAndPersistAsync(string query, CancellationToken ct = default)
    {
        var tasks = _providers.Select(p => p.SearchAsync(query, ct));
        var results = (await Task.WhenAll(tasks)).SelectMany(r => r).ToList();
        return await PersistAsync(results, ct);
    }

    // Shared find-or-create/de-duplication path, factored out so any source
    // of candidate papers, a live multi-database search (above) or an import
    // from an external reference manager such as Mendeley, ends up with the
    // same de-duplication guarantees and real PaperId values.
    public async Task<List<Paper>> PersistAsync(List<Paper> papers, CancellationToken ct = default)
    {
        var deduped = Deduplicate(papers);

        var persisted = new List<Paper>();
        foreach (var paper in deduped)
        {
            var existing = await FindExistingAsync(paper);
            if (existing != null)
            {
                persisted.Add(existing);
            }
            else
            {
                _db.Papers.Add(paper);
                persisted.Add(paper);
            }
        }
        await _db.SaveChangesAsync(ct);
        return persisted;
    }

    // DOI match first; falls back to a normalised-title match when a source
    // (commonly ACM/IEEE-only-indexed results) doesn't return a DOI. A full
    // fuzzy/Levenshtein match is listed as a future enhancement, not required
    // for the MSc-scope prototype (Technical Design, Section 5.3).
    public List<Paper> Deduplicate(List<Paper> papers)
    {
        return papers
            .GroupBy(p => !string.IsNullOrWhiteSpace(p.Doi)
                ? "doi:" + p.Doi!.Trim().ToLowerInvariant()
                : "title:" + p.Title.Trim().ToLowerInvariant())
            .Select(g => g.OrderByDescending(p => p.CitationCount).First())
            .ToList();
    }

    private async Task<Paper?> FindExistingAsync(Paper paper)
    {
        if (!string.IsNullOrWhiteSpace(paper.Doi))
        {
            var byDoi = await _db.Papers.FirstOrDefaultAsync(p => p.Doi == paper.Doi);
            if (byDoi != null) return byDoi;
        }
        return await _db.Papers.FirstOrDefaultAsync(p => p.Title == paper.Title);
    }
}

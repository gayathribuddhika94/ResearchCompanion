using ResearchCompanion.Data.Entities;

namespace ResearchCompanion.Services;

// Every academic database is wrapped behind this same interface so the
// aggregation and topic-discovery services never need to know which
// source they're talking to. See Technical Design, Section 7.
public interface IExternalSearchProvider
{
    string SourceName { get; }
    Task<IReadOnlyList<Paper>> SearchAsync(string query, CancellationToken ct = default);
}

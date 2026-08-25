using System.Net.Http.Json;
using System.Text.Json.Serialization;
using ResearchCompanion.Data.Entities;

namespace ResearchCompanion.Services;

// Semantic Scholar's public Graph API needs no key for light, prototype-scale use.
public class SemanticScholarClient : IExternalSearchProvider
{
    private readonly HttpClient _http;
    public string SourceName => "Semantic Scholar";

    public SemanticScholarClient(HttpClient http) => _http = http;

    public async Task<IReadOnlyList<Paper>> SearchAsync(string query, CancellationToken ct = default)
    {
        try
        {
            var url = $"paper/search?query={Uri.EscapeDataString(query)}" +
                      "&fields=title,authors,year,abstract,citationCount,externalIds,url&limit=10";
            var result = await _http.GetFromJsonAsync<SearchResponse>(url, ct);
            if (result?.Data == null) return Array.Empty<Paper>();

            return result.Data.Select(p => new Paper
            {
                Doi = p.ExternalIds?.Doi,
                Title = p.Title ?? "(untitled)",
                Authors = p.Authors != null && p.Authors.Count > 0
                    ? string.Join(", ", p.Authors.Select(a => a.Name))
                    : "Unknown",
                Year = p.Year,
                SourceDatabase = SourceName,
                Abstract = p.Abstract,
                CitationCount = p.CitationCount ?? 0,
                Url = p.Url
            }).ToList();
        }
        catch (HttpRequestException)
        {
            // API unreachable — return no results rather than failing the whole search.
            return Array.Empty<Paper>();
        }
    }

    private class SearchResponse
    {
        [JsonPropertyName("data")]
        public List<SsPaper>? Data { get; set; }
    }

    private class SsPaper
    {
        [JsonPropertyName("title")] public string? Title { get; set; }
        [JsonPropertyName("year")] public int? Year { get; set; }
        [JsonPropertyName("abstract")] public string? Abstract { get; set; }
        [JsonPropertyName("citationCount")] public int? CitationCount { get; set; }
        [JsonPropertyName("url")] public string? Url { get; set; }
        [JsonPropertyName("authors")] public List<SsAuthor>? Authors { get; set; }
        [JsonPropertyName("externalIds")] public SsExternalIds? ExternalIds { get; set; }
    }

    private class SsAuthor
    {
        [JsonPropertyName("name")] public string Name { get; set; } = "Unknown";
    }

    private class SsExternalIds
    {
        [JsonPropertyName("DOI")] public string? Doi { get; set; }
    }
}

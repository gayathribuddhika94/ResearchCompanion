using System.Net.Http.Json;
using System.Text.Json.Serialization;
using ResearchCompanion.Data.Entities;

namespace ResearchCompanion.Services;

// CrossRef is free and useful mainly for DOI resolution / metadata backfill
// during de-duplication (Technical Design, Section 5.3).
public class CrossRefClient : IExternalSearchProvider
{
    private readonly HttpClient _http;
    public string SourceName => "CrossRef";

    public CrossRefClient(HttpClient http) => _http = http;

    public async Task<IReadOnlyList<Paper>> SearchAsync(string query, CancellationToken ct = default)
    {
        try
        {
            var url = $"works?query={Uri.EscapeDataString(query)}&rows=10";
            var result = await _http.GetFromJsonAsync<CrossRefResponse>(url, ct);
            var items = result?.Message?.Items;
            if (items == null) return Array.Empty<Paper>();

            return items.Select(i => new Paper
            {
                Doi = i.Doi,
                Title = i.Title != null && i.Title.Count > 0 ? i.Title[0] : "(untitled)",
                Authors = i.Author != null && i.Author.Count > 0
                    ? string.Join(", ", i.Author.Select(a => $"{a.Given} {a.Family}".Trim()))
                    : "Unknown",
                Year = i.Published?.DateParts != null && i.Published.DateParts.Count > 0 && i.Published.DateParts[0].Count > 0
                    ? i.Published.DateParts[0][0]
                    : null,
                SourceDatabase = SourceName,
                Abstract = null,
                CitationCount = i.IsReferencedByCount ?? 0,
                Url = i.Url
            }).ToList();
        }
        catch (HttpRequestException)
        {
            return Array.Empty<Paper>();
        }
    }

    private class CrossRefResponse
    {
        [JsonPropertyName("message")] public CrossRefMessage? Message { get; set; }
    }

    private class CrossRefMessage
    {
        [JsonPropertyName("items")] public List<CrossRefItem>? Items { get; set; }
    }

    private class CrossRefItem
    {
        [JsonPropertyName("title")] public List<string>? Title { get; set; }
        [JsonPropertyName("author")] public List<CrossRefAuthor>? Author { get; set; }
        [JsonPropertyName("DOI")] public string? Doi { get; set; }
        [JsonPropertyName("URL")] public string? Url { get; set; }
        [JsonPropertyName("is-referenced-by-count")] public int? IsReferencedByCount { get; set; }
        [JsonPropertyName("published")] public CrossRefDate? Published { get; set; }
    }

    private class CrossRefAuthor
    {
        [JsonPropertyName("given")] public string? Given { get; set; }
        [JsonPropertyName("family")] public string? Family { get; set; }
    }

    private class CrossRefDate
    {
        [JsonPropertyName("date-parts")] public List<List<int>>? DateParts { get; set; }
    }
}

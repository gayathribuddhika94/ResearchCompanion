using System.Xml.Linq;
using ResearchCompanion.Data.Entities;

namespace ResearchCompanion.Services;

// arXiv has no JSON API — it returns an Atom XML feed, parsed here with XDocument.
public class ArxivClient : IExternalSearchProvider
{
    private readonly HttpClient _http;
    private static readonly XNamespace Atom = "http://www.w3.org/2005/Atom";
    public string SourceName => "arXiv";

    public ArxivClient(HttpClient http) => _http = http;

    public async Task<IReadOnlyList<Paper>> SearchAsync(string query, CancellationToken ct = default)
    {
        try
        {
            var url = $"api/query?search_query=all:{Uri.EscapeDataString(query)}&start=0&max_results=10";
            var xml = await _http.GetStringAsync(url, ct);
            var doc = XDocument.Parse(xml);

            var papers = new List<Paper>();
            foreach (var entry in doc.Descendants(Atom + "entry"))
            {
                var title = entry.Element(Atom + "title")?.Value?.Trim().Replace("\n", " ");
                var summary = entry.Element(Atom + "summary")?.Value?.Trim();
                var id = entry.Element(Atom + "id")?.Value;
                var published = entry.Element(Atom + "published")?.Value;

                int? year = null;
                if (DateTime.TryParse(published, out var dt)) year = dt.Year;

                var authors = entry.Elements(Atom + "author")
                    .Select(a => a.Element(Atom + "name")?.Value)
                    .Where(n => !string.IsNullOrWhiteSpace(n))
                    .ToList();

                papers.Add(new Paper
                {
                    Doi = null,
                    Title = string.IsNullOrWhiteSpace(title) ? "(untitled)" : title,
                    Authors = authors.Count > 0 ? string.Join(", ", authors) : "Unknown",
                    Year = year,
                    SourceDatabase = SourceName,
                    Abstract = summary,
                    CitationCount = 0,
                    Url = id
                });
            }
            return papers;
        }
        catch (Exception ex) when (ex is HttpRequestException or System.Xml.XmlException)
        {
            return Array.Empty<Paper>();
        }
    }
}

using Microsoft.Extensions.Logging;
using System.Text.RegularExpressions;

namespace AITrainingArena.BattleEngine;

/// <summary>
/// Search engine that queries local IPFS for documents, falls back to Wikipedia API.
/// Used by ProposerService and SolverService for multi-hop knowledge retrieval.
/// Phase 1: Wikipedia API fallback. Phase 2: IPFS-native full-text search.
/// </summary>
public sealed class IPFSSearchEngine
{
    private static readonly Uri WikipediaApiBase = new("https://en.wikipedia.org/w/api.php");

    private readonly HttpClient _httpClient;
    private readonly ILogger<IPFSSearchEngine> _logger;

    public IPFSSearchEngine(HttpClient httpClient, ILogger<IPFSSearchEngine> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    /// <summary>
    /// Searches for documents matching the query.
    /// Falls back to Wikipedia API if IPFS has no results.
    /// Returns up to maxResults document snippets.
    /// </summary>
    public async Task<IReadOnlyList<string>> SearchAsync(
        string query, int maxResults = 5, CancellationToken ct = default)
    {
        _logger.LogDebug("Searching for: {Query}", query);
        var results = await SearchWikipediaAsync(query, maxResults, ct);
        _logger.LogDebug("Found {Count} results for: {Query}", results.Count, query);
        return results;
    }

    /// <summary>Fetches a document from IPFS by CID. Returns empty string if unavailable.</summary>
    public async Task<string> GetDocumentAsync(string cid, CancellationToken ct = default)
    {
        _logger.LogDebug("Fetching IPFS document: {Cid}", cid);
        await Task.CompletedTask;
        return string.Empty;
    }

    private async Task<IReadOnlyList<string>> SearchWikipediaAsync(
        string query, int maxResults, CancellationToken ct)
    {
        try
        {
            var encoded = Uri.EscapeDataString(query);
            var url = string.Concat(
                WikipediaApiBase.ToString(),
                "?action=query&list=search",
                $"&srsearch={encoded}&srlimit={maxResults}&format=json&srprop=snippet");
            var json = await _httpClient.GetStringAsync(url, ct);
            return ParseWikipediaSnippets(json);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Wikipedia search failed for: {Query}", query);
            return [];
        }
    }

    private static IReadOnlyList<string> ParseWikipediaSnippets(string json)
    {
        var snippets = new List<string>();
        var pos = 0;
        const string marker = "\"snippet\"";
        while (true)
        {
            var idx = json.IndexOf(marker, pos, StringComparison.Ordinal);
            if (idx < 0) break;
            var colonIdx = json.IndexOf(':', idx + marker.Length);
            var quoteStart = json.IndexOf('"', colonIdx + 1) + 1;
            var quoteEnd = json.IndexOf('"', quoteStart);
            if (quoteStart > 0 && quoteEnd > quoteStart)
                snippets.Add(StripHtmlTags(json[quoteStart..quoteEnd]));
            pos = quoteEnd + 1;
        }
        return snippets;
    }

    private static string StripHtmlTags(string input)
        => Regex.Replace(input, "<[^>]*>", string.Empty);
}

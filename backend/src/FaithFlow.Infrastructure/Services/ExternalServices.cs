using System.Net.Http.Json;
using System.Text.Json;
using FaithFlow.Application.Common.DTOs.SermonNotes;
using FaithFlow.Application.Common.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace FaithFlow.Infrastructure.Services;

public class GeminiService : IGeminiService
{
    private readonly HttpClient _httpClient;
    private readonly string? _apiKey;
    private readonly ILogger<GeminiService> _logger;

    public GeminiService(HttpClient httpClient, IConfiguration config, ILogger<GeminiService> logger)
    {
        _httpClient = httpClient;
        _apiKey = config["GoogleAI:ApiKey"];
        _logger = logger;
    }

    public async Task<string> GeneratePrayerAsync(string title, string description)
    {
        var prompt = $"""
            You are a compassionate prayer companion. Given the following prayer request, 
            generate a warm, compassionate, scripture-based prayer in 2-3 sentences. 
            Include a relevant Bible verse reference.
            
            Prayer Request Title: {title}
            Prayer Request: {description}
            
            Generate only the prayer text, nothing else.
            """;

        return await CallGeminiAsync(prompt);
    }

    public async Task<string> GenerateDiscussionGuideAsync(string title, string scripture, string summary)
    {
        var prompt = $"""
            You are a small group discussion guide creator for a church. Given the following 
            sermon information, generate exactly 5 thoughtful discussion questions for a 
            small group Bible study. Questions should encourage personal reflection, 
            scripture application, and group discussion.
            
            Sermon Title: {title}
            Scripture: {scripture}
            Summary: {summary}
            
            Format as a numbered list (1-5). Generate only the questions, nothing else.
            """;

        return await CallGeminiAsync(prompt);
    }

    private async Task<string> CallGeminiAsync(string prompt)
    {
        if (string.IsNullOrWhiteSpace(_apiKey))
        {
            _logger.LogWarning("Gemini API key not configured. Returning placeholder.");
            return "AI features require a Google API key. Please configure GOOGLE_API_KEY in your environment.";
        }

        try
        {
            var url = $"https://generativelanguage.googleapis.com/v1beta/models/gemini-2.5-flash:generateContent?key={_apiKey}";
            var body = new
            {
                contents = new[] { new { parts = new[] { new { text = prompt } } } },
                generationConfig = new { temperature = 0.7, maxOutputTokens = 1500 }
            };

            var response = await _httpClient.PostAsJsonAsync(url, body);
            
            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                _logger.LogError("Gemini API Error {StatusCode}: {ErrorBody}", response.StatusCode, errorContent);
                return "Unable to generate AI content at this time. Please try again later.";
            }

            var json = await response.Content.ReadFromJsonAsync<JsonElement>();
            var text = json.GetProperty("candidates")[0]
                .GetProperty("content")
                .GetProperty("parts")[0]
                .GetProperty("text")
                .GetString();

            return text ?? "Unable to generate content.";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calling Gemini API");
            return "Unable to generate AI content at this time. Please try again later.";
        }
    }
}

public class BibleService : IBibleService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<BibleService> _logger;

    public BibleService(HttpClient httpClient, ILogger<BibleService> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<ScriptureLookupResponse?> LookupScriptureAsync(string reference)
    {
        try
        {
            var encodedRef = reference.Replace(" ", "+");
            var response = await _httpClient.GetAsync($"https://bible-api.com/{encodedRef}");

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("Bible API returned {StatusCode} for reference: {Reference}", response.StatusCode, reference);
                return null;
            }

            var json = await response.Content.ReadFromJsonAsync<JsonElement>();

            return new ScriptureLookupResponse
            {
                Reference = json.GetProperty("reference").GetString() ?? reference,
                Text = json.GetProperty("text").GetString()?.Trim() ?? "",
                Translation = json.TryGetProperty("translation_name", out var tn) ? tn.GetString() ?? "WEB" : "WEB"
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error looking up scripture: {Reference}", reference);
            return null;
        }
    }
}

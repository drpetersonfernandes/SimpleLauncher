using System.Globalization;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using SimpleLauncher.ResourceTranslator.Models;

namespace SimpleLauncher.ResourceTranslator.Services;

public class GeminiTranslationService
{
    private static readonly HttpClient HttpClient = new()
    {
        Timeout = TimeSpan.FromMinutes(5)
    };

    private readonly string _apiKey;
    private readonly string _modelId;
    private readonly string _apiVersion;

    public GeminiTranslationService(string apiKey, string modelId, string apiVersion)
    {
        _apiKey = apiKey;
        _modelId = modelId;
        _apiVersion = apiVersion;
    }

    public static List<GeminiModelInfo> GetAvailableModels()
    {
        return
        [
            new GeminiModelInfo
            {
                Id = "gemini-3.1-pro-preview",
                Name = "gemini-3.1-pro-preview",
                Description = "$2-4.0 Input. $12-18.0 Output. 1M Context",
                ContextLength = 1000000,
                ApiVersion = "v1beta"
            },
            new GeminiModelInfo
            {
                Id = "gemini-3-pro-preview",
                Name = "gemini-3-pro-preview",
                Description = "$2-4.0 Input. $12-18.0 Output. 1M Context",
                ContextLength = 1000000,
                ApiVersion = "v1beta"
            },
            new GeminiModelInfo
            {
                Id = "gemini-3-flash-preview",
                Name = "gemini-3-flash-preview",
                Description = "$0.5 Input. $3.0 Output. 1M Context",
                ContextLength = 1000000,
                ApiVersion = "v1beta"
            },
            new GeminiModelInfo
            {
                Id = "gemini-2.5-pro",
                Name = "gemini-2.5-pro",
                Description = "$1.25-2.5 Input. $10-15.0 Output. 1M Context",
                ContextLength = 1000000,
                ApiVersion = "v1beta"
            },
            new GeminiModelInfo
            {
                Id = "gemini-2.5-flash",
                Name = "gemini-2.5-flash",
                Description = "$0.3 Input. $2.5 Output. 1M Context",
                ContextLength = 1000000,
                ApiVersion = "v1beta"
            },
            new GeminiModelInfo
            {
                Id = "gemini-2.5-flash-preview-09-2025",
                Name = "gemini-2.5-flash-preview-09-2025",
                Description = "$0.3 Input. $2.5 Output. 1M Context",
                ContextLength = 1000000,
                ApiVersion = "v1beta"
            },
            new GeminiModelInfo
            {
                Id = "gemini-2.5-flash-lite",
                Name = "gemini-2.5-flash-lite",
                Description = "$0.1 Input. $0.4 Output. 1M Context",
                ContextLength = 1000000,
                ApiVersion = "v1beta"
            },
            new GeminiModelInfo
            {
                Id = "gemini-2.5-flash-lite-preview-09-2025",
                Name = "gemini-2.5-flash-lite-preview-09-2025",
                Description = "$0.1 Input. $0.4 Output. 1M Context",
                ContextLength = 1000000,
                ApiVersion = "v1beta"
            }
        ];
    }

    public async Task<Dictionary<string, string>> TranslateBatchAsync(
        string targetLanguageName,
        List<KeyValuePair<string, string>> entries,
        CancellationToken cancellationToken = default)
    {
        var apiUrl = $"https://generativelanguage.googleapis.com/{_apiVersion}/models/{_modelId}:generateContent?key={_apiKey}";

        var sb = new StringBuilder();
        sb.AppendLine(CultureInfo.InvariantCulture, $"You are a professional UI translator. Translate each English string into {targetLanguageName}.");
        sb.AppendLine("Preserve UI context, keep placeholders like {0}, {1}, etc. intact.");
        sb.AppendLine("Do NOT add explanations, markdown, or any extra text.");
        sb.AppendLine("Return EXACTLY one line per item in this strict format:");
        sb.AppendLine("Key|TranslatedValue");
        sb.AppendLine();
        sb.AppendLine("English strings:");
        foreach (var entry in entries)
        {
            var escapedValue = entry.Value.Replace("|", "\\|");
            sb.AppendLine(CultureInfo.InvariantCulture, $"{entry.Key}|{escapedValue}");
        }

        var requestData = new
        {
            contents = new[]
            {
                new
                {
                    role = "user",
                    parts = new[] { new { text = sb.ToString() } }
                }
            },
            generationConfig = new
            {
                temperature = 0.2,
                topP = 0.95,
                topK = 40
            }
        };

        using var response = await HttpClient.PostAsJsonAsync(apiUrl, requestData, cancellationToken);
        var responseJson = await response.Content.ReadAsStringAsync(cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            throw new HttpRequestException($"Gemini API error ({response.StatusCode}): {responseJson}");
        }

        var text = ExtractTextFromResponse(responseJson);
        return ParseTranslations(text, entries.Select(static e => e.Key).ToList());
    }

    private static string ExtractTextFromResponse(string responseJson)
    {
        using var doc = JsonDocument.Parse(responseJson);

        if (doc.RootElement.TryGetProperty("error", out var errorElement))
        {
            var message = errorElement.TryGetProperty("message", out var msgProp)
                ? msgProp.GetString()
                : "Unknown error";
            throw new InvalidOperationException($"Gemini API error: {message}");
        }

        if (doc.RootElement.TryGetProperty("promptFeedback", out var feedbackElement))
        {
            var blockReason = feedbackElement.TryGetProperty("blockReason", out var reasonProp)
                ? reasonProp.GetString()
                : "Unknown";
            throw new InvalidOperationException($"Request blocked by Gemini. Reason: {blockReason}");
        }

        if (!doc.RootElement.TryGetProperty("candidates", out var candidates) ||
            candidates.ValueKind != JsonValueKind.Array ||
            candidates.GetArrayLength() == 0)
        {
            throw new InvalidOperationException("No candidates in Gemini response.");
        }

        var first = candidates[0];
        if (first.TryGetProperty("finishReason", out var finishReason))
        {
            var reason = finishReason.GetString();
            if (reason != "STOP")
            {
                throw new InvalidOperationException($"Gemini generation stopped. Reason: {reason}");
            }
        }

        if (!first.TryGetProperty("content", out var content) ||
            !content.TryGetProperty("parts", out var parts) ||
            parts.ValueKind != JsonValueKind.Array ||
            parts.GetArrayLength() == 0 ||
            !parts[0].TryGetProperty("text", out var textElement))
        {
            throw new InvalidOperationException("Unable to extract text from Gemini response.");
        }

        return textElement.GetString() ?? string.Empty;
    }

    private static Dictionary<string, string> ParseTranslations(string text, List<string> expectedKeys)
    {
        var result = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        var remainingKeys = new HashSet<string>(expectedKeys, StringComparer.OrdinalIgnoreCase);

        foreach (var rawLine in text.Split(['\r', '\n'], StringSplitOptions.RemoveEmptyEntries))
        {
            var line = rawLine.Trim();
            if (string.IsNullOrEmpty(line)) continue;

            var pipeIndex = line.IndexOf('|');
            if (pipeIndex <= 0) continue;

            var key = line[..pipeIndex].Trim();
            var value = line[(pipeIndex + 1)..].Trim();

            // Unescape pipes
            value = value.Replace("\\|", "|");

            if (remainingKeys.Contains(key))
            {
                result[key] = value;
                remainingKeys.Remove(key);
            }
        }

        // Fill any missing translations with empty string
        foreach (var key in remainingKeys)
        {
            result[key] = string.Empty;
        }

        return result;
    }
}

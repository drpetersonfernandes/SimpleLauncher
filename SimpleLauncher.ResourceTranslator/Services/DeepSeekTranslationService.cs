using System.Globalization;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using SimpleLauncher.ResourceTranslator.Models;

namespace SimpleLauncher.ResourceTranslator.Services;

/// <summary>
///     Provides translation services using the DeepSeek API (OpenAI-compatible).
/// </summary>
public class DeepSeekTranslationService
{
    private static readonly HttpClient HttpClient = new()
    {
        Timeout = TimeSpan.FromMinutes(5)
    };

    private readonly string _apiKey;
    private readonly string _modelId;

    /// <summary>
    ///     Initializes a new instance of the <see cref="DeepSeekTranslationService" /> class.
    /// </summary>
    /// <param name="apiKey">The DeepSeek API key.</param>
    /// <param name="modelId">The model identifier to use for translations.</param>
    public DeepSeekTranslationService(string apiKey, string modelId)
    {
        _apiKey = apiKey;
        _modelId = modelId;
    }

    /// <summary>
    ///     Returns the list of available DeepSeek models for translation.
    /// </summary>
    /// <returns>A list of available model information.</returns>
    public static IList<DeepSeekModelInfo> GetAvailableModels()
    {
        return
        [
            new DeepSeekModelInfo
            {
                Id = "deepseek-chat",
                Name = "deepseek-chat",
                Description = "DeepSeek-V3. $0.27 Input. $1.10 Output. 128K Context",
                ContextLength = 131072
            },
            new DeepSeekModelInfo
            {
                Id = "deepseek-reasoner",
                Name = "deepseek-reasoner",
                Description = "DeepSeek-R1. $0.55 Input. $2.19 Output. 128K Context",
                ContextLength = 131072
            }
        ];
    }

    /// <summary>
    ///     Translates a batch of key-value pairs to the target language using DeepSeek API.
    /// </summary>
    /// <param name="targetLanguageName">The name of the target language.</param>
    /// <param name="entries">The list of key-value pairs to translate.</param>
    /// <param name="cancellationToken">Optional cancellation token.</param>
    /// <returns>A dictionary of translated key-value pairs.</returns>
    public async Task<IDictionary<string, string>> TranslateBatchAsync(
        string targetLanguageName,
        IList<KeyValuePair<string, string>> entries,
        CancellationToken cancellationToken = default)
    {
        try
        {
            const string apiUrl = "https://api.deepseek.com/chat/completions";

            var sb = new StringBuilder();
            sb.AppendLine(CultureInfo.InvariantCulture,
                $"You are a professional UI translator. Translate each English string into {targetLanguageName}.");
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
                model = _modelId,
                messages = new[]
                {
                    new
                    {
                        role = "user",
                        content = sb.ToString()
                    }
                },
                temperature = 0.2,
                top_p = 0.95
            };

            using var request = new HttpRequestMessage(HttpMethod.Post, apiUrl);
            request.Headers.Add("Authorization", $"Bearer {_apiKey}");
            request.Content = JsonContent.Create(requestData);

            using var response = await HttpClient.SendAsync(request, cancellationToken);
            var responseJson = await response.Content.ReadAsStringAsync(cancellationToken);

            if (!response.IsSuccessStatusCode)
                throw new HttpRequestException($"DeepSeek API error ({response.StatusCode}): {responseJson}");

            var text = ExtractTextFromResponse(responseJson);
            return ParseTranslations(text, entries.Select(static e => e.Key).ToList());
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            Log.Error(ex, "Translation batch failed for {TargetLanguage}", targetLanguageName);
            throw;
        }
    }

    private static string ExtractTextFromResponse(string responseJson)
    {
        using var doc = JsonDocument.Parse(responseJson);

        if (doc.RootElement.TryGetProperty("error", out var errorElement))
        {
            var errorMessage = errorElement.TryGetProperty("message", out var msgProp)
                ? msgProp.GetString()
                : "Unknown error";
            throw new InvalidOperationException($"DeepSeek API error: {errorMessage}");
        }

        if (!doc.RootElement.TryGetProperty("choices", out var choices) ||
            choices.ValueKind != JsonValueKind.Array ||
            choices.GetArrayLength() == 0)
            throw new InvalidOperationException("No choices in DeepSeek response.");

        var first = choices[0];
        if (first.TryGetProperty("finish_reason", out var finishReason))
        {
            var reason = finishReason.GetString();
            if (!string.Equals(reason, "stop", StringComparison.Ordinal))
                throw new InvalidOperationException($"DeepSeek generation stopped. Reason: {reason}");
        }

        if (!first.TryGetProperty("message", out var message) ||
            !message.TryGetProperty("content", out var contentElement))
            throw new InvalidOperationException("Unable to extract content from DeepSeek response.");

        return contentElement.GetString() ?? "";
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
        foreach (var key in remainingKeys) result[key] = "";

        return result;
    }
}

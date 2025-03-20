using System.IO;
using System.Text.Json;

namespace MAMEUtility;

public class AppConfig
{
    public string BugReportApiUrl { get; set; } = "https://www.purelogiccode.com/bugreport/api/send-bug-report";
    public string BugReportApiKey { get; set; } = "hjh7yu6t56tyr540o9u8767676r5674534453235264c75b6t7ggghgg76trf564e";

    private static readonly Lazy<AppConfig> Instance2 = new(LoadConfig);

    public static AppConfig Instance => Instance2.Value;

    private static AppConfig LoadConfig()
    {
        try
        {
            var configPath = Path.Combine(
                AppDomain.CurrentDomain.BaseDirectory,
                "appsettings.json");

            if (File.Exists(configPath))
            {
                var json = File.ReadAllText(configPath);
                return JsonSerializer.Deserialize<AppConfig>(json) ?? new AppConfig();
            }
        }
        catch
        {
            // If we can't load the config, use defaults
        }

        return new AppConfig();
    }
}
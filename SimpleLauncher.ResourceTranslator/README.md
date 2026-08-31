# SimpleLauncher.ResourceTranslator

Console application that uses the **OpenRouter API** to automatically translate missing English resource keys into all other language files for [SimpleLauncher](../SimpleLauncher).

## What It Does

1. Scans `SimpleLauncher/resources/strings.en.xaml` as the master key list.
2. Compares every other `strings.*.xaml` file against English.
3. **Auto-removes duplicate keys** found in target language files.
4. **Translates missing keys** in batches via LLM.
5. **Preserves empty values** from English as empty tags in target languages.
6. **Re-sorts** each resource file alphabetically by key after writing.

## Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- An [OpenRouter API key](https://openrouter.ai/keys) (the app **prompts every run** and does **not** store it)

## Running

```bash
dotnet run --project SimpleLauncher.ResourceTranslator
```

### Workflow

1. The app locates `SimpleLauncher/resources` automatically from the solution structure.
2. It prints an analysis summary: how many languages need updates, how many keys are missing, and how many duplicates will be removed.
3. Press any key to proceed.
4. Enter your OpenRouter API key when prompted.
5. Select a model (or press Enter for the default `z-ai/glm-5.3-flash`).
6. The app translates keys in batches of 40 with a small delay between requests to avoid rate limits.
7. Each language file is updated and saved automatically.

## Project Structure

```
SimpleLauncher.ResourceTranslator/
├── SimpleLauncher.ResourceTranslator.csproj
├── Program.cs                          # Entry point, user prompts, orchestration
├── Models/
│   ├── OpenRouterModelInfo.cs            # Model metadata (id, name, description)
│   └── MissingKeyBatch.cs              # Holds missing keys & duplicates per language
└── Services/
    ├── ResourceAnalyzer.cs             # Reads English keys and diffs other languages
    ├── OpenRouterTranslationService.cs # HTTP client for OpenRouter API batch translation
    └── XamlResourceWriter.cs           # Writes updated XAML, removes duplicates, sorts keys
```

## Configuration

No configuration files are used. The only runtime inputs are:

- **API key** (typed interactively, never persisted)
- **Model selection** (default: `z-ai/glm-5.3-flash`)

## Notes

- If a translation batch fails (network error, rate limit, etc.), the app **skips that batch** and does not add empty strings to the resource file. Run the app again later to retry.
- The app is safe to run multiple times; it only processes keys that are actually missing.
- Empty English values are intentionally preserved as empty `<system:String>` entries so translators can fill them in later.

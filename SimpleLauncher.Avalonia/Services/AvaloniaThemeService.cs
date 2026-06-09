using Avalonia;
using Avalonia.Media;
using Avalonia.Styling;
using SimpleLauncher.Core.Interfaces;

namespace SimpleLauncher.Avalonia.Services;

public class AvaloniaThemeService : IThemeService
{
    private static readonly Dictionary<string, Color> AccentColorMap = new()
    {
        ["Amber"] = Color.Parse("#F5A623"),
        ["Blue"] = Color.Parse("#5B9BD5"),
        ["Brown"] = Color.Parse("#8B6914"),
        ["Cobalt"] = Color.Parse("#0047AB"),
        ["Crimson"] = Color.Parse("#DC143C"),
        ["Cyan"] = Color.Parse("#00BCD4"),
        ["Emerald"] = Color.Parse("#50C878"),
        ["Green"] = Color.Parse("#4CAF50"),
        ["Indigo"] = Color.Parse("#3F51B5"),
        ["Lime"] = Color.Parse("#CDDC39"),
        ["Magenta"] = Color.Parse("#E91E63"),
        ["Maroon"] = Color.Parse("#800000"),
        ["Mauve"] = Color.Parse("#E0B0FF"),
        ["Olive"] = Color.Parse("#808000"),
        ["OliveDrab"] = Color.Parse("#6B8E23"),
        ["Orange"] = Color.Parse("#FF9800"),
        ["Pink"] = Color.Parse("#FF69B4"),
        ["Plum"] = Color.Parse("#DDA0DD"),
        ["Purple"] = Color.Parse("#9C27B0"),
        ["Red"] = Color.Parse("#F44336"),
        ["Sienna"] = Color.Parse("#A0522D"),
        ["SkyBlue"] = Color.Parse("#87CEEB"),
        ["Steel"] = Color.Parse("#4682B4"),
        ["Taupe"] = Color.Parse("#483C32"),
        ["Teal"] = Color.Parse("#009688"),
        ["Violet"] = Color.Parse("#7C4DFF"),
        ["Yellow"] = Color.Parse("#FFEB3B")
    };

    public string CurrentBaseTheme { get; private set; } = "Dark";
    public string CurrentAccentColor { get; private set; } = "Blue";

    public void ApplyTheme(string baseTheme, string accentColor)
    {
        CurrentBaseTheme = baseTheme;
        CurrentAccentColor = accentColor;

        var app = Application.Current;
        if (app == null) return;

        var accent = AccentColorMap.GetValueOrDefault(accentColor, AccentColorMap["Blue"]);

        switch (baseTheme)
        {
            case "Light":
                ApplyLightTheme(app, accent);
                break;
            case "Midnight":
                ApplyMidnightTheme(app, accent);
                break;
            case "HighContrast":
                ApplyHighContrastTheme(app, accent);
                break;
            default:
                ApplyDarkTheme(app, accent);
                break;
        }
    }

    private static void ApplyDarkTheme(Application app, Color accent)
    {
        app.RequestedThemeVariant = ThemeVariant.Dark;
        SetThemeResources(app, new ThemeColors
        {
            Background = Color.Parse("#1e1e2e"),
            Surface = Color.Parse("#313244"),
            SurfaceAlt = Color.Parse("#45475a"),
            SurfaceHover = Color.Parse("#585b70"),
            Foreground = Color.Parse("#cdd6f4"),
            ForegroundSecondary = Color.Parse("#a6adc8"),
            ForegroundMuted = Color.Parse("#6c7086"),
            Accent = accent,
            AccentLight = Lighten(accent, 0.3f),
            AccentDark = Darken(accent, 0.2f),
            IdealForeground = Colors.White,
            Border = Color.Parse("#585b70"),
            Divider = Color.Parse("#313244"),
            Success = Color.Parse("#a6e3a1"),
            Warning = Color.Parse("#f9e2af"),
            Error = Color.Parse("#f38ba8"),
            NavBackground = Color.Parse("#181825"),
            StatusBarBackground = Color.Parse("#1e1e2e"),
            CardBackground = Color.Parse("#45475a"),
            CardHover = Color.Parse("#585b70"),
            Overlay = Color.Parse("#80000000")
        });
    }

    private static void ApplyLightTheme(Application app, Color accent)
    {
        app.RequestedThemeVariant = ThemeVariant.Light;
        SetThemeResources(app, new ThemeColors
        {
            Background = Color.Parse("#eff1f5"),
            Surface = Color.Parse("#e6e9ef"),
            SurfaceAlt = Color.Parse("#dce0e8"),
            SurfaceHover = Color.Parse("#ccd0da"),
            Foreground = Color.Parse("#4c4f69"),
            ForegroundSecondary = Color.Parse("#6c6f85"),
            ForegroundMuted = Color.Parse("#9ca0b0"),
            Accent = accent,
            AccentLight = Lighten(accent, 0.3f),
            AccentDark = Darken(accent, 0.2f),
            IdealForeground = Colors.White,
            Border = Color.Parse("#ccd0da"),
            Divider = Color.Parse("#e6e9ef"),
            Success = Color.Parse("#40a02b"),
            Warning = Color.Parse("#df8e1d"),
            Error = Color.Parse("#d20f39"),
            NavBackground = Color.Parse("#dce0e8"),
            StatusBarBackground = Color.Parse("#e6e9ef"),
            CardBackground = Color.Parse("#dce0e8"),
            CardHover = Color.Parse("#ccd0da"),
            Overlay = Color.Parse("#40000000")
        });
    }

    private static void ApplyMidnightTheme(Application app, Color accent)
    {
        app.RequestedThemeVariant = ThemeVariant.Dark;
        SetThemeResources(app, new ThemeColors
        {
            Background = Color.Parse("#0a0a14"),
            Surface = Color.Parse("#12121e"),
            SurfaceAlt = Color.Parse("#1a1a2e"),
            SurfaceHover = Color.Parse("#252540"),
            Foreground = Color.Parse("#c0c0d0"),
            ForegroundSecondary = Color.Parse("#8888a0"),
            ForegroundMuted = Color.Parse("#555570"),
            Accent = accent,
            AccentLight = Lighten(accent, 0.3f),
            AccentDark = Darken(accent, 0.2f),
            IdealForeground = Colors.White,
            Border = Color.Parse("#252540"),
            Divider = Color.Parse("#1a1a2e"),
            Success = Color.Parse("#50c878"),
            Warning = Color.Parse("#f0c040"),
            Error = Color.Parse("#e05050"),
            NavBackground = Color.Parse("#08080f"),
            StatusBarBackground = Color.Parse("#0a0a14"),
            CardBackground = Color.Parse("#1a1a2e"),
            CardHover = Color.Parse("#252540"),
            Overlay = Color.Parse("#a0000000")
        });
    }

    private static void ApplyHighContrastTheme(Application app, Color accent)
    {
        app.RequestedThemeVariant = ThemeVariant.Dark;
        SetThemeResources(app, new ThemeColors
        {
            Background = Colors.Black,
            Surface = Color.Parse("#1a1a1a"),
            SurfaceAlt = Color.Parse("#333333"),
            SurfaceHover = Color.Parse("#4d4d4d"),
            Foreground = Colors.White,
            ForegroundSecondary = Color.Parse("#cccccc"),
            ForegroundMuted = Color.Parse("#999999"),
            Accent = Colors.Yellow,
            AccentLight = Colors.LightYellow,
            AccentDark = Colors.Gold,
            IdealForeground = Colors.Black,
            Border = Colors.White,
            Divider = Colors.White,
            Success = Colors.Lime,
            Warning = Colors.Yellow,
            Error = Colors.Red,
            NavBackground = Colors.Black,
            StatusBarBackground = Colors.Black,
            CardBackground = Color.Parse("#1a1a1a"),
            CardHover = Color.Parse("#333333"),
            Overlay = Color.Parse("#cc000000")
        });
    }

    private static void SetThemeResources(Application app, ThemeColors colors)
    {
        var res = app.Resources;

        // Core brushes
        res["ThemeBackgroundBrush"] = new SolidColorBrush(colors.Background);
        res["ThemeSurfaceBrush"] = new SolidColorBrush(colors.Surface);
        res["ThemeSurfaceAltBrush"] = new SolidColorBrush(colors.SurfaceAlt);
        res["ThemeSurfaceHoverBrush"] = new SolidColorBrush(colors.SurfaceHover);
        res["ThemeForegroundBrush"] = new SolidColorBrush(colors.Foreground);
        res["ThemeForegroundSecondaryBrush"] = new SolidColorBrush(colors.ForegroundSecondary);
        res["ThemeForegroundMutedBrush"] = new SolidColorBrush(colors.ForegroundMuted);
        res["ThemeAccentBrush"] = new SolidColorBrush(colors.Accent);
        res["ThemeAccentLightBrush"] = new SolidColorBrush(colors.AccentLight);
        res["ThemeAccentDarkBrush"] = new SolidColorBrush(colors.AccentDark);
        res["ThemeIdealForegroundBrush"] = new SolidColorBrush(colors.IdealForeground);
        res["ThemeBorderBrush"] = new SolidColorBrush(colors.Border);
        res["ThemeDividerBrush"] = new SolidColorBrush(colors.Divider);
        res["ThemeSuccessBrush"] = new SolidColorBrush(colors.Success);
        res["ThemeWarningBrush"] = new SolidColorBrush(colors.Warning);
        res["ThemeErrorBrush"] = new SolidColorBrush(colors.Error);
        res["ThemeNavBackgroundBrush"] = new SolidColorBrush(colors.NavBackground);
        res["ThemeStatusBarBackgroundBrush"] = new SolidColorBrush(colors.StatusBarBackground);
        res["ThemeCardBackgroundBrush"] = new SolidColorBrush(colors.CardBackground);
        res["ThemeCardHoverBrush"] = new SolidColorBrush(colors.CardHover);
        res["ThemeOverlayBrush"] = new SolidColorBrush(colors.Overlay);

        // Aliases for MahApps compatibility
        res["MahApps.Brushes.ThemeBackground"] = res["ThemeBackgroundBrush"];
        res["MahApps.Brushes.ThemeForeground"] = res["ThemeForegroundBrush"];
        res["MahApps.Brushes.Accent"] = res["ThemeAccentBrush"];
        res["MahApps.Brushes.Accent2"] = res["ThemeAccentLightBrush"];
        res["MahApps.Brushes.Accent3"] = res["ThemeAccentLightBrush"];
        res["MahApps.Brushes.Accent4"] = res["ThemeAccentDarkBrush"];
        res["MahApps.Brushes.AccentBase"] = res["ThemeAccentBrush"];
        res["MahApps.Brushes.IdealForeground"] = res["ThemeIdealForegroundBrush"];
        res["MahApps.Brushes.Gray5"] = res["ThemeBorderBrush"];
        res["MahApps.Brushes.Gray7"] = res["ThemeBorderBrush"];
        res["MahApps.Brushes.Gray8"] = res["ThemeSurfaceAltBrush"];
        res["MahApps.Brushes.Gray9"] = res["ThemeSurfaceBrush"];
        res["MahApps.Brushes.Gray10"] = res["ThemeSurfaceAltBrush"];
        res["MahApps.Colors.Accent"] = colors.Accent;
        res["MahApps.Colors.Gray3"] = colors.Surface;
        res["MahApps.Colors.Gray5"] = colors.Border;
    }

    private static Color Lighten(Color color, float amount)
    {
        var r = (byte)Math.Min(255, color.R + (255 - color.R) * amount);
        var g = (byte)Math.Min(255, color.G + (255 - color.G) * amount);
        var b = (byte)Math.Min(255, color.B + (255 - color.B) * amount);
        return Color.FromRgb(r, g, b);
    }

    private static Color Darken(Color color, float amount)
    {
        var r = (byte)(color.R * (1 - amount));
        var g = (byte)(color.G * (1 - amount));
        var b = (byte)(color.B * (1 - amount));
        return Color.FromRgb(r, g, b);
    }

    private struct ThemeColors
    {
        public Color Background;
        public Color Surface;
        public Color SurfaceAlt;
        public Color SurfaceHover;
        public Color Foreground;
        public Color ForegroundSecondary;
        public Color ForegroundMuted;
        public Color Accent;
        public Color AccentLight;
        public Color AccentDark;
        public Color IdealForeground;
        public Color Border;
        public Color Divider;
        public Color Success;
        public Color Warning;
        public Color Error;
        public Color NavBackground;
        public Color StatusBarBackground;
        public Color CardBackground;
        public Color CardHover;
        public Color Overlay;
    }
}

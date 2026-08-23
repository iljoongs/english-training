using System.IO;
using System.Text.Json;
using EnglishTraining.Models;

namespace EnglishTraining.Services;

/// <summary>
/// Persists lightweight app state (last-selected topic, reading window
/// position/size) across runs. Unlike Json*Repository (which manage a
/// list of entries), this stores a single settings object.
/// </summary>
public sealed class AppSettingsStore
{
    private static readonly JsonSerializerOptions SerializerOptions = new() { WriteIndented = true };

    private readonly string _filePath;
    private readonly AppSettings _settings;

    public AppSettingsStore(string filePath)
    {
        _filePath = filePath;
        _settings = File.Exists(filePath)
            ? JsonSerializer.Deserialize<AppSettings>(File.ReadAllText(filePath), SerializerOptions) ?? new AppSettings()
            : new AppSettings();
    }

    public static AppSettingsStore CreateDefault()
    {
        return new AppSettingsStore(AppDataPaths.Resolve("settings.json"));
    }

    public Guid? LastSelectedTopicId => _settings.LastSelectedTopicId;

    public double? WindowWidth => _settings.WindowWidth;

    public double? WindowHeight => _settings.WindowHeight;

    public double? WindowLeft => _settings.WindowLeft;

    public double? WindowTop => _settings.WindowTop;

    public string? FontFamilyName => _settings.FontFamilyName;

    public double? FontSize => _settings.FontSize;

    public double? LineSpacingMultiplier => _settings.LineSpacingMultiplier;

    public MarginPreset? MarginPreset => _settings.MarginPreset;

    public ReadingTheme? Theme => _settings.Theme;

    public double? DimmingOpacity => _settings.DimmingOpacity;

    public void SetLastSelectedTopic(Guid? topicId)
    {
        _settings.LastSelectedTopicId = topicId;
        Save();
    }

    public void SetWindowBounds(double left, double top, double width, double height)
    {
        _settings.WindowLeft = left;
        _settings.WindowTop = top;
        _settings.WindowWidth = width;
        _settings.WindowHeight = height;
        Save();
    }

    public void SetDisplaySettings(
        string fontFamilyName,
        double fontSize,
        double lineSpacingMultiplier,
        MarginPreset marginPreset,
        ReadingTheme theme,
        double dimmingOpacity)
    {
        _settings.FontFamilyName = fontFamilyName;
        _settings.FontSize = fontSize;
        _settings.LineSpacingMultiplier = lineSpacingMultiplier;
        _settings.MarginPreset = marginPreset;
        _settings.Theme = theme;
        _settings.DimmingOpacity = dimmingOpacity;
        Save();
    }

    private void Save()
    {
        var directory = Path.GetDirectoryName(_filePath);
        if (!string.IsNullOrEmpty(directory))
        {
            Directory.CreateDirectory(directory);
        }

        File.WriteAllText(_filePath, JsonSerializer.Serialize(_settings, SerializerOptions));
    }
}

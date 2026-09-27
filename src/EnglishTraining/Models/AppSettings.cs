namespace EnglishTraining.Models;

public sealed class AppSettings
{
    public string? LastSelectedTopicFile { get; set; }
    public string? LastSelectedTopicTitle { get; set; }
    public string? DataFolder { get; set; }
    public double? WindowWidth { get; set; }
    public double? WindowHeight { get; set; }
    public double? WindowLeft { get; set; }
    public double? WindowTop { get; set; }
    public string? FontFamilyName { get; set; }
    public double? FontSize { get; set; }
    public double? LineSpacingMultiplier { get; set; }
    public MarginPreset? MarginPreset { get; set; }
    public ReadingTheme? Theme { get; set; }
    public double? DimmingOpacity { get; set; }
}

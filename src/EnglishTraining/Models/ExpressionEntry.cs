namespace EnglishTraining.Models;

public sealed class ExpressionEntry : IEntry
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public string Text { get; set; } = string.Empty;
    public string Meaning { get; set; } = string.Empty;
    public string Usage { get; set; } = string.Empty;
    public string Example { get; set; } = string.Empty;
}

namespace EnglishTraining.Models;

public sealed class InterpretationEntry : IEntry
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public string Text { get; set; } = string.Empty;
    public string PartOfSpeech { get; set; } = string.Empty;
    public string Ko { get; set; } = string.Empty;
    public string Expression { get; set; } = string.Empty;
}

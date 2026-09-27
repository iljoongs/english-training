namespace EnglishTraining.Models;

public sealed class Topic
{
    public required Guid Id { get; init; }
    public required string Title { get; set; }
    public required string Text { get; set; }

    /// <summary>
    /// Name of the lesson file (§30/§31) this topic was read from, used to
    /// restore the last-selected topic across folder reloads (Ids are
    /// regenerated every read).
    /// </summary>
    public required string SourceFileName { get; init; }
}

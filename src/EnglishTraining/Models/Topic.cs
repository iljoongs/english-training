namespace EnglishTraining.Models;

public sealed class Topic
{
    public required Guid Id { get; init; }
    public required string Title { get; set; }
    public required string Text { get; set; }
}

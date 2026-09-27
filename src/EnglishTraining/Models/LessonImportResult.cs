namespace EnglishTraining.Models;

public sealed class LessonImportResult
{
    public int TopicsAdded { get; init; }
    public int WordsAdded { get; init; }
    public int WritingsAdded { get; init; }
    public int DuplicatesSkipped { get; init; }
}

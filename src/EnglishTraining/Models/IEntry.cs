namespace EnglishTraining.Models;

public interface IEntry
{
    Guid Id { get; }
    string Text { get; set; }
}

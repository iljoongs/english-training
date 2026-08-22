using EnglishTraining.Models;

namespace EnglishTraining.Services;

public interface ITopicRepository
{
    IReadOnlyList<Topic> Topics { get; }

    string FilePath { get; }

    void Add(Topic topic);

    void Remove(IEnumerable<Guid> ids);

    void Save();

    void SaveAs(string filePath);

    void Open(string filePath);
}

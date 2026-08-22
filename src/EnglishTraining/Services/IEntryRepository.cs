using EnglishTraining.Models;

namespace EnglishTraining.Services;

public interface IEntryRepository<T> where T : IEntry
{
    IReadOnlyList<T> Entries { get; }

    string FilePath { get; }

    void Add(T entry);

    void Remove(IEnumerable<Guid> ids);

    void Save();

    void SaveAs(string filePath);

    void Open(string filePath);
}

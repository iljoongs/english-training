using System.IO;
using System.Text.Json;
using EnglishTraining.Models;

namespace EnglishTraining.Services;

public sealed class JsonEntryRepository<T> : IEntryRepository<T> where T : IEntry
{
    private static readonly JsonSerializerOptions SerializerOptions = new() { WriteIndented = true };

    private readonly List<T> _entries;
    private string _filePath;

    public JsonEntryRepository(string filePath, IReadOnlyList<T> seedIfMissing)
    {
        _filePath = filePath;

        if (File.Exists(_filePath))
        {
            var json = File.ReadAllText(_filePath);
            _entries = JsonSerializer.Deserialize<List<T>>(json, SerializerOptions) ?? [];
        }
        else
        {
            _entries = [.. seedIfMissing];
            Save();
        }
    }

    public IReadOnlyList<T> Entries => _entries;

    public string FilePath => _filePath;

    public void Add(T entry)
    {
        _entries.Add(entry);
    }

    public void Remove(IEnumerable<Guid> ids)
    {
        var idSet = ids.ToHashSet();
        _entries.RemoveAll(e => idSet.Contains(e.Id));
    }

    public void Save()
    {
        var directory = Path.GetDirectoryName(_filePath);
        if (!string.IsNullOrEmpty(directory))
        {
            Directory.CreateDirectory(directory);
        }

        var json = JsonSerializer.Serialize(_entries, SerializerOptions);
        File.WriteAllText(_filePath, json);
    }

    public void SaveAs(string filePath)
    {
        _filePath = filePath;
        Save();
    }

    public void Open(string filePath)
    {
        _filePath = filePath;

        var json = File.Exists(filePath) ? File.ReadAllText(filePath) : "[]";
        var loaded = JsonSerializer.Deserialize<List<T>>(json, SerializerOptions) ?? [];

        _entries.Clear();
        _entries.AddRange(loaded);
    }
}

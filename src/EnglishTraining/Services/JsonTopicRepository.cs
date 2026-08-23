using System.IO;
using System.Text.Json;
using EnglishTraining.Models;

namespace EnglishTraining.Services;

public sealed class JsonTopicRepository : ITopicRepository
{
    private const string DefaultSampleText =
        "I was wondering if you could help me with this problem. I look forward to seeing you soon.\n" +
        "You are supposed to be here by nine, but as far as I know the schedule has changed.";

    private static readonly JsonSerializerOptions SerializerOptions = new() { WriteIndented = true };

    private readonly List<Topic> _topics;
    private string _filePath;

    public JsonTopicRepository(string filePath)
    {
        _filePath = filePath;

        if (File.Exists(_filePath))
        {
            var json = File.ReadAllText(_filePath);
            _topics = JsonSerializer.Deserialize<List<Topic>>(json, SerializerOptions) ?? [];
        }
        else
        {
            _topics = [new Topic { Id = Guid.NewGuid(), Title = "Sample", Text = DefaultSampleText }];
            Save();
        }
    }

    public static JsonTopicRepository CreateDefault()
    {
        return new JsonTopicRepository(AppDataPaths.Resolve("topics.json"));
    }

    public IReadOnlyList<Topic> Topics => _topics;

    public string FilePath => _filePath;

    public void Add(Topic topic)
    {
        _topics.Add(topic);
    }

    public void Remove(IEnumerable<Guid> ids)
    {
        var idSet = ids.ToHashSet();
        _topics.RemoveAll(t => idSet.Contains(t.Id));
    }

    public void Save()
    {
        var directory = Path.GetDirectoryName(_filePath);
        if (!string.IsNullOrEmpty(directory))
        {
            Directory.CreateDirectory(directory);
        }

        var json = JsonSerializer.Serialize(_topics, SerializerOptions);
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
        var loaded = JsonSerializer.Deserialize<List<Topic>>(json, SerializerOptions) ?? [];

        _topics.Clear();
        _topics.AddRange(loaded);
    }
}

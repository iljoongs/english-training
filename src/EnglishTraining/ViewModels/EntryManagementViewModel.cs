using System.Collections.ObjectModel;
using System.IO;
using EnglishTraining.Models;
using EnglishTraining.Services;

namespace EnglishTraining.ViewModels;

public sealed class EntryManagementViewModel<T> : ViewModelBase where T : class, IEntry, new()
{
    private readonly IEntryRepository<T> _repository;
    private readonly Func<string, T> _importParser;
    private readonly Action<T, string> _exporter;
    private T? _selectedEntry;

    public EntryManagementViewModel(IEntryRepository<T> repository, Func<string, T> importParser, Action<T, string> exporter)
    {
        _repository = repository;
        _importParser = importParser;
        _exporter = exporter;
        Entries = new ObservableCollection<T>(repository.Entries);
        _selectedEntry = Entries.FirstOrDefault();
    }

    public ObservableCollection<T> Entries { get; }

    public T? SelectedEntry
    {
        get => _selectedEntry;
        set
        {
            if (SetField(ref _selectedEntry, value))
            {
                _repository.Save();
                NotifyFileStatusChanged();
            }
        }
    }

    public string FileStatusText
    {
        get
        {
            var path = _repository.FilePath;
            var size = File.Exists(path) ? new FileInfo(path).Length : 0;
            return $"{path} ({FormatSize(size)})";
        }
    }

    public T AddEntry(string text)
    {
        var entry = new T { Text = text };
        _repository.Add(entry);
        _repository.Save();
        NotifyFileStatusChanged();

        Entries.Add(entry);
        SelectedEntry = entry;
        return entry;
    }

    public T AddEntryFromFile(string filePath)
    {
        var entry = _importParser(filePath);
        _repository.Add(entry);
        _repository.Save();
        NotifyFileStatusChanged();

        Entries.Add(entry);
        SelectedEntry = entry;
        return entry;
    }

    public void ExportEntry(T entry, string filePath)
    {
        _exporter(entry, filePath);
    }

    public void DeleteEntries(IEnumerable<T> entries)
    {
        var toRemove = entries.ToList();
        if (toRemove.Count == 0)
        {
            return;
        }

        _repository.Remove(toRemove.Select(e => e.Id));
        _repository.Save();
        NotifyFileStatusChanged();

        foreach (var entry in toRemove)
        {
            Entries.Remove(entry);
        }

        if (SelectedEntry is not null && toRemove.Contains(SelectedEntry))
        {
            SelectedEntry = Entries.FirstOrDefault();
        }
    }

    public void SaveCurrentEntry()
    {
        _repository.Save();
        NotifyFileStatusChanged();
    }

    public void SaveFileAs(string filePath)
    {
        _repository.SaveAs(filePath);
        NotifyFileStatusChanged();
    }

    public void OpenFile(string filePath)
    {
        _repository.Open(filePath);

        Entries.Clear();
        foreach (var entry in _repository.Entries)
        {
            Entries.Add(entry);
        }

        SelectedEntry = Entries.FirstOrDefault();
        NotifyFileStatusChanged();
    }

    private void NotifyFileStatusChanged()
    {
        OnPropertyChanged(nameof(FileStatusText));
    }

    private static string FormatSize(long bytes)
    {
        if (bytes < 1024)
        {
            return $"{bytes} B";
        }

        if (bytes < 1024 * 1024)
        {
            return $"{bytes / 1024.0:0.#} KB";
        }

        return $"{bytes / (1024.0 * 1024.0):0.#} MB";
    }
}

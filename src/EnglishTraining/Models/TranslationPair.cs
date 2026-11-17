namespace EnglishTraining.Models;

/// <summary>One English sentence and its Korean translation (§32).</summary>
public sealed record TranslationPair(string English, string Korean);

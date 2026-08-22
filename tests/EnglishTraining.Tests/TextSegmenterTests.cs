using EnglishTraining.Models;
using EnglishTraining.Services;

namespace EnglishTraining.Tests;

public class TextSegmenterTests
{
    private static JsonExpressionRepository BuildRepository(params string[] texts)
    {
        var expressions = texts.Select(t => new LearningExpression { Text = t }).ToList();
        return new JsonExpressionRepository(expressions);
    }

    [Fact]
    public void Segment_PrefersLongestRegisteredExpression()
    {
        var repo = BuildRepository("look", "look forward", "look forward to");

        var segments = TextSegmenter.Segment("I look forward to seeing you.", repo);

        var match = Assert.Single(segments, s => s.IsMatch);
        Assert.Equal("look forward to", match.DisplayText);
        Assert.Equal("look forward to", match.Expression!.Text);
    }

    [Fact]
    public void Segment_PreservesOriginalCasingAndPunctuationInDisplayText()
    {
        var repo = BuildRepository("look forward to");

        var segments = TextSegmenter.Segment("Look forward to, everyone said.", repo);

        var match = Assert.Single(segments, s => s.IsMatch);
        Assert.Equal("Look forward to", match.DisplayText);
    }

    [Fact]
    public void Segment_DoesNotMatchInflectedForms()
    {
        var repo = BuildRepository("look forward to");

        var segments = TextSegmenter.Segment("She looked forward to it.", repo);

        Assert.DoesNotContain(segments, s => s.IsMatch);
    }

    [Fact]
    public void Segment_LeavesUnregisteredWordsAsPlainText()
    {
        var repo = BuildRepository("went to");

        var segments = TextSegmenter.Segment("I went to the office yesterday.", repo);

        Assert.Contains(segments, s => s.IsMatch && s.DisplayText == "went to");
        Assert.All(segments.Where(s => !s.IsMatch), s => Assert.Null(s.Expression));
    }

    [Fact]
    public void Segment_ConcatenatedDisplayTextReconstructsOriginal()
    {
        var repo = BuildRepository("look forward to", "wondering if");
        const string text = "I was wondering if you could help. I look forward to seeing you.";

        var segments = TextSegmenter.Segment(text, repo);

        Assert.Equal(text, string.Concat(segments.Select(s => s.DisplayText)));
    }

    [Fact]
    public void Segment_EmptyRepositoryProducesSinglePlainSegment()
    {
        var repo = BuildRepository();

        var segments = TextSegmenter.Segment("Just plain text.", repo);

        var segment = Assert.Single(segments);
        Assert.False(segment.IsMatch);
        Assert.Equal("Just plain text.", segment.DisplayText);
    }
}

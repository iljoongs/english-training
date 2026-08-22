using EnglishTraining.Services;

namespace EnglishTraining.Tests;

public class TextNormalizerTests
{
    [Theory]
    [InlineData("Look forward to,", "look forward to")]
    [InlineData("I", "i")]
    [InlineData("as   far as  I know", "as far as i know")]
    [InlineData("don't", "dont")]
    public void Normalize_LowercasesAndStripsPunctuation(string input, string expected)
    {
        Assert.Equal(expected, TextNormalizer.Normalize(input));
    }
}

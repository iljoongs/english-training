using EnglishTraining.Models;

namespace EnglishTraining.Services;

public static class DefaultLearningData
{
    public static List<InterpretationEntry> Interpretations() =>
    [
        new() { Id = Guid.NewGuid(), Text = "look", Ko = "보다" },
        new() { Id = Guid.NewGuid(), Text = "look forward", Ko = "기대하다 (불완전 표현, 테스트용)" },
        new() { Id = Guid.NewGuid(), Text = "look forward to", Ko = "~을 기대하다, ~을 고대하다" },
        new() { Id = Guid.NewGuid(), Text = "wondering if", Ko = "~인지 궁금하다" },
        new() { Id = Guid.NewGuid(), Text = "as far as I know", Ko = "내가 아는 한" },
    ];

    public static List<WritingEntry> Writings() =>
    [
        new()
        {
            Id = Guid.NewGuid(),
            Text = "look forward to",
            Description = "한국어 '~을 기대하다'를 영어로 표현",
            Example = "I look forward to seeing you.",
        },
        new()
        {
            Id = Guid.NewGuid(),
            Text = "wondering if",
            Description = "'~인지 궁금합니다'를 영어로 표현",
            Example = "I was wondering if you could help me with this problem.",
        },
    ];
}

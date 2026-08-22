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

    public static List<ExpressionEntry> Expressions() =>
    [
        new()
        {
            Id = Guid.NewGuid(),
            Text = "look forward to",
            Meaning = "~을 기대하다",
            Usage = "look forward to + 명사 / 동명사(-ing)",
            Example = "I look forward to meeting you.",
        },
        new()
        {
            Id = Guid.NewGuid(),
            Text = "be supposed to",
            Meaning = "~하기로 되어 있다",
            Usage = "be supposed to + 동사원형",
            Example = "You are supposed to submit the report by Friday.",
        },
        new()
        {
            Id = Guid.NewGuid(),
            Text = "as far as I know",
            Meaning = "내가 알기로는",
            Usage = "문장 앞/뒤에서 부사구로 사용",
            Example = "As far as I know, the meeting is scheduled for tomorrow.",
        },
    ];
}

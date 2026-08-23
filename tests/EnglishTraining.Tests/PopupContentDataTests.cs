using EnglishTraining.Models;
using EnglishTraining.ViewModels;

namespace EnglishTraining.Tests;

public class PopupContentDataTests
{
    [Fact]
    public void InterpretationHeader_WithPartOfSpeech_JoinsWithComma()
    {
        var data = new PopupContentData
        {
            Title = "deadline",
            Interpretation = new InterpretationInfo { PartOfSpeech = "n", Ko = "마감일, 기한" },
        };

        Assert.Equal("n, 마감일, 기한", data.InterpretationHeader);
    }

    [Fact]
    public void InterpretationHeader_WithoutPartOfSpeech_IsJustKo()
    {
        var data = new PopupContentData
        {
            Title = "I was wondering if you could help me with this.",
            Interpretation = new InterpretationInfo { Ko = "이것 좀 도와주실 수 있는지 궁금했어요." },
        };

        Assert.Equal("이것 좀 도와주실 수 있는지 궁금했어요.", data.InterpretationHeader);
    }

    [Fact]
    public void InterpretationHeader_NoInterpretation_IsNull()
    {
        var data = new PopupContentData { Title = "look" };

        Assert.Null(data.InterpretationHeader);
    }
}

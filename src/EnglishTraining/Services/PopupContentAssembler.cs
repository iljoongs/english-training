using EnglishTraining.Models;

namespace EnglishTraining.Services;

public static class PopupContentAssembler
{
    public static bool TryBuildSections(
        LearningExpression expression,
        bool showInterpretation,
        bool showWriting,
        out InterpretationInfo? interpretation,
        out WritingInfo? writing)
    {
        interpretation = showInterpretation ? expression.Interpretation : null;
        writing = showWriting ? expression.Writing : null;

        return interpretation != null || writing != null;
    }
}

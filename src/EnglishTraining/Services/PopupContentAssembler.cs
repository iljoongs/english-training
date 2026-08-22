using EnglishTraining.Models;

namespace EnglishTraining.Services;

public static class PopupContentAssembler
{
    public static bool TryBuildSections(
        LearningExpression expression,
        bool showInterpretation,
        bool showWriting,
        bool showExpression,
        out InterpretationInfo? interpretation,
        out WritingInfo? writing,
        out ExpressionInfo? expressionInfo)
    {
        interpretation = showInterpretation ? expression.Interpretation : null;
        writing = showWriting ? expression.Writing : null;
        expressionInfo = showExpression ? expression.Expression : null;

        return interpretation != null || writing != null || expressionInfo != null;
    }
}

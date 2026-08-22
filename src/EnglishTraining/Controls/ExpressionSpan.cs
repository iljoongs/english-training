using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;
using EnglishTraining.Models;

namespace EnglishTraining.Controls;

public sealed class ExpressionSpan : TextBlock
{
    private static readonly TimeSpan ShowDelay = TimeSpan.FromMilliseconds(250);

    private DispatcherTimer? _showTimer;

    public LearningExpression Expression { get; }

    public event EventHandler? HoverStarted;
    public event EventHandler? HoverEnded;

    public ExpressionSpan(string displayText, LearningExpression expression)
    {
        Expression = expression;
        Text = displayText;
        Cursor = Cursors.Help;

        MouseEnter += OnMouseEnter;
        MouseLeave += OnMouseLeave;
    }

    private void OnMouseEnter(object sender, MouseEventArgs e)
    {
        _showTimer?.Stop();
        var timer = new DispatcherTimer { Interval = ShowDelay };
        timer.Tick += (_, _) =>
        {
            timer.Stop();
            if (IsMouseOver)
            {
                HoverStarted?.Invoke(this, EventArgs.Empty);
            }
        };
        _showTimer = timer;
        timer.Start();
    }

    private void OnMouseLeave(object sender, MouseEventArgs e)
    {
        _showTimer?.Stop();
        _showTimer = null;
        HoverEnded?.Invoke(this, EventArgs.Empty);
    }
}

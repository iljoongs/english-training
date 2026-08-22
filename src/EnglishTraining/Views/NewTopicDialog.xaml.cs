using System.Windows;

namespace EnglishTraining.Views;

public partial class NewTopicDialog : Window
{
    public NewTopicDialog(string windowTitle = "새 주제 추가", string labelText = "주제명")
    {
        InitializeComponent();
        Title = windowTitle;
        LabelTextBlock.Text = labelText;
        Loaded += (_, _) => TitleTextBox.Focus();
    }

    public string EnteredText { get; private set; } = string.Empty;

    private void OnOkClick(object sender, RoutedEventArgs e)
    {
        var text = TitleTextBox.Text.Trim();
        if (string.IsNullOrEmpty(text))
        {
            MessageBox.Show(this, "값을 입력하세요.", "알림", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        EnteredText = text;
        DialogResult = true;
    }
}

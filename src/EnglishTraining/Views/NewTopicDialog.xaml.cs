using System.Windows;

namespace EnglishTraining.Views;

public partial class NewTopicDialog : Window
{
    public NewTopicDialog(string windowTitle = "Add New Topic", string labelText = "Topic Name")
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
            MessageBox.Show(this, "Please enter a value.", "Notice", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        EnteredText = text;
        DialogResult = true;
    }
}

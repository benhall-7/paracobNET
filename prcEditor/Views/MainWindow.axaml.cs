using Avalonia.Controls;
using Avalonia.Interactivity;
using prcEditor.ViewModels;

namespace prcEditor.Views;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        var viewModel = new MainWindowViewModel(this);
        DataContext = viewModel;

        Opened += (_, _) => viewModel.OnWindowOpened();
    }

    private void CommitValueEditorOnLostFocus(object? sender, RoutedEventArgs e)
    {
        switch ((sender as Control)?.DataContext)
        {
            case NumericOrStringEditorViewModel numericOrString:
                numericOrString.Commit();
                break;
            case Hash40EditorViewModel hash40:
                hash40.Commit();
                break;
        }
    }
}
using Avalonia.Controls;
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
}
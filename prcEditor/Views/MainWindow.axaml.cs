using Avalonia.Controls;
using prcEditor.ViewModels;

namespace prcEditor.Views;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        DataContext = new MainWindowViewModel(this);
    }
}
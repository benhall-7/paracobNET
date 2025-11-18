using Avalonia.Controls;
using perky.ViewModels;

namespace perky.Views;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        DataContext = new MainWindowViewModel(this);
    }
}
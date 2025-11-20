using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Input;
using Avalonia.Controls;
using Avalonia.Platform.Storage;
using paracobNET;
using prcEditor.Services;

namespace prcEditor.ViewModels;

public class MainWindowViewModel : INotifyPropertyChanged
{
    private readonly Window _owner; // for dialogs
    private ParamTreeNodeViewModel? _root;
    private ParamTreeNodeViewModel? _selectedNode;
    private ParamContainer? _container;

    public Labels Labels { get; } = new();

    public event PropertyChangedEventHandler? PropertyChanged;
    private event Action? SelectedNodeChanged;

    public ParamTreeNodeViewModel? Root
    {
        get => _root;
        set
        {
            if (!ReferenceEquals(_root, value))
            {
                _root = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(HasDocument));
                SaveFileCommand.RaiseCanExecuteChanged();
                LoadLabelsCommand.RaiseCanExecuteChanged();
            }
        }
    }

    public bool HasDocument => Root is not null;

    public ParamTreeNodeViewModel? SelectedNode
    {
        get => _selectedNode;
        set
        {
            if (!ReferenceEquals(_selectedNode, value))
            {
                _selectedNode = value;
                OnPropertyChanged();
                SelectedNodeChanged?.Invoke();
            }
        }
    }

    public ObservableCollection<DataGridRowViewModel> Rows { get; } = new();

    // Commands
    public RelayCommand OpenFileCommand { get; }
    public RelayCommand SaveFileCommand { get; }
    public RelayCommand LoadLabelsCommand { get; }

    public MainWindowViewModel(Window owner)
    {
        _owner = owner;

        OpenFileCommand = new RelayCommand(async _ => await OpenFileAsync(), _ => true);
        SaveFileCommand = new RelayCommand(async _ => await SaveFileAsync(), _ => HasDocument);
        LoadLabelsCommand = new RelayCommand(async _ => await LoadLabelsAsync(), _ => true);

        SelectedNodeChanged += OnUpdateSelectedParam;
    }

    ~MainWindowViewModel()
    {
        SelectedNodeChanged -= OnUpdateSelectedParam;
    }

    private async Task OpenFileAsync()
    {

        var result = await _owner.StorageProvider.OpenFilePickerAsync(
            new FilePickerOpenOptions
            {
                AllowMultiple = false,
                FileTypeFilter =
                [
                    new FilePickerFileType("PRC files") { Patterns = ["*.prc", "*.prcx", "*.stdat", "*.stdatx", "*.stprm", "*.stprmx"] },
                    new FilePickerFileType("All files") { Patterns = ["*"] }
                ]
            }
        );

        if (result is null || result.Count == 0)
            return;

        var path = result[0].Path.LocalPath;

        // Load via your library – adjust to your actual API name
        // e.g. ParamSerializer.Load(path) returning ParamMapNode
        ParamContainer container = new ParamContainer(path);

        _container = container;
        Root = ParamTreeBuilder.BuildContainer(container, Labels);
        SelectedNode = Root;
    }

    private async Task SaveFileAsync()
    {
        if (_container is null)
            return;

        var result = await _owner.StorageProvider.SaveFilePickerAsync(
            new FilePickerSaveOptions
            {
                SuggestedFileName = "modified.prc",
                FileTypeChoices =
                [
                    new FilePickerFileType("PRC files") { Patterns = ["*.prc", "*.prcx", "*.stdat", "*.stdatx", "*.stprm", "*.stprmx"] },
                    new FilePickerFileType("All files") { Patterns = ["*"] }
                ],
            }
        );

        var path = result?.Path.LocalPath;
        if (string.IsNullOrWhiteSpace(path))
            return;

        _container.SaveFile(path);
    }

    private async Task LoadLabelsAsync()
    {
        var result = await _owner.StorageProvider.OpenFilePickerAsync(
            new FilePickerOpenOptions
            {
                AllowMultiple = false,
                FileTypeFilter =
                [
                    new FilePickerFileType("Label files") { Patterns = ["*.csv"] },
                    new FilePickerFileType("All files") { Patterns = ["*"] }
                ]
            }
        );

        if (result is null || result.Count == 0)
            return;

        var path = result[0].Path.LocalPath;
        Labels.LoadFromFile(path);
    }

    private void OnUpdateSelectedParam()
    {
        Rows.Clear();
        if (SelectedNode != null)
        {
            switch (SelectedNode.Node)
            {
                case ParamArrayNode:
                case ParamMapNode:
                    foreach (var child in SelectedNode.Children)
                    {
                        Rows.Add(new DataGridRowViewModel(child.Accessor, child.Node, Labels));
                    }
                    break;
                default:
                    Rows.Add(new DataGridRowViewModel(SelectedNode.Accessor, SelectedNode.Node, Labels));
                    break;
            }
        }
    }

    public void OnWindowOpened()
    {
        var baseDir = AppContext.BaseDirectory;
        var labelsPath = Path.Combine(baseDir, "ParamLabels.csv");

        if (File.Exists(labelsPath))
        {
            try
            {
                Labels.LoadFromFile(labelsPath);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to load default labels: {ex}");
            }
        }
    }

    protected void OnPropertyChanged([CallerMemberName] string? name = null) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}

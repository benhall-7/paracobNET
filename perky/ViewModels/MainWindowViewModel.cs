using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Input;
using Avalonia.Controls;
using Avalonia.Platform.Storage;
using paracobNET;
using perky.Services;

namespace perky.ViewModels;

public class MainWindowViewModel : INotifyPropertyChanged
{
    private readonly Window _owner; // for dialogs
    private ParamTreeNodeViewModel? _root;
    private ParamTreeNodeViewModel? _selectedNode;
    private ParamContainer? _container;

    public event PropertyChangedEventHandler? PropertyChanged;

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
                OnPropertyChanged(nameof(FocusedParam));
            }
        }
    }

    public ParamNode? FocusedParam => SelectedNode?.Node;

    // Commands
    public RelayCommand OpenFileCommand { get; }
    public RelayCommand SaveFileCommand { get; }
    public RelayCommand LoadLabelsCommand { get; }

    public MainWindowViewModel(Window owner)
    {
        _owner = owner;

        OpenFileCommand = new RelayCommand(async _ => await OpenFileAsync(), _ => true);
        SaveFileCommand = new RelayCommand(async _ => await SaveFileAsync(), _ => HasDocument);
        LoadLabelsCommand = new RelayCommand(async _ => await LoadLabelsAsync(), _ => HasDocument);
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
        ParamContainer rootNode = new ParamContainer(path);

        _container = rootNode;
        Root = ParamTreeBuilder.BuildContainer(rootNode, System.IO.Path.GetFileName(path));
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
        // TODO: later:
        // - open labels file (or download from GitHub)
        // - build a Dictionary<Hash40, string>
        // - rebuild the Root tree with ParamTreeBuilder.Build(rootNode, labels)
        await Task.CompletedTask;
    }

    protected void OnPropertyChanged([CallerMemberName] string? name = null) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}

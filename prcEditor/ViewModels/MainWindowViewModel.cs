using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Input;
using Avalonia.Controls;
using Avalonia.Platform.Storage;
using paracobNET;
using paracobNET.Xml;
using prcEditor.Services;

namespace prcEditor.ViewModels;

public class MainWindowViewModel : INotifyPropertyChanged
{
    private readonly Window _owner; // for dialogs
    private ParamTreeNodeViewModel? _root;
    private ParamSearchViewModel _paramSearch;
    private ParamTreeNodeViewModel? _selectedNode;
    private XmlEditSessionViewModel? _activeXmlEditSession;
    private ParamContainer? _container;

    public Labels Labels { get; } = new();

    public ParamSearchViewModel ParamSearch
    {
        get => _paramSearch;
        set
        {
            if (!ReferenceEquals(_paramSearch, value))
            {
                _paramSearch = value;
                OnPropertyChanged();
            }
        }
    }

    public event PropertyChangedEventHandler? PropertyChanged;
    private event Action? SelectedNodeChanged;

    public event EventHandler? RootChanged;
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
                OnPropertyChanged(nameof(CanSaveDocument));
                SaveFileCommand.RaiseCanExecuteChanged();
                LoadLabelsCommand.RaiseCanExecuteChanged();
                RootChanged?.Invoke(this, EventArgs.Empty);
            }
        }
    }

    public ParamTreeNodeViewModel? SelectedNode
    {
        get => _selectedNode;
        set
        {
            if (!ReferenceEquals(_selectedNode, value))
            {
                _selectedNode = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(HasSelectedNode));
                OnPropertyChanged(nameof(CanEditSelectedNodeAsXml));
                EditSelectedNodeAsXmlCommand.RaiseCanExecuteChanged();
                SelectedNodeChanged?.Invoke();
            }
        }
    }

    public XmlEditSessionViewModel? ActiveXmlEditSession
    {
        get => _activeXmlEditSession;
        private set
        {
            if (ReferenceEquals(_activeXmlEditSession, value)) return;
            _activeXmlEditSession = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(IsXmlEditSessionActive));
            OnPropertyChanged(nameof(IsMainUiEnabled));
            OnPropertyChanged(nameof(CanSaveDocument));
            OnPropertyChanged(nameof(CanEditSelectedNodeAsXml));
            RaiseCommandCanExecuteChanged();
        }
    }

    public ObservableCollection<DataGridRowViewModel> Rows { get; } = new();

    public bool HasDocument => Root is not null;
    public bool HasSelectedNode => SelectedNode is not null;
    public bool IsXmlEditSessionActive => ActiveXmlEditSession is not null;
    public bool IsMainUiEnabled => !IsXmlEditSessionActive;
    public bool CanSaveDocument => HasDocument && IsMainUiEnabled;
    public bool CanEditSelectedNodeAsXml => HasSelectedNode && IsMainUiEnabled;

    // Commands
    public RelayCommand OpenFileCommand { get; }
    public RelayCommand SaveFileCommand { get; }
    public RelayCommand LoadLabelsCommand { get; }
    public RelayCommand EditSelectedNodeAsXmlCommand { get; }

    public MainWindowViewModel(Window owner)
    {
        _owner = owner;

        _paramSearch = new ParamSearchViewModel(
            root: _root,
            labels: Labels,
            selectNode: node => SelectedNode = node);

        OpenFileCommand = new RelayCommand(async _ => await OpenFileAsync(), _ => IsMainUiEnabled);
        SaveFileCommand = new RelayCommand(async _ => await SaveFileAsync(), _ => CanSaveDocument);
        LoadLabelsCommand = new RelayCommand(async _ => await LoadLabelsAsync(), _ => IsMainUiEnabled);
        EditSelectedNodeAsXmlCommand = new RelayCommand(_ => EditSelectedNodeAsXml(), _ => CanEditSelectedNodeAsXml);

        SelectedNodeChanged += OnUpdateSelectedNode;
        RootChanged += OnRootChanged;
    }

    ~MainWindowViewModel()
    {
        SelectedNodeChanged -= OnUpdateSelectedNode;
        RootChanged -= OnRootChanged;
    }

    private async Task OpenFileAsync()
    {
        if (!IsMainUiEnabled)
            return;


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
    }

    private async Task SaveFileAsync()
    {
        if (_container is null || !IsMainUiEnabled)
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
        if (!IsMainUiEnabled)
            return;

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

    private void EditSelectedNodeAsXml()
    {
        if (SelectedNode is null || IsXmlEditSessionActive)
            return;

        var targetNode = SelectedNode;
        var tempPath = CreateTempXmlPath();
        try
        {
            ParamXmlSerializer.Save(targetNode.Node, tempPath, Labels.HashToStringLabels);
            var initialStatus = TryOpenExternalFile(tempPath);

            ActiveXmlEditSession = new XmlEditSessionViewModel(
                targetText: GetNodeRouteText(targetNode),
                filePath: tempPath,
                commit: () => ApplyXmlEdit(tempPath, targetNode),
                check: () => CheckXmlEdit(tempPath, targetNode),
                cancel: () => EndXmlEditSession(deleteTempFile: true),
                initialStatus: initialStatus);
        }
        catch
        {
            TryDeleteTempFile(tempPath);
            throw;
        }
    }

    private string? ApplyXmlEdit(string path, ParamTreeNodeViewModel targetNode)
    {
        try
        {
            var replacement = ParamXmlSerializer.Load(path, Labels.StringToHashLabels);
            ValidateXmlReplacementTarget(replacement, targetNode);
            ReplaceNode(replacement, targetNode);
            EndXmlEditSession(deleteTempFile: true);
            return null;
        }
        catch (Exception ex)
        {
            return ex.Message;
        }
    }

    private string? CheckXmlEdit(string path, ParamTreeNodeViewModel targetNode)
    {
        try
        {
            var replacement = ParamXmlSerializer.Load(path, Labels.StringToHashLabels);
            ValidateXmlReplacementTarget(replacement, targetNode);
            return null;
        }
        catch (Exception ex)
        {
            return ex.Message;
        }
    }

    private static void ValidateXmlReplacementTarget(ParamNode replacement, ParamTreeNodeViewModel targetNode)
    {
        if ((targetNode.Parent is null || targetNode.Accessor is NoAccessor) &&
            replacement is not ParamMapNode)
        {
            throw new InvalidDataException("The root XML node must be a struct.");
        }
    }

    private void ReplaceNode(ParamNode replacement, ParamTreeNodeViewModel targetNode)
    {
        if (_container is null)
            throw new InvalidOperationException("No param document is open.");

        if (targetNode.Parent is null || targetNode.Accessor is NoAccessor)
        {
            var mapRoot = (ParamMapNode)replacement;

            _container.Root = mapRoot;
            targetNode.ReplaceNode(mapRoot);
            RefreshAfterTreeMutation(targetNode);
            return;
        }

        var parent = targetNode.Parent;
        int childIndex = parent.Children.IndexOf(targetNode);
        if (childIndex < 0)
            throw new InvalidOperationException("Could not locate the XML edit target in its parent.");

        switch (parent.Node)
        {
            case ParamMapNode mapNode:
                var key = mapNode.Entries[childIndex].Key;
                mapNode.Entries[childIndex] = new KeyValuePair<Hash40, ParamNode>(key, replacement);
                break;
            case ParamArrayNode arrayNode:
                arrayNode[childIndex] = replacement;
                break;
            default:
                throw new InvalidOperationException("The selected node parent is not editable.");
        }

        targetNode.ReplaceNode(replacement);
        RefreshAfterTreeMutation(targetNode);
    }

    private void RefreshAfterTreeMutation(ParamTreeNodeViewModel targetNode)
    {
        if (!ReferenceEquals(SelectedNode, targetNode))
            SelectedNode = targetNode;
        else
            OnUpdateSelectedNode();

        ParamSearch = new ParamSearchViewModel(
            root: _root,
            labels: Labels,
            selectNode: node => SelectedNode = node);
    }

    private string GetNodeRouteText(ParamTreeNodeViewModel node)
    {
        var route = new Stack<string>();
        var current = node;
        while (current is not null)
        {
            route.Push(Labels.GetLabel(current.Accessor));
            current = current.Parent;
        }

        return string.Join("/", route);
    }

    private static string CreateTempXmlPath()
    {
        var directory = Path.Combine(Path.GetTempPath(), "prcEditor");
        Directory.CreateDirectory(directory);
        var guid = Guid.NewGuid();
        return Path.Combine(directory, $"{guid:N}.xml");
    }

    private static string? TryOpenExternalFile(string path)
    {
        try
        {
            ExternalFileLauncher.Open(path);
            return null;
        }
        catch (Exception ex)
        {
            return $"Failed to open XML file in the default editor: {ex.Message}";
        }
    }

    private static void TryDeleteTempFile(string path)
    {
        try
        {
            if (File.Exists(path))
                File.Delete(path);
        }
        catch
        {
            // Don't crash if we're not allowed to delete the tempfile.
        }
    }

    private void EndXmlEditSession(bool deleteTempFile)
    {
        var session = ActiveXmlEditSession;
        ActiveXmlEditSession = null;

        if (deleteTempFile && session is not null)
            TryDeleteTempFile(session.FilePath);
    }

    private void RaiseCommandCanExecuteChanged()
    {
        OpenFileCommand.RaiseCanExecuteChanged();
        SaveFileCommand.RaiseCanExecuteChanged();
        LoadLabelsCommand.RaiseCanExecuteChanged();
        EditSelectedNodeAsXmlCommand.RaiseCanExecuteChanged();
    }

    private void OnRootChanged(object? sender, EventArgs e)
    {
        SelectedNode = Root;
        ParamSearch = new ParamSearchViewModel(
            root: _root,
            labels: Labels,
            selectNode: node => SelectedNode = node);
    }

    private void OnUpdateSelectedNode()
    {
        SelectedNode?.ExpandParents();
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

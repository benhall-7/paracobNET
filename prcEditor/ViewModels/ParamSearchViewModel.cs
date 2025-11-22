using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using paracobNET;
using paracobNET.Extensions;
using prcEditor.Services;

namespace prcEditor.ViewModels;

public class HashSearchResult
{
    private Labels _labels;
    public ParamTreeNodeViewModel Node { get; }
    public List<Accessor> Route { get; }
    // matched on the parent map's key hash
    public bool IsKeyMatch { get; }
    // matched on the node's Hash40 value
    public bool IsValueMatch { get; }

    public string RouteText => string.Join("/", Route.Select(_labels.GetLabel));
    public string DisplayText => $"{_labels.GetLabel(Node.Accessor)}: {Node.Node.ToPrettyString(_labels.HashToStringLabels)}";

    public HashSearchResult(ParamTreeNodeViewModel node, List<Accessor> route, bool isKeyMatch, bool isValueMatch, Labels labels)
    {
        _labels = labels;
        Node = node;
        Route = route;
        IsKeyMatch = isKeyMatch;
        IsValueMatch = isValueMatch;
    }
}

public class ParamSearchViewModel : INotifyPropertyChanged
{
    private readonly ParamTreeNodeViewModel? _root;

    private Labels _labels;
    private readonly Action<ParamTreeNodeViewModel> _selectNode; // callback to main VM
    public event PropertyChangedEventHandler? PropertyChanged;

    private string _query = "";
    public string Query
    {
        get => _query;
        set
        {
            if (_query != value)
            {
                _query = value;
                OnPropertyChanged();
            }
        }
    }

    private Hash40? _queryHash => Extensions.TryFromLabelOrHex(_query, _labels.StringToHashLabels);

    private bool _exactMatch;
    public bool ExactMatch
    {
        get => _exactMatch;
        set
        {
            if (_exactMatch != value)
            {
                _exactMatch = value;
                OnPropertyChanged();
            }
        }
    }

    private HashSearchResult? _selectedResult;
    private event EventHandler? SelectedResultChanged;
    public HashSearchResult? SelectedResult
    {
        get => _selectedResult;
        set
        {
            if (_selectedResult == value) return;
            _selectedResult = value;
            OnPropertyChanged();
            SelectedResultChanged?.Invoke(this, EventArgs.Empty);
        }
    }

    public ObservableCollection<HashSearchResult> Results { get; } = new();
    public RelayCommand RunSearchCommand { get; }
    public RelayCommand SelectResultCommand { get; }

    public ParamSearchViewModel(
        ParamTreeNodeViewModel? root,
        Action<ParamTreeNodeViewModel> selectNode,
        Labels labels)
    {
        _root = root;
        _selectNode = selectNode;
        _labels = labels;
        _exactMatch = true;

        RunSearchCommand = new RelayCommand(_ => RunSearch());
        SelectResultCommand = new RelayCommand(
            param =>
            {
                if (param is HashSearchResult result)
                {
                    _selectNode(result.Node);
                }
            });

        _labels.LabelsChanged += OnLabelsChanged;
        SelectedResultChanged += OnSelectedResultChanged;
    }

    ~ParamSearchViewModel()
    {
        _labels.LabelsChanged -= OnLabelsChanged;
        SelectedResultChanged -= OnSelectedResultChanged;
    }

    private void RunSearch()
    {
        Results.Clear();
        // a reasonable max capacity (think tree depth) for most cases
        List<Accessor> stack = new(16);
        if (_root is not null)
            SearchTree(_root, ref stack);
    }

    private void SearchTree(ParamTreeNodeViewModel node, ref List<Accessor> stack)
    {
        if (node.Accessor is Hash40Accessor hash40Accessor)
        {
            if (Matches(_query, hash40Accessor.Value))
            {
                Results.Add(new HashSearchResult(
                    node,
                    stack.ToList(),
                    true,
                    false,
                    _labels));
            }
        }

        // check Hash40 value
        if (node.Node is ParamHash40Node hashNode)
        {
            if (Matches(_query, hashNode.Value))
            {
                Results.Add(new HashSearchResult(
                    node,
                    stack.ToList(),
                    false,
                    true,
                    _labels));
            }
        }

        // recurse into any children
        foreach (var child in node.Children)
        {
            stack.Add(child.Accessor);
            SearchTree(child, ref stack);
            stack.RemoveAt(stack.Count - 1);
        }
    }

    private bool Matches(string query, Hash40 hash)
    {
        if (_exactMatch)
        {
            if (_queryHash is null)
                return false;
            return hash == _queryHash;
        }
        else
        {
            var qStr = query.ToLowerInvariant();
            var hashStr = hash.ToLabelOrHex(_labels.HashToStringLabels).ToLowerInvariant();
            return hashStr.Contains(qStr);
        }
    }

    protected void OnPropertyChanged([CallerMemberName] string? name = null) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

    private void OnLabelsChanged()
    {
        OnPropertyChanged(nameof(_queryHash));
        OnPropertyChanged(nameof(Results));
    }

    private void OnSelectedResultChanged(object? sender, EventArgs e)
    {
        if (SelectedResult is not null)
        {
            _selectNode(SelectedResult.Node);
        }
    }
}

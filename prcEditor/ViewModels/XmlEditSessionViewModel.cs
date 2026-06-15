using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Avalonia.Media;
using prcEditor.Services;

namespace prcEditor.ViewModels;

public sealed class XmlEditSessionViewModel : INotifyPropertyChanged
{
    private readonly Func<string?> _commit;
    private readonly Func<string?> _check;
    private readonly Action _cancel;
    private string _status;
    private bool _hasError;

    public XmlEditSessionViewModel(
        string targetText,
        string filePath,
        Func<string?> commit,
        Func<string?> check,
        Action cancel,
        string? initialStatus = null)
    {
        TargetText = targetText;
        FilePath = filePath;
        _commit = commit;
        _check = check;
        _cancel = cancel;
        _status = string.IsNullOrWhiteSpace(initialStatus)
            ? "XML file opened in default editor"
            : initialStatus;
        _hasError = !string.IsNullOrWhiteSpace(initialStatus);

        CommitCommand = new RelayCommand(_ => Commit());
        CheckCommand = new RelayCommand(_ => Check());
        CancelCommand = new RelayCommand(_ => _cancel());
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    public string TargetText { get; }
    public string FilePath { get; }

    public string Status
    {
        get => _status;
        private set
        {
            if (_status == value)
                return;

            _status = value;
            OnPropertyChanged();
        }
    }

    public bool HasError
    {
        get => _hasError;
        private set
        {
            if (_hasError == value)
                return;

            _hasError = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(StatusBackground));
            OnPropertyChanged(nameof(StatusBorderBrush));
            OnPropertyChanged(nameof(StatusForeground));
        }
    }

    public IBrush StatusBackground => HasError ? Brushes.MistyRose : Brushes.Transparent;
    public IBrush StatusBorderBrush => HasError ? Brushes.Firebrick : Brushes.LightGray;
    public IBrush StatusForeground => HasError ? Brushes.Firebrick : Brushes.Black;

    public RelayCommand CommitCommand { get; }
    public RelayCommand CheckCommand { get; }
    public RelayCommand CancelCommand { get; }

    private void Commit()
    {
        var error = _commit();
        if (error is null)
            return;

        HasError = true;
        Status = error;
    }

    private void Check()
    {
        var error = _check();
        HasError = error is not null;
        Status = error ?? "XML data valid";
    }

    private void OnPropertyChanged([CallerMemberName] string? propertyName = null) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
}
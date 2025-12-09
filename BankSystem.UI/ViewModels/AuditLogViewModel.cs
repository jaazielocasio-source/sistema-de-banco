using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace BankSystem.UI.ViewModels;

public partial class AuditLogViewModel : ObservableObject
{
    private readonly Services.DialogService _dialogs;

    [ObservableProperty] private string _logContent = string.Empty;
    [ObservableProperty] private string? _filter;

    private readonly string _path = "./logs/audit.log";

    public AuditLogViewModel(Services.DialogService dialogs)
    {
        _dialogs = dialogs;
        Refresh();
    }

    [RelayCommand]
    private void Refresh()
    {
        if (!File.Exists(_path))
        {
            LogContent = "Log vacío.";
            return;
        }
        var text = File.ReadAllText(_path);
        if (!string.IsNullOrWhiteSpace(Filter))
        {
            var lines = text.Split(Environment.NewLine);
            text = string.Join(Environment.NewLine, lines.Where(l => l.Contains(Filter, StringComparison.OrdinalIgnoreCase)));
        }
        LogContent = text;
    }
}

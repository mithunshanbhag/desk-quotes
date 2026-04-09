using Microsoft.Win32;

namespace DeskQuotes.Services.Implementations;

public interface IStartupLaunchStore
{
    string? GetValue(string valueName);
    void SetValue(string valueName, string value);
    void DeleteValue(string valueName);
}

internal sealed class CurrentUserRunStartupLaunchStore(string subKeyPath) : IStartupLaunchStore
{
    private readonly string _subKeyPath = string.IsNullOrWhiteSpace(subKeyPath)
        ? throw new ArgumentException("The Run registry key path must be provided.", nameof(subKeyPath))
        : subKeyPath;

    public string? GetValue(string valueName)
    {
        using var runKey = Registry.CurrentUser.OpenSubKey(_subKeyPath, false);
        return runKey?.GetValue(valueName) as string;
    }

    public void SetValue(string valueName, string value)
    {
        using var runKey = Registry.CurrentUser.CreateSubKey(_subKeyPath, true);
        runKey.SetValue(valueName, value, RegistryValueKind.String);
    }

    public void DeleteValue(string valueName)
    {
        using var runKey = Registry.CurrentUser.OpenSubKey(_subKeyPath, true);
        runKey?.DeleteValue(valueName, false);
    }
}

public class StartupLaunchService
{
    private readonly string _startupCommand;
    private readonly string _startupValueName;
    private readonly IStartupLaunchStore _store;

    public StartupLaunchService()
        : this(
            new CurrentUserRunStartupLaunchStore(AppConstants.RunAtSignInRegistryKeyPath),
            AppConstants.RunAtSignInRegistryValueName,
            Environment.ProcessPath)
    {
    }

    public StartupLaunchService(IStartupLaunchStore store, string startupValueName, string? processPath)
    {
        _store = store ?? throw new ArgumentNullException(nameof(store));
        _startupValueName = string.IsNullOrWhiteSpace(startupValueName)
            ? throw new ArgumentException("The startup registry value name must be provided.", nameof(startupValueName))
            : startupValueName;
        _startupCommand = BuildStartupCommand(processPath);
    }

    public virtual bool IsEnabled()
    {
        return !string.IsNullOrWhiteSpace(_store.GetValue(_startupValueName));
    }

    public virtual void Enable()
    {
        _store.SetValue(_startupValueName, _startupCommand);
    }

    public virtual void Disable()
    {
        _store.DeleteValue(_startupValueName);
    }

    private static string BuildStartupCommand(string? processPath)
    {
        var resolvedProcessPath = string.IsNullOrWhiteSpace(processPath)
            ? Path.Combine(AppContext.BaseDirectory, $"{AppConstants.AppName}.exe")
            : processPath.Trim();

        return $"\"{resolvedProcessPath.Trim('\"')}\"";
    }
}

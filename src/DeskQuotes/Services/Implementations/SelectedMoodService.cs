namespace DeskQuotes.Services.Implementations;

public class SelectedMoodService
{
    private readonly string _stateFilePath;
    private string? _selectedMood;
    private string? _startupWarningMessage;

    public SelectedMoodService()
        : this(CreateDefaultStateFilePath())
    {
    }

    public SelectedMoodService(string stateFilePath)
    {
        _stateFilePath = string.IsNullOrWhiteSpace(stateFilePath)
            ? throw new ArgumentException("The mood state file path must be provided.", nameof(stateFilePath))
            : stateFilePath;
        _selectedMood = LoadSelectedMood();
    }

    public virtual string? GetSelectedMood()
    {
        return _selectedMood;
    }

    public virtual string? GetStartupWarningMessage()
    {
        return _startupWarningMessage;
    }

    public virtual void SetSelectedMood(string? mood)
    {
        var normalizedMood = NormalizeMood(mood);
        PersistSelectedMood(normalizedMood);
        _selectedMood = normalizedMood;
    }

    private static string CreateDefaultStateFilePath()
    {
        var applicationDataDirectory = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            AppConstants.AppName);
        Directory.CreateDirectory(applicationDataDirectory);
        return Path.Combine(applicationDataDirectory, AppConstants.SelectedMoodStateFileName);
    }

    private string? LoadSelectedMood()
    {
        try
        {
            return File.Exists(_stateFilePath)
                ? NormalizeMood(File.ReadAllText(_stateFilePath))
                : null;
        }
        catch (IOException)
        {
            _startupWarningMessage = "Unable to read the persisted mood selection. Starting with All Quotes.";
            return null;
        }
        catch (UnauthorizedAccessException)
        {
            _startupWarningMessage = "Unable to read the persisted mood selection. Starting with All Quotes.";
            return null;
        }
    }

    private static string? NormalizeMood(string? mood)
    {
        return string.IsNullOrWhiteSpace(mood)
            ? null
            : mood.Trim();
    }

    private void PersistSelectedMood(string? mood)
    {
        if (mood is null)
        {
            if (File.Exists(_stateFilePath))
                File.Delete(_stateFilePath);

            return;
        }

        var stateDirectory = Path.GetDirectoryName(_stateFilePath);
        if (!string.IsNullOrWhiteSpace(stateDirectory))
            Directory.CreateDirectory(stateDirectory);

        File.WriteAllText(_stateFilePath, mood);
    }
}

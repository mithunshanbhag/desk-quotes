namespace DeskQuotes.Services.Implementations;

public class SelectedMoodService
{
    private readonly ILogger<SelectedMoodService> _logger;
    private readonly string _stateFilePath;
    private string? _selectedMood;
    private string? _startupWarningMessage;

    public SelectedMoodService()
        : this(CreateDefaultStateFilePath(), null)
    {
    }

    public SelectedMoodService(ILogger<SelectedMoodService> logger)
        : this(CreateDefaultStateFilePath(), logger)
    {
    }

    public SelectedMoodService(string stateFilePath, ILogger<SelectedMoodService>? logger = null)
    {
        _logger = logger ?? NullLogger<SelectedMoodService>.Instance;
        _stateFilePath = string.IsNullOrWhiteSpace(stateFilePath)
            ? throw new ArgumentException("The mood state file path must be provided.", nameof(stateFilePath))
            : stateFilePath;
        _selectedMood = LoadSelectedMood();
        _logger.LogDebug("Loaded persisted mood selection: {SelectedMood}.", _selectedMood ?? AppConstants.AllQuotesMoodMenuLabel);
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
        _logger.LogInformation("Updated selected mood to {SelectedMood}.", normalizedMood ?? AppConstants.AllQuotesMoodMenuLabel);
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
            if (!File.Exists(_stateFilePath))
            {
                _logger.LogDebug("No persisted mood selection exists at {StateFilePath}.", _stateFilePath);
                return null;
            }

            var selectedMood = NormalizeMood(File.ReadAllText(_stateFilePath));
            _logger.LogDebug("Read persisted mood selection from {StateFilePath}.", _stateFilePath);
            return selectedMood;
        }
        catch (IOException exception)
        {
            _startupWarningMessage = "Unable to read the persisted mood selection. Starting with All Quotes.";
            _logger.LogWarning(exception, "Unable to read the persisted mood selection from {StateFilePath}.", _stateFilePath);
            return null;
        }
        catch (UnauthorizedAccessException exception)
        {
            _startupWarningMessage = "Unable to read the persisted mood selection. Starting with All Quotes.";
            _logger.LogWarning(exception, "Access denied while reading the persisted mood selection from {StateFilePath}.", _stateFilePath);
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
            {
                File.Delete(_stateFilePath);
                _logger.LogInformation("Cleared persisted mood selection at {StateFilePath}.", _stateFilePath);
            }

            return;
        }

        var stateDirectory = Path.GetDirectoryName(_stateFilePath);
        if (!string.IsNullOrWhiteSpace(stateDirectory))
            Directory.CreateDirectory(stateDirectory);

        File.WriteAllText(_stateFilePath, mood);
        _logger.LogDebug("Persisted mood selection {SelectedMood} to {StateFilePath}.", mood, _stateFilePath);
    }
}

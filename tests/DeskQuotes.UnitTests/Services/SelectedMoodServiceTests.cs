namespace DeskQuotes.UnitTests.Services;

public class SelectedMoodServiceTests
{
    [Fact]
    public void Constructor_WhenPersistedMoodExists_LoadsIt()
    {
        var stateFilePath = CreateStateFilePath(out var stateDirectory);

        try
        {
            Directory.CreateDirectory(stateDirectory);
            File.WriteAllText(stateFilePath, " focus ");

            var sut = new SelectedMoodService(stateFilePath);

            Assert.Equal("focus", sut.GetSelectedMood());
        }
        finally
        {
            DeleteDirectory(stateDirectory);
        }
    }

    [Fact]
    public void SetSelectedMood_WhenSelectionIsCleared_DeletesPersistedStateFile()
    {
        var stateFilePath = CreateStateFilePath(out var stateDirectory);

        try
        {
            var sut = new SelectedMoodService(stateFilePath);

            sut.SetSelectedMood("purpose");
            sut.SetSelectedMood(null);

            Assert.Null(sut.GetSelectedMood());
            Assert.False(File.Exists(stateFilePath));
        }
        finally
        {
            DeleteDirectory(stateDirectory);
        }
    }

    [Fact]
    public void SetSelectedMood_WhenPersistedStateCannotBeWritten_Throws()
    {
        var stateDirectory = Path.Combine(Path.GetTempPath(), $"{nameof(SelectedMoodServiceTests)}-{Guid.NewGuid():N}");

        try
        {
            Directory.CreateDirectory(stateDirectory);
            var sut = new SelectedMoodService(stateDirectory);

            var exception = Record.Exception(() => sut.SetSelectedMood("focus"));

            Assert.NotNull(exception);
            Assert.True(exception is IOException or UnauthorizedAccessException);
            Assert.Null(sut.GetSelectedMood());
        }
        finally
        {
            DeleteDirectory(stateDirectory);
        }
    }

    private static string CreateStateFilePath(out string stateDirectory)
    {
        stateDirectory = Path.Combine(Path.GetTempPath(), $"{nameof(SelectedMoodServiceTests)}-{Guid.NewGuid():N}");
        return Path.Combine(stateDirectory, AppConstants.SelectedMoodStateFileName);
    }

    private static void DeleteDirectory(string directoryPath)
    {
        if (Directory.Exists(directoryPath))
            Directory.Delete(directoryPath, true);
    }
}
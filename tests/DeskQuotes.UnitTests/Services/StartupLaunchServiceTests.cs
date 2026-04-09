namespace DeskQuotes.UnitTests.Services;

public class StartupLaunchServiceTests
{
    [Fact]
    public void IsEnabled_WhenStartupValueDoesNotExist_ReturnsFalse()
    {
        var store = new FakeStartupLaunchStore();
        var sut = new StartupLaunchService(store, AppConstants.RunAtSignInRegistryValueName, @"C:\Apps\DeskQuotes.exe");

        var isEnabled = sut.IsEnabled();

        Assert.False(isEnabled);
    }

    [Fact]
    public void IsEnabled_WhenStartupValueExists_ReturnsTrue()
    {
        var store = new FakeStartupLaunchStore();
        store.SetValue(AppConstants.RunAtSignInRegistryValueName, "\"C:\\Apps\\DeskQuotes.exe\"");
        var sut = new StartupLaunchService(store, AppConstants.RunAtSignInRegistryValueName, @"C:\Apps\DeskQuotes.exe");

        var isEnabled = sut.IsEnabled();

        Assert.True(isEnabled);
    }

    [Fact]
    public void Enable_WhenCalled_PersistsQuotedProcessPath()
    {
        var store = new FakeStartupLaunchStore();
        var sut = new StartupLaunchService(store, AppConstants.RunAtSignInRegistryValueName, @"C:\Program Files\DeskQuotes\DeskQuotes.exe");

        sut.Enable();

        Assert.Equal(
            "\"C:\\Program Files\\DeskQuotes\\DeskQuotes.exe\"",
            store.GetValue(AppConstants.RunAtSignInRegistryValueName));
    }

    [Fact]
    public void Disable_WhenCalled_RemovesStartupValue()
    {
        var store = new FakeStartupLaunchStore();
        store.SetValue(AppConstants.RunAtSignInRegistryValueName, "\"C:\\Apps\\DeskQuotes.exe\"");
        var sut = new StartupLaunchService(store, AppConstants.RunAtSignInRegistryValueName, @"C:\Apps\DeskQuotes.exe");

        sut.Disable();

        Assert.Null(store.GetValue(AppConstants.RunAtSignInRegistryValueName));
    }

    private sealed class FakeStartupLaunchStore : IStartupLaunchStore
    {
        private readonly Dictionary<string, string> _values = new(StringComparer.Ordinal);

        public string? GetValue(string valueName)
        {
            return _values.TryGetValue(valueName, out var value) ? value : null;
        }

        public void SetValue(string valueName, string value)
        {
            _values[valueName] = value;
        }

        public void DeleteValue(string valueName)
        {
            _values.Remove(valueName);
        }
    }
}

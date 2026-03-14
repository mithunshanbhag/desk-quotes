namespace DeskQuotes.UnitTests.Services;

public class GlobalHotkeyServiceTests
{
    #region Boundary cases

    [Fact]
    public void Dispose_WhenHotkeyWasNeverRegistered_IsSafe()
    {
        var sut = new SpyGlobalHotkeyService();

        sut.Dispose();

        Assert.Equal(0, sut.RemoveMessageFilterCallCount);
        Assert.Equal(0, sut.UnregisterHotkeyCallCount);
    }

    #endregion

    private sealed class SpyGlobalHotkeyService : GlobalHotkeyService
    {
        public int AddMessageFilterCallCount { get; private set; }
        public int RegisterHotkeyCallCount { get; private set; }
        public int RemoveMessageFilterCallCount { get; private set; }
        public int UnregisterHotkeyCallCount { get; private set; }
        public bool RegisterHotkeyReturnValue { get; init; } = true;

        protected override void AddMessageFilter(IMessageFilter messageFilter)
        {
            AddMessageFilterCallCount++;
        }

        protected override void RemoveMessageFilter(IMessageFilter messageFilter)
        {
            RemoveMessageFilterCallCount++;
        }

        protected override bool TryRegisterHotkeyCore(IntPtr hWnd, int id, uint modifiers, uint virtualKey)
        {
            RegisterHotkeyCallCount++;
            return RegisterHotkeyReturnValue;
        }

        protected override bool TryUnregisterHotkeyCore(IntPtr hWnd, int id)
        {
            UnregisterHotkeyCallCount++;
            return true;
        }
    }

    #region Positive cases

    [Fact]
    public void TryRegisterHotkey_WhenRegistrationSucceeds_AddsMessageFilterAndReturnsTrue()
    {
        var sut = new SpyGlobalHotkeyService();

        var result = sut.TryRegisterHotkey(AppConstants.RefreshWallpaperHotkeyId, AppConstants.RefreshWallpaperHotkeyModifiers, AppConstants.RefreshWallpaperHotkeyVirtualKey,
            () => { });

        Assert.True(result);
        Assert.Equal(1, sut.AddMessageFilterCallCount);
        Assert.Equal(0, sut.RemoveMessageFilterCallCount);
        Assert.Equal(1, sut.RegisterHotkeyCallCount);
    }

    [Fact]
    public void TryRegisterHotkey_WhenTwoHotkeysRegistered_AddsMessageFilterOnceAndReturnsTrueBothTimes()
    {
        var sut = new SpyGlobalHotkeyService();

        var result1 = sut.TryRegisterHotkey(AppConstants.RefreshWallpaperHotkeyId, AppConstants.RefreshWallpaperHotkeyModifiers, AppConstants.RefreshWallpaperHotkeyVirtualKey,
            () => { });
        var result2 = sut.TryRegisterHotkey(AppConstants.EditSettingsHotkeyId, AppConstants.EditSettingsHotkeyModifiers, AppConstants.EditSettingsHotkeyVirtualKey, () => { });

        Assert.True(result1);
        Assert.True(result2);
        Assert.Equal(1, sut.AddMessageFilterCallCount);
        Assert.Equal(2, sut.RegisterHotkeyCallCount);
    }

    [Fact]
    public void TryRegisterHotkey_WhenAllWallpaperActionsAreRegistered_AddsMessageFilterOnce()
    {
        var sut = new SpyGlobalHotkeyService();

        var refreshRegistered = sut.TryRegisterHotkey(AppConstants.RefreshWallpaperHotkeyId, AppConstants.RefreshWallpaperHotkeyModifiers,
            AppConstants.RefreshWallpaperHotkeyVirtualKey, () => { });
        var editRegistered = sut.TryRegisterHotkey(AppConstants.EditSettingsHotkeyId, AppConstants.EditSettingsHotkeyModifiers,
            AppConstants.EditSettingsHotkeyVirtualKey, () => { });
        var darkenRegistered = sut.TryRegisterHotkey(AppConstants.DarkenWallpaperBackgroundColorHotkeyId, AppConstants.DarkenWallpaperBackgroundColorHotkeyModifiers,
            AppConstants.DarkenWallpaperBackgroundColorHotkeyVirtualKey, () => { });
        var lightenRegistered = sut.TryRegisterHotkey(AppConstants.LightenWallpaperBackgroundColorHotkeyId, AppConstants.LightenWallpaperBackgroundColorHotkeyModifiers,
            AppConstants.LightenWallpaperBackgroundColorHotkeyVirtualKey, () => { });
        var randomRegistered = sut.TryRegisterHotkey(AppConstants.RandomWallpaperBackgroundColorHotkeyId, AppConstants.RandomWallpaperBackgroundColorHotkeyModifiers,
            AppConstants.RandomWallpaperBackgroundColorHotkeyVirtualKey, () => { });
        var randomFontRegistered = sut.TryRegisterHotkey(AppConstants.RandomWallpaperFontHotkeyId, AppConstants.RandomWallpaperFontHotkeyModifiers,
            AppConstants.RandomWallpaperFontHotkeyVirtualKey, () => { });

        Assert.True(refreshRegistered);
        Assert.True(editRegistered);
        Assert.True(darkenRegistered);
        Assert.True(lightenRegistered);
        Assert.True(randomRegistered);
        Assert.True(randomFontRegistered);
        Assert.Equal(1, sut.AddMessageFilterCallCount);
        Assert.Equal(6, sut.RegisterHotkeyCallCount);
    }

    [Fact]
    public void PreFilterMessage_WhenRegisteredHotkeyMessageReceived_InvokesCallbackAndReturnsTrue()
    {
        var sut = new SpyGlobalHotkeyService();
        var callbackCallCount = 0;
        sut.TryRegisterHotkey(AppConstants.RefreshWallpaperHotkeyId, AppConstants.RefreshWallpaperHotkeyModifiers, AppConstants.RefreshWallpaperHotkeyVirtualKey,
            () => callbackCallCount++);
        var message = Message.Create(IntPtr.Zero, AppConstants.WmHotkey, AppConstants.RefreshWallpaperHotkeyId, IntPtr.Zero);

        var result = sut.PreFilterMessage(ref message);

        Assert.True(result);
        Assert.Equal(1, callbackCallCount);
    }

    [Fact]
    public void PreFilterMessage_WhenEditSettingsHotkeyMessageReceived_InvokesCallbackAndReturnsTrue()
    {
        var sut = new SpyGlobalHotkeyService();
        var callbackCallCount = 0;
        sut.TryRegisterHotkey(AppConstants.EditSettingsHotkeyId, AppConstants.EditSettingsHotkeyModifiers, AppConstants.EditSettingsHotkeyVirtualKey, () => callbackCallCount++);
        var message = Message.Create(IntPtr.Zero, AppConstants.WmHotkey, AppConstants.EditSettingsHotkeyId, IntPtr.Zero);

        var result = sut.PreFilterMessage(ref message);

        Assert.True(result);
        Assert.Equal(1, callbackCallCount);
    }

    [Fact]
    public void PreFilterMessage_WhenRandomWallpaperFontHotkeyMessageReceived_InvokesCallbackAndReturnsTrue()
    {
        var sut = new SpyGlobalHotkeyService();
        var callbackCallCount = 0;
        sut.TryRegisterHotkey(AppConstants.RandomWallpaperFontHotkeyId, AppConstants.RandomWallpaperFontHotkeyModifiers,
            AppConstants.RandomWallpaperFontHotkeyVirtualKey, () => callbackCallCount++);
        var message = Message.Create(IntPtr.Zero, AppConstants.WmHotkey, AppConstants.RandomWallpaperFontHotkeyId, IntPtr.Zero);

        var result = sut.PreFilterMessage(ref message);

        Assert.True(result);
        Assert.Equal(1, callbackCallCount);
    }

    [Fact]
    public void PreFilterMessage_WhenBothHotkeysRegistered_DispatchesToCorrectHandler()
    {
        var sut = new SpyGlobalHotkeyService();
        var refreshCallCount = 0;
        var editSettingsCallCount = 0;
        sut.TryRegisterHotkey(AppConstants.RefreshWallpaperHotkeyId, AppConstants.RefreshWallpaperHotkeyModifiers, AppConstants.RefreshWallpaperHotkeyVirtualKey,
            () => refreshCallCount++);
        sut.TryRegisterHotkey(AppConstants.EditSettingsHotkeyId, AppConstants.EditSettingsHotkeyModifiers, AppConstants.EditSettingsHotkeyVirtualKey,
            () => editSettingsCallCount++);
        var editSettingsMessage = Message.Create(IntPtr.Zero, AppConstants.WmHotkey, AppConstants.EditSettingsHotkeyId, IntPtr.Zero);

        var result = sut.PreFilterMessage(ref editSettingsMessage);

        Assert.True(result);
        Assert.Equal(0, refreshCallCount);
        Assert.Equal(1, editSettingsCallCount);
    }

    [Fact]
    public void Dispose_WhenHotkeyIsRegistered_RemovesMessageFilterAndUnregistersHotkey()
    {
        var sut = new SpyGlobalHotkeyService();
        sut.TryRegisterHotkey(AppConstants.RefreshWallpaperHotkeyId, AppConstants.RefreshWallpaperHotkeyModifiers, AppConstants.RefreshWallpaperHotkeyVirtualKey, () => { });

        sut.Dispose();

        Assert.Equal(1, sut.RemoveMessageFilterCallCount);
        Assert.Equal(1, sut.UnregisterHotkeyCallCount);
    }

    [Fact]
    public void Dispose_WhenBothHotkeysRegistered_UnregistersBothHotkeys()
    {
        var sut = new SpyGlobalHotkeyService();
        sut.TryRegisterHotkey(AppConstants.RefreshWallpaperHotkeyId, AppConstants.RefreshWallpaperHotkeyModifiers, AppConstants.RefreshWallpaperHotkeyVirtualKey, () => { });
        sut.TryRegisterHotkey(AppConstants.EditSettingsHotkeyId, AppConstants.EditSettingsHotkeyModifiers, AppConstants.EditSettingsHotkeyVirtualKey, () => { });

        sut.Dispose();

        Assert.Equal(1, sut.RemoveMessageFilterCallCount);
        Assert.Equal(2, sut.UnregisterHotkeyCallCount);
    }

    #endregion

    #region Negative cases

    [Fact]
    public void TryRegisterHotkey_WhenRegistrationFails_RemovesMessageFilterAndReturnsFalse()
    {
        var sut = new SpyGlobalHotkeyService { RegisterHotkeyReturnValue = false };

        var result = sut.TryRegisterHotkey(AppConstants.RefreshWallpaperHotkeyId, AppConstants.RefreshWallpaperHotkeyModifiers, AppConstants.RefreshWallpaperHotkeyVirtualKey,
            () => { });

        Assert.False(result);
        Assert.Equal(1, sut.AddMessageFilterCallCount);
        Assert.Equal(1, sut.RemoveMessageFilterCallCount);
        Assert.Equal(0, sut.UnregisterHotkeyCallCount);
    }

    [Fact]
    public void TryRegisterHotkey_WhenAlreadyRegistered_ThrowsInvalidOperationException()
    {
        var sut = new SpyGlobalHotkeyService();
        sut.TryRegisterHotkey(AppConstants.RefreshWallpaperHotkeyId, AppConstants.RefreshWallpaperHotkeyModifiers, AppConstants.RefreshWallpaperHotkeyVirtualKey, () => { });

        Action action = () => _ = sut.TryRegisterHotkey(AppConstants.RefreshWallpaperHotkeyId, AppConstants.RefreshWallpaperHotkeyModifiers,
            AppConstants.RefreshWallpaperHotkeyVirtualKey,
            () => { });

        Assert.Throws<InvalidOperationException>(action);
    }

    [Fact]
    public void PreFilterMessage_WhenMessageDoesNotMatchRegisteredHotkey_DoesNotInvokeCallback()
    {
        var sut = new SpyGlobalHotkeyService();
        var callbackCallCount = 0;
        sut.TryRegisterHotkey(AppConstants.RefreshWallpaperHotkeyId, AppConstants.RefreshWallpaperHotkeyModifiers, AppConstants.RefreshWallpaperHotkeyVirtualKey,
            () => callbackCallCount++);
        var message = Message.Create(IntPtr.Zero, 0x9999, AppConstants.RefreshWallpaperHotkeyId, IntPtr.Zero);

        var result = sut.PreFilterMessage(ref message);

        Assert.False(result);
        Assert.Equal(0, callbackCallCount);
    }

    #endregion
}
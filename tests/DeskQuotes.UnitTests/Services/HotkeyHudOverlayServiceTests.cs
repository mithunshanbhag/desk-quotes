namespace DeskQuotes.UnitTests.Services;

public class HotkeyHudOverlayServiceTests
{
    #region Positive cases

    [Fact]
    public void ShowWallpaperRefreshed_WhenCalled_UsesRefreshOverlayContent()
    {
        var sut = new RecordingHotkeyHudOverlayService();

        sut.ShowWallpaperRefreshed();

        AssertOverlayContent(sut.LastContent, HotkeyHudOverlayKind.Refresh, "Wallpaper refreshed");
        Assert.Equal(1, sut.ShowCallCount);
    }

    [Fact]
    public void ShowBackgroundDarkened_WhenCalled_UsesDarkerBackgroundOverlayContent()
    {
        var sut = new RecordingHotkeyHudOverlayService();

        sut.ShowBackgroundDarkened();

        AssertOverlayContent(sut.LastContent, HotkeyHudOverlayKind.BackgroundDarker, "Background darker");
        Assert.Equal(1, sut.ShowCallCount);
    }

    [Fact]
    public void ShowBackgroundLightened_WhenCalled_UsesLighterBackgroundOverlayContent()
    {
        var sut = new RecordingHotkeyHudOverlayService();

        sut.ShowBackgroundLightened();

        AssertOverlayContent(sut.LastContent, HotkeyHudOverlayKind.BackgroundLighter, "Background lighter");
        Assert.Equal(1, sut.ShowCallCount);
    }

    [Fact]
    public void ShowRandomBackground_WhenCalled_UsesRandomBackgroundOverlayContent()
    {
        var sut = new RecordingHotkeyHudOverlayService();

        sut.ShowRandomBackground();

        AssertOverlayContent(sut.LastContent, HotkeyHudOverlayKind.RandomBackground, "Random background");
        Assert.Equal(1, sut.ShowCallCount);
    }

    [Fact]
    public void ShowFontChanged_WhenFontNameProvided_IncludesFontNameInOverlayMessage()
    {
        var sut = new RecordingHotkeyHudOverlayService();

        sut.ShowFontChanged("Georgia");

        AssertOverlayContent(sut.LastContent, HotkeyHudOverlayKind.Font, "Font: Georgia");
        Assert.Equal(1, sut.ShowCallCount);
    }

    [Fact]
    public void ShowOpeningSettings_WhenCalled_UsesSettingsOverlayContent()
    {
        var sut = new RecordingHotkeyHudOverlayService();

        sut.ShowOpeningSettings();

        AssertOverlayContent(sut.LastContent, HotkeyHudOverlayKind.Settings, "Opening settings");
        Assert.Equal(1, sut.ShowCallCount);
    }

    [Fact]
    public void WarmUp_WhenCalledMultipleTimes_CreatesOverlayFormOnlyOnce()
    {
        var sut = new SpyHotkeyHudOverlayService();

        sut.WarmUp();
        sut.WarmUp();

        Assert.Equal(1, sut.CreateOverlayFormCallCount);
    }

    #endregion

    #region Boundary cases

    [Fact]
    public void ShowFontChanged_WhenFontNameMissing_UsesFallbackOverlayMessage()
    {
        var sut = new RecordingHotkeyHudOverlayService();

        sut.ShowFontChanged("   ");

        AssertOverlayContent(sut.LastContent, HotkeyHudOverlayKind.Font, "Font changed");
        Assert.Equal(1, sut.ShowCallCount);
    }

    #endregion

    private static void AssertOverlayContent(HotkeyHudOverlayContent? content, HotkeyHudOverlayKind expectedKind, string expectedMessage)
    {
        Assert.NotNull(content);
        Assert.Equal(expectedKind, content.Kind);
        Assert.Equal(expectedMessage, content.Message);
    }

    private sealed class RecordingHotkeyHudOverlayService : HotkeyHudOverlayService
    {
        public HotkeyHudOverlayContent? LastContent { get; private set; }
        public int ShowCallCount { get; private set; }

        public override void Show(HotkeyHudOverlayContent content)
        {
            ShowCallCount++;
            LastContent = content;
        }
    }

    private sealed class SpyHotkeyHudOverlayService : HotkeyHudOverlayService
    {
        public int CreateOverlayFormCallCount { get; private set; }

        protected override HotkeyHudOverlayForm CreateOverlayForm()
        {
            CreateOverlayFormCallCount++;
            return new HotkeyHudOverlayForm();
        }
    }
}

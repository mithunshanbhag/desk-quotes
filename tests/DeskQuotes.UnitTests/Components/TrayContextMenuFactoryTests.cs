namespace DeskQuotes.UnitTests.Components;

public class TrayContextMenuFactoryTests
{
    private static readonly EventHandler NoOpHandler = static (_, _) => { };

    #region Positive cases

    [Fact]
    public void Build_WhenCreated_AppliesSubtleNativeStylingAndSeparators()
    {
        var trackedMoodMenuItems = new List<ToolStripMenuItem>();
        var sut = CreateSut(trackedMoodMenuItems, "focus", false, "focus", "action");

        using var menu = sut.Build();

        Assert.Equal("Segoe UI", menu.Font.FontFamily.Name);
        Assert.Equal(10F, menu.Font.Size);
        Assert.Equal(ToolStripRenderMode.System, menu.RenderMode);
        Assert.False(menu.ShowCheckMargin);
        Assert.False(menu.ShowImageMargin);
        Assert.Equal(9, menu.Items.Count);
        Assert.Equal(AppConstants.RefreshWallpaperMenuLabel, menu.Items[0].Text);
        Assert.Equal(AppConstants.SetMoodMenuLabel, menu.Items[1].Text);
        Assert.Equal(AppConstants.WallpaperBackgroundColorMenuLabel, menu.Items[2].Text);
        Assert.Equal(AppConstants.ChangeWallpaperFontMenuLabel, menu.Items[3].Text);
        Assert.IsType<ToolStripSeparator>(menu.Items[4]);
        Assert.Equal(AppConstants.RunAtSignInMenuLabel, menu.Items[5].Text);
        Assert.Equal(AppConstants.EditSettingsMenuLabel, menu.Items[6].Text);
        Assert.IsType<ToolStripSeparator>(menu.Items[7]);
        Assert.Equal(AppConstants.ExitMenuLabel, menu.Items[8].Text);

        var refreshMenuItem = Assert.IsType<ToolStripMenuItem>(menu.Items[0]);
        Assert.Equal(new Padding(12, 6, 12, 6), refreshMenuItem.Padding);

        var separator = Assert.IsType<ToolStripSeparator>(menu.Items[4]);
        Assert.Equal(new Padding(12, 4, 12, 4), separator.Margin);
    }

    [Fact]
    public void Build_WhenSelectedMoodAndTagsProvided_BuildsCheckedSetMoodSubmenu()
    {
        var trackedMoodMenuItems = new List<ToolStripMenuItem>();
        var sut = CreateSut(trackedMoodMenuItems, "focus", false, "focus", "action");

        using var menu = sut.Build();

        var setMoodMenuItem = Assert.IsType<ToolStripMenuItem>(menu.Items[1]);
        Assert.Collection(
            setMoodMenuItem.DropDownItems.Cast<ToolStripItem>(),
            item => Assert.Equal(AppConstants.AllQuotesMoodMenuLabel, Assert.IsType<ToolStripMenuItem>(item).Text),
            item => Assert.IsType<ToolStripSeparator>(item),
            item => Assert.Equal("focus", Assert.IsType<ToolStripMenuItem>(item).Text),
            item => Assert.Equal("action", Assert.IsType<ToolStripMenuItem>(item).Text));

        var allQuotesMenuItem = Assert.IsType<ToolStripMenuItem>(setMoodMenuItem.DropDownItems[0]);
        var separator = Assert.IsType<ToolStripSeparator>(setMoodMenuItem.DropDownItems[1]);
        var focusMenuItem = Assert.IsType<ToolStripMenuItem>(setMoodMenuItem.DropDownItems[2]);
        var actionMenuItem = Assert.IsType<ToolStripMenuItem>(setMoodMenuItem.DropDownItems[3]);

        Assert.Equal(AppConstants.AllQuotesMoodMenuLabel, allQuotesMenuItem.Text);
        Assert.False(allQuotesMenuItem.Checked);
        Assert.Equal(new Padding(12, 4, 12, 4), separator.Margin);
        Assert.True(focusMenuItem.Checked);
        Assert.False(actionMenuItem.Checked);
        Assert.Collection(
            trackedMoodMenuItems,
            item => Assert.Equal(AppConstants.AllQuotesMoodMenuLabel, item.Text),
            item => Assert.Equal("focus", item.Text),
            item => Assert.Equal("action", item.Text));

        var setMoodDropDown = Assert.IsType<ToolStripDropDownMenu>(setMoodMenuItem.DropDown);
        Assert.Equal(ToolStripRenderMode.System, setMoodDropDown.RenderMode);
        Assert.True(setMoodDropDown.ShowCheckMargin);
        Assert.False(setMoodDropDown.ShowImageMargin);
    }

    [Fact]
    public void Build_WhenRunAtSignInIsEnabled_ShowsCheckedTopLevelMenuItem()
    {
        var trackedMoodMenuItems = new List<ToolStripMenuItem>();
        var sut = CreateSut(trackedMoodMenuItems, null, true);

        using var menu = sut.Build();

        var runAtSignInMenuItem = Assert.IsType<ToolStripMenuItem>(menu.Items[5]);

        Assert.Equal(AppConstants.RunAtSignInMenuLabel, runAtSignInMenuItem.Text);
        Assert.True(runAtSignInMenuItem.Checked);
        Assert.True(menu.ShowCheckMargin);
    }

    #endregion

    #region Boundary cases

    [Fact]
    public void Build_WhenNoConfiguredMoodsExist_OmitsMoodSeparator()
    {
        var trackedMoodMenuItems = new List<ToolStripMenuItem>();
        var sut = CreateSut(trackedMoodMenuItems);

        using var menu = sut.Build();

        var setMoodMenuItem = Assert.IsType<ToolStripMenuItem>(menu.Items[1]);
        var allQuotesMenuItem = Assert.Single(setMoodMenuItem.DropDownItems.Cast<ToolStripItem>());
        var menuItem = Assert.IsType<ToolStripMenuItem>(allQuotesMenuItem);

        Assert.True(menuItem.Checked);
        Assert.Single(trackedMoodMenuItems);
    }

    #endregion

    private static TrayContextMenuFactory CreateSut(
        ICollection<ToolStripMenuItem> trackedMoodMenuItems,
        string? selectedMood = null,
        bool isRunAtSignInEnabled = false,
        params string[] tagCatalog)
    {
        return new TrayContextMenuFactory(
            tagCatalog,
            selectedMood,
            isRunAtSignInEnabled,
            trackedMoodMenuItems,
            new TrayContextMenuActions
            {
                RefreshWallpaper = NoOpHandler,
                SetMood = NoOpHandler,
                DarkenWallpaperBackgroundColor = NoOpHandler,
                LightenWallpaperBackgroundColor = NoOpHandler,
                RandomizeWallpaperBackgroundColor = NoOpHandler,
                RandomizeWallpaperFont = NoOpHandler,
                ToggleRunAtSignIn = NoOpHandler,
                EditSettings = NoOpHandler,
                Exit = NoOpHandler
            });
    }
}

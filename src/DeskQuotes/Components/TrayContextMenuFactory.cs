namespace DeskQuotes.Components;

internal sealed class TrayContextMenuFactory(
    IReadOnlyList<string> tagCatalog,
    string? selectedMood,
    ICollection<ToolStripMenuItem> setMoodMenuItems,
    TrayContextMenuActions actions)
{
    private const string MenuFontFamilyName = "Segoe UI";
    private const float MenuFontSizeInPoints = 10F;
    private static readonly Padding MenuItemPadding = new(12, 6, 12, 6);
    private static readonly Padding SeparatorMargin = new(12, 4, 12, 4);

    public ContextMenuStrip Build()
    {
        var contextMenuStrip = new ContextMenuStrip
        {
            Font = new Font(MenuFontFamilyName, MenuFontSizeInPoints, FontStyle.Regular, GraphicsUnit.Point),
            RenderMode = ToolStripRenderMode.System,
            ShowCheckMargin = false,
            ShowImageMargin = false
        };

        contextMenuStrip.Items.AddRange(
        [
            CreateMenuItem(AppConstants.RefreshWallpaperMenuLabel, actions.RefreshWallpaper, AppConstants.RefreshWallpaperHotkeyDisplay),
            CreateSetMoodMenuItem(),
            CreateWallpaperBackgroundColorMenuItem(),
            CreateChangeWallpaperFontMenuItem(),
            new ToolStripSeparator(),
            CreateMenuItem(AppConstants.EditSettingsMenuLabel, actions.EditSettings, AppConstants.EditSettingsHotkeyDisplay),
            new ToolStripSeparator(),
            CreateMenuItem(AppConstants.ExitMenuLabel, actions.Exit)
        ]);

        ApplyStyling(contextMenuStrip, contextMenuStrip.Items);
        return contextMenuStrip;
    }

    private ToolStripMenuItem CreateSetMoodMenuItem()
    {
        var setMoodMenuItem = new ToolStripMenuItem(AppConstants.SetMoodMenuLabel);
        setMoodMenuItem.DropDownItems.Add(CreateSetMoodMenuItem(AppConstants.AllQuotesMoodMenuLabel, null));

        if (tagCatalog.Count > 0)
            setMoodMenuItem.DropDownItems.Add(new ToolStripSeparator());

        foreach (var mood in tagCatalog)
            setMoodMenuItem.DropDownItems.Add(CreateSetMoodMenuItem(mood, mood));

        return setMoodMenuItem;
    }

    private ToolStripMenuItem CreateSetMoodMenuItem(string text, string? mood)
    {
        var setMoodMenuItem = new ToolStripMenuItem(text, null, actions.SetMood)
        {
            Checked = IsSameMood(mood, selectedMood),
            Tag = mood
        };
        setMoodMenuItems.Add(setMoodMenuItem);
        return setMoodMenuItem;
    }

    private ToolStripMenuItem CreateWallpaperBackgroundColorMenuItem()
    {
        var wallpaperBackgroundColorMenuItem = new ToolStripMenuItem(AppConstants.WallpaperBackgroundColorMenuLabel);
        wallpaperBackgroundColorMenuItem.DropDownItems.Add(CreateMenuItem(AppConstants.DarkenWallpaperBackgroundColorMenuLabel, actions.DarkenWallpaperBackgroundColor,
            AppConstants.DarkenWallpaperBackgroundColorHotkeyDisplay));
        wallpaperBackgroundColorMenuItem.DropDownItems.Add(CreateMenuItem(AppConstants.LightenWallpaperBackgroundColorMenuLabel, actions.LightenWallpaperBackgroundColor,
            AppConstants.LightenWallpaperBackgroundColorHotkeyDisplay));
        wallpaperBackgroundColorMenuItem.DropDownItems.Add(CreateMenuItem(AppConstants.RandomWallpaperBackgroundColorMenuLabel, actions.RandomizeWallpaperBackgroundColor,
            AppConstants.RandomWallpaperBackgroundColorHotkeyDisplay));
        return wallpaperBackgroundColorMenuItem;
    }

    private ToolStripMenuItem CreateChangeWallpaperFontMenuItem()
    {
        var changeWallpaperFontMenuItem = new ToolStripMenuItem(AppConstants.ChangeWallpaperFontMenuLabel);
        changeWallpaperFontMenuItem.DropDownItems.Add(CreateMenuItem(AppConstants.RandomWallpaperFontMenuLabel, actions.RandomizeWallpaperFont,
            AppConstants.RandomWallpaperFontHotkeyDisplay));
        return changeWallpaperFontMenuItem;
    }

    private static ToolStripMenuItem CreateMenuItem(string text, EventHandler onClick, string? shortcutKeyDisplayString = null)
    {
        return new ToolStripMenuItem(text, null, onClick)
        {
            ShortcutKeyDisplayString = shortcutKeyDisplayString
        };
    }

    private static void ApplyStyling(ToolStripDropDownMenu menu, ToolStripItemCollection items)
    {
        menu.RenderMode = ToolStripRenderMode.System;
        menu.ShowCheckMargin = items.OfType<ToolStripMenuItem>().Any(item => item.Checked);
        menu.ShowImageMargin = false;

        foreach (var item in items.Cast<ToolStripItem>())
        {
            item.Font = menu.Font;

            switch (item)
            {
                case ToolStripSeparator separator:
                    separator.Margin = SeparatorMargin;
                    break;
                case ToolStripMenuItem menuItem:
                    menuItem.Padding = MenuItemPadding;

                    if (menuItem.DropDown is ToolStripDropDownMenu dropDownMenu)
                    {
                        dropDownMenu.Font = menu.Font;
                        ApplyStyling(dropDownMenu, menuItem.DropDownItems);
                    }

                    break;
            }
        }
    }

    private static bool IsSameMood(string? left, string? right)
    {
        return string.IsNullOrWhiteSpace(left)
            ? string.IsNullOrWhiteSpace(right)
            : string.Equals(left, right, StringComparison.OrdinalIgnoreCase);
    }
}
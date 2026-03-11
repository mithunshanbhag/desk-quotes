namespace DeskQuotes.Services.Implementations;

public partial class GlobalHotkeyService : IMessageFilter, IDisposable
{
    private Action? _hotkeyPressedHandler;
    private bool _isDisposed;
    private bool _isRegistered;
    private bool _messageFilterAdded;

    public void Dispose()
    {
        if (_isDisposed)
            return;

        if (_isRegistered)
        {
            TryUnregisterHotkeyCore(IntPtr.Zero, AppConstants.RefreshWallpaperHotkeyId);
            _isRegistered = false;
        }

        if (_messageFilterAdded)
        {
            RemoveMessageFilter(this);
            _messageFilterAdded = false;
        }

        _hotkeyPressedHandler = null;
        _isDisposed = true;
        GC.SuppressFinalize(this);
    }

    public bool PreFilterMessage(ref Message message)
    {
        if (!_isRegistered)
            return false;

        if (message.Msg != AppConstants.WmHotkey)
            return false;

        if (message.WParam != AppConstants.RefreshWallpaperHotkeyId)
            return false;

        _hotkeyPressedHandler?.Invoke();
        return true;
    }

    public virtual bool TryRegisterHotkey(Action hotkeyPressedHandler)
    {
        ObjectDisposedException.ThrowIf(_isDisposed, this);
        ArgumentNullException.ThrowIfNull(hotkeyPressedHandler);

        if (_isRegistered)
            throw new InvalidOperationException("The global hotkey has already been registered.");

        _hotkeyPressedHandler = hotkeyPressedHandler;
        AddMessageFilter(this);
        _messageFilterAdded = true;

        if (TryRegisterHotkeyCore(IntPtr.Zero, AppConstants.RefreshWallpaperHotkeyId, AppConstants.RefreshWallpaperHotkeyModifiers, AppConstants.RefreshWallpaperHotkeyVirtualKey))
        {
            _isRegistered = true;
            return true;
        }

        RemoveMessageFilter(this);
        _messageFilterAdded = false;
        _hotkeyPressedHandler = null;

        return false;
    }

    protected virtual void AddMessageFilter(IMessageFilter messageFilter)
    {
        Application.AddMessageFilter(messageFilter);
    }

    protected virtual void RemoveMessageFilter(IMessageFilter messageFilter)
    {
        Application.RemoveMessageFilter(messageFilter);
    }

    protected virtual bool TryRegisterHotkeyCore(IntPtr hWnd, int id, uint modifiers, uint virtualKey)
    {
        return RegisterHotKey(hWnd, id, modifiers, virtualKey);
    }

    protected virtual bool TryUnregisterHotkeyCore(IntPtr hWnd, int id)
    {
        return UnregisterHotKey(hWnd, id);
    }

    [LibraryImport("user32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static partial bool RegisterHotKey(IntPtr hWnd, int id, uint fsModifiers, uint vk);

    [LibraryImport("user32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static partial bool UnregisterHotKey(IntPtr hWnd, int id);
}
namespace DeskQuotes.Services.Implementations;

public partial class GlobalHotkeyService : IMessageFilter, IDisposable
{
    private readonly Dictionary<int, Action> _handlers = new();
    private readonly HashSet<int> _registeredIds = [];
    private bool _isDisposed;
    private bool _messageFilterAdded;

    public void Dispose()
    {
        if (_isDisposed)
            return;

        foreach (var id in _registeredIds)
            TryUnregisterHotkeyCore(IntPtr.Zero, id);

        _registeredIds.Clear();

        if (_messageFilterAdded)
        {
            RemoveMessageFilter(this);
            _messageFilterAdded = false;
        }

        _handlers.Clear();
        _isDisposed = true;
        GC.SuppressFinalize(this);
    }

    public bool PreFilterMessage(ref Message message)
    {
        if (message.Msg != AppConstants.WmHotkey)
            return false;

        var hotkeyId = (int)message.WParam;
        if (!_handlers.TryGetValue(hotkeyId, out var handler))
            return false;

        handler.Invoke();
        return true;
    }

    public virtual bool TryRegisterHotkey(int id, uint modifiers, uint virtualKey, Action hotkeyPressedHandler)
    {
        ObjectDisposedException.ThrowIf(_isDisposed, this);
        ArgumentNullException.ThrowIfNull(hotkeyPressedHandler);

        if (_registeredIds.Contains(id))
            throw new InvalidOperationException($"A global hotkey with id {id} has already been registered.");

        _handlers[id] = hotkeyPressedHandler;

        if (!_messageFilterAdded)
        {
            AddMessageFilter(this);
            _messageFilterAdded = true;
        }

        if (TryRegisterHotkeyCore(IntPtr.Zero, id, modifiers, virtualKey))
        {
            _registeredIds.Add(id);
            return true;
        }

        _handlers.Remove(id);

        if (_registeredIds.Count == 0)
        {
            RemoveMessageFilter(this);
            _messageFilterAdded = false;
        }

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
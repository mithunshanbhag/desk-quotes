namespace DeskQuotes;

public sealed class SingleInstanceGate : IDisposable
{
    private readonly Mutex _mutex;
    private bool _disposed;

    private SingleInstanceGate(Mutex mutex)
    {
        _mutex = mutex;
    }

    public static SingleInstanceGate? TryAcquire(string mutexName)
    {
        var mutex = new Mutex(true, mutexName, out var createdNew);

        if (!createdNew)
        {
            mutex.Dispose();
            return null;
        }

        return new SingleInstanceGate(mutex);
    }

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _mutex.ReleaseMutex();
        _mutex.Dispose();
        _disposed = true;
    }
}

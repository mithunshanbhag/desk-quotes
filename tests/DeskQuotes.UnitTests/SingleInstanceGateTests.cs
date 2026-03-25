using DeskQuotes;

namespace DeskQuotes.UnitTests;

public class SingleInstanceGateTests
{
    [Fact]
    public void TryAcquire_WhenMutexIsAlreadyHeld_ReturnsNull()
    {
        var mutexName = $"{AppConstants.SingleInstanceMutexName}.tests.{Guid.NewGuid():N}";

        using var firstGate = SingleInstanceGate.TryAcquire(mutexName);

        Assert.NotNull(firstGate);

        var secondGate = SingleInstanceGate.TryAcquire(mutexName);

        Assert.Null(secondGate);
    }

    [Fact]
    public void TryAcquire_AfterGateIsDisposed_ReturnsNewGate()
    {
        var mutexName = $"{AppConstants.SingleInstanceMutexName}.tests.{Guid.NewGuid():N}";

        var firstGate = SingleInstanceGate.TryAcquire(mutexName);

        Assert.NotNull(firstGate);

        firstGate.Dispose();

        using var secondGate = SingleInstanceGate.TryAcquire(mutexName);

        Assert.NotNull(secondGate);
    }
}

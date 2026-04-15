namespace DeskQuotes.Models;

public sealed record HotkeyHudOverlayContent
{
    public required HotkeyHudOverlayKind Kind { get; init; }
    public required string Message { get; init; }
}

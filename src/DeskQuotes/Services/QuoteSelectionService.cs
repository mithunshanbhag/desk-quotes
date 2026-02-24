namespace DeskQuotes.Services;

public class QuoteSelectionService
{
    public static bool TrySelectRandomQuote(IEnumerable<Quote>? configuredQuotes, out Quote? selectedQuote)
    {
        selectedQuote = null;

        if (configuredQuotes is null) return false;

        var candidates = configuredQuotes
            .Where(quote => !string.IsNullOrWhiteSpace(quote.Text))
            .ToArray();

        if (candidates.Length == 0) return false;

        selectedQuote = candidates[Random.Shared.Next(candidates.Length)];
        return true;
    }
}
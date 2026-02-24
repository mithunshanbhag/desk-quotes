namespace DeskQuotes.Services.Implementations;

public class QuoteSelectionService(IValidator<IEnumerable<Quote>?>? configuredQuotesValidator = null)
{
    private readonly IValidator<IEnumerable<Quote>?> _configuredQuotesValidator = configuredQuotesValidator ?? new QuoteSelectionInputValidator();

    public virtual bool TrySelectQuote(IEnumerable<Quote>? configuredQuotes, out Quote? selectedQuote)
    {
        selectedQuote = null;

        var configuredQuotesArray = configuredQuotes?.ToArray();
        if (configuredQuotesArray is null) return false;

        var validationResult = _configuredQuotesValidator.Validate(configuredQuotesArray);
        if (!validationResult.IsValid) return false;

        var candidates = configuredQuotesArray
            .Where(quote => !string.IsNullOrWhiteSpace(quote.Text))
            .ToArray();

        if (candidates.Length == 0) return false;

        selectedQuote = candidates[Random.Shared.Next(candidates.Length)];
        return true;
    }

    public static bool TrySelectRandomQuote(IEnumerable<Quote>? configuredQuotes, out Quote? selectedQuote)
    {
        return new QuoteSelectionService().TrySelectQuote(configuredQuotes, out selectedQuote);
    }
}
namespace DeskQuotes.Services.Implementations;

public class QuoteSelectionService(
    IValidator<IEnumerable<Quote>?>? configuredQuotesValidator = null,
    ILogger<QuoteSelectionService>? logger = null)
{
    private readonly IValidator<IEnumerable<Quote>?> _configuredQuotesValidator = configuredQuotesValidator ?? new QuoteSelectionInputValidator();
    private readonly ILogger<QuoteSelectionService> _logger = logger ?? NullLogger<QuoteSelectionService>.Instance;

    public virtual bool TrySelectQuote(IEnumerable<Quote>? configuredQuotes, out Quote? selectedQuote)
    {
        selectedQuote = null;

        var configuredQuotesArray = configuredQuotes?.ToArray();
        if (configuredQuotesArray is null)
        {
            _logger.LogWarning("Cannot select a quote because the configured quote collection is null.");
            return false;
        }

        var validationResult = _configuredQuotesValidator.Validate(configuredQuotesArray);
        if (!validationResult.IsValid)
        {
            _logger.LogWarning("Configured quotes failed validation and cannot be used for selection.");
            return false;
        }

        var candidates = configuredQuotesArray
            .Where(quote => !string.IsNullOrWhiteSpace(quote.Text))
            .ToArray();

        if (candidates.Length == 0)
        {
            _logger.LogWarning("No eligible quotes were available after filtering blank quote text.");
            return false;
        }

        selectedQuote = candidates[Random.Shared.Next(candidates.Length)];
        _logger.LogDebug("Selected a quote from {CandidateCount} eligible candidate(s).", candidates.Length);
        return true;
    }

    public static bool TrySelectRandomQuote(IEnumerable<Quote>? configuredQuotes, out Quote? selectedQuote)
    {
        return new QuoteSelectionService().TrySelectQuote(configuredQuotes, out selectedQuote);
    }
}

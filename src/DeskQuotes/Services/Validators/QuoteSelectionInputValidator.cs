namespace DeskQuotes.Services.Validators;

public class QuoteSelectionInputValidator : AbstractValidator<IEnumerable<Quote>?>
{
    public QuoteSelectionInputValidator()
    {
        RuleFor(quotes => quotes)
            .NotNull()
            .Must(ContainAtLeastOneValidQuote);
    }

    private static bool ContainAtLeastOneValidQuote(IEnumerable<Quote>? quotes)
    {
        if (quotes is null) return false;

        return quotes.Any(quote => !string.IsNullOrWhiteSpace(quote.Text));
    }
}
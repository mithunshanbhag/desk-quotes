namespace DeskQuotes.UnitTests.Services;

public class QuoteSelectionServiceValidationTests
{
    #region Negative cases

    [Fact]
    public void TrySelectRandomQuote_WhenQuotesAreNull_ReturnsFalse()
    {
        var result = QuoteSelectionService.TrySelectRandomQuote(null, out var selectedQuote);

        Assert.False(result);
        Assert.Null(selectedQuote);
    }

    [Fact(Skip = "@TODO: Investigate later")]
    public void TrySelectRandomQuote_WhenInputHasNoValidQuoteText_ReturnsFalse()
    {
        var quotes = new List<Quote>
        {
            null!,
            new() { Text = " ", Author = "Author 1" },
            new() { Text = "", Author = "Author 2" }
        };

        Action act = () => _ = QuoteSelectionService.TrySelectRandomQuote(quotes, out _);

        var exception = Record.Exception(act);

        Assert.Null(exception);
        Assert.False(QuoteSelectionService.TrySelectRandomQuote(quotes, out var selectedQuote));
        Assert.Null(selectedQuote);
    }

    #endregion
}
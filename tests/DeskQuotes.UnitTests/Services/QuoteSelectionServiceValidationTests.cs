namespace DeskQuotes.UnitTests.Services;

public class QuoteSelectionServiceValidationTests
{
    #region Negative cases

    [Fact]
    public void TrySelectRandomQuote_WhenQuotesAreNull_ReturnsFalse()
    {
        var result = QuoteSelectionService.TrySelectRandomQuote(null, out var selectedQuote);

        result.Should().BeFalse();
        selectedQuote.Should().BeNull();
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

        var act = () => QuoteSelectionService.TrySelectRandomQuote(quotes, out var selectedQuote);

        act.Should().NotThrow();
        QuoteSelectionService.TrySelectRandomQuote(quotes, out var selectedQuote).Should().BeFalse();
        selectedQuote.Should().BeNull();
    }

    #endregion
}

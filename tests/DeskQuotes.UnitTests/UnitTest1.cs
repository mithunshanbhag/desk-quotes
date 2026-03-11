namespace DeskQuotes.UnitTests;

public class QuoteSelectionServiceTests
{
    private readonly QuoteSelectionService _sut = new();

    #region Negative cases

    [Fact]
    public void TrySelectRandomQuote_WhenQuotesAreNull_ReturnsFalse()
    {
        var result = QuoteSelectionService.TrySelectRandomQuote(null, out var selectedQuote);

        result.Should().BeFalse();
        selectedQuote.Should().BeNull();
    }

    [Fact]
    public void TrySelectRandomQuote_WhenAllQuotesAreEmpty_ReturnsFalse()
    {
        var quotes = new[]
        {
            new Quote { Text = "", Author = "Author 1" },
            new Quote { Text = " ", Author = "Author 2" }
        };

        var result = QuoteSelectionService.TrySelectRandomQuote(quotes, out var selectedQuote);

        result.Should().BeFalse();
        selectedQuote.Should().BeNull();
    }

    #endregion

    #region Positive cases

    [Fact]
    public void TrySelectRandomQuote_WhenOnlyOneQuoteIsValid_ReturnsThatQuote()
    {
        var expectedQuote = new Quote { Text = "Stay hungry.", Author = "Steve Jobs" };
        var quotes = new[]
        {
            new Quote { Text = " ", Author = "Ignored" },
            expectedQuote,
            new Quote { Text = "", Author = "Ignored" }
        };

        var result = QuoteSelectionService.TrySelectRandomQuote(quotes, out var selectedQuote);

        result.Should().BeTrue();
        selectedQuote.Should().BeSameAs(expectedQuote);
    }

    #endregion
}

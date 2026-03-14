namespace DeskQuotes.UnitTests;

public class QuoteConfigurationBindingTests
{
    #region Positive cases

    [Fact]
    public void Bind_WhenQuoteIncludesTags_BindsTagsSuccessfully()
    {
        using var jsonStream = new MemoryStream("""
                                                {
                                                  "quotes": [
                                                    {
                                                      "text": "Choose action over hesitation.",
                                                      "author": "Unknown",
                                                      "tags": ["action", "courage"]
                                                    }
                                                  ]
                                                }
                                                """u8.ToArray());
        var configuration = new ConfigurationBuilder()
            .AddJsonStream(jsonStream)
            .Build();
        var quotes = new List<Quote>();

        configuration.GetSection("quotes").Bind(quotes);

        var quote = Assert.Single(quotes);
        Assert.Equal("Choose action over hesitation.", quote.Text);
        Assert.Equal("Unknown", quote.Author);
        Assert.NotNull(quote.Tags);
        Assert.Equal(["action", "courage"], quote.Tags);
    }

    [Fact]
    public void Bind_WhenQuoteOmitsTags_BindsLegacyQuoteAndSelectionStillWorks()
    {
        using var jsonStream = new MemoryStream("""
                                                {
                                                  "quotes": [
                                                    {
                                                      "text": "Stay hungry.",
                                                      "author": "Steve Jobs"
                                                    }
                                                  ]
                                                }
                                                """u8.ToArray());
        var configuration = new ConfigurationBuilder()
            .AddJsonStream(jsonStream)
            .Build();
        var quotes = new List<Quote>();

        configuration.GetSection("quotes").Bind(quotes);

        var quote = Assert.Single(quotes);
        Assert.Null(quote.Tags);

        var result = QuoteSelectionService.TrySelectRandomQuote(quotes, out var selectedQuote);

        Assert.True(result);
        Assert.Same(quote, selectedQuote);
    }

    #endregion
}
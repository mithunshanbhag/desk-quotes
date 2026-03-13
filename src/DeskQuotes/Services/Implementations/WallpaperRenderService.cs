namespace DeskQuotes.Services.Implementations;

public class WallpaperRenderService(IValidator<Size>? resolutionValidator = null)
{
    private const int FallbackWidth = 1920;
    private const int FallbackHeight = 1080;
    private const float TextHorizontalInsetRatio = 0.1f;
    private const float TextBlockTopInsetRatio = 0.12f;
    private const float TextBlockBottomInsetRatio = 0.12f;
    private const float AuthorSpacingRatio = 0.024f;
    private const float MinimumAuthorSpacing = 12f;
    private const float MaximumAuthorSpacing = 28f;

    public virtual string RenderQuoteWallpaper(Quote quote, Size resolution, Color backgroundColor)
    {
        ArgumentNullException.ThrowIfNull(quote);

        var isWidthValid = resolutionValidator?.Validate(
            resolution,
            options => options.IncludeProperties(size => size.Width)).IsValid ?? resolution.Width > 0;
        var isHeightValid = resolutionValidator?.Validate(
            resolution,
            options => options.IncludeProperties(size => size.Height)).IsValid ?? resolution.Height > 0;

        var width = isWidthValid ? resolution.Width : FallbackWidth;
        var height = isHeightValid ? resolution.Height : FallbackHeight;

        var outputDirectory = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            AppConstants.AppName);
        Directory.CreateDirectory(outputDirectory);

        var outputPath = Path.Combine(outputDirectory, "deskquotes-wallpaper.bmp");
        if (File.Exists(outputPath)) File.Delete(outputPath);

        using var bitmap = new Bitmap(width, height);
        using var graphics = Graphics.FromImage(bitmap);
        graphics.SmoothingMode = SmoothingMode.AntiAlias;
        graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
        graphics.TextRenderingHint = TextRenderingHint.AntiAliasGridFit;
        graphics.Clear(backgroundColor);

        var quoteText = string.IsNullOrWhiteSpace(quote.Text) ? AppConstants.AppName : $"“{quote.Text.Trim()}”";
        var authorText = string.IsNullOrWhiteSpace(quote.Author) ? string.Empty : $"— {quote.Author.Trim()}";

        var quoteFontSize = Math.Clamp(width / 36f, 26f, 72f);
        var authorFontSize = Math.Clamp(width / 64f, 18f, 40f);
        using var quoteFont = new Font(FontFamily.GenericSansSerif, quoteFontSize, FontStyle.Bold, GraphicsUnit.Pixel);
        using var authorFont = new Font(FontFamily.GenericSansSerif, authorFontSize, FontStyle.Regular, GraphicsUnit.Pixel);

        using var quoteBrush = new SolidBrush(Color.FromArgb(240, 240, 240));
        using var authorBrush = new SolidBrush(Color.FromArgb(210, 210, 210));
        using var format = new StringFormat();
        format.Alignment = StringAlignment.Center;
        format.LineAlignment = StringAlignment.Center;
        format.Trimming = StringTrimming.Word;

        var quoteBounds = CreateQuoteBounds(graphics, quoteText, quoteFont, width, height, format, authorText, authorFont, out var authorBounds);
        graphics.DrawString(quoteText, quoteFont, quoteBrush, quoteBounds, format);

        if (authorBounds.HasValue) graphics.DrawString(authorText, authorFont, authorBrush, authorBounds.Value, format);

        bitmap.Save(outputPath, ImageFormat.Bmp);
        return outputPath;
    }

    private static RectangleF CreateQuoteBounds(
        Graphics graphics,
        string quoteText,
        Font quoteFont,
        int width,
        int height,
        StringFormat format,
        string authorText,
        Font authorFont,
        out RectangleF? authorBounds)
    {
        var textLeft = width * TextHorizontalInsetRatio;
        var textWidth = width * (1f - (2f * TextHorizontalInsetRatio));
        var textSize = new SizeF(textWidth, height);
        var measuredQuoteSize = graphics.MeasureString(quoteText, quoteFont, textSize, format);

        if (string.IsNullOrEmpty(authorText))
        {
            authorBounds = null;
            return CreateCenteredTextBounds(textLeft, textWidth, height, measuredQuoteSize.Height, measuredQuoteSize.Height);
        }

        var measuredAuthorSize = graphics.MeasureString(authorText, authorFont, textSize, format);
        var authorSpacing = Math.Clamp(height * AuthorSpacingRatio, MinimumAuthorSpacing, MaximumAuthorSpacing);
        var combinedTextHeight = measuredQuoteSize.Height + authorSpacing + measuredAuthorSize.Height;
        var quoteBounds = CreateCenteredTextBounds(textLeft, textWidth, height, combinedTextHeight, measuredQuoteSize.Height);
        authorBounds = new RectangleF(textLeft, quoteBounds.Bottom + authorSpacing, textWidth, measuredAuthorSize.Height);

        return quoteBounds;
    }

    private static RectangleF CreateCenteredTextBounds(float left, float width, int wallpaperHeight, float combinedTextHeight, float currentTextHeight)
    {
        var minimumTop = wallpaperHeight * TextBlockTopInsetRatio;
        var maximumTop = wallpaperHeight - (wallpaperHeight * TextBlockBottomInsetRatio) - combinedTextHeight;
        var top = Math.Clamp((wallpaperHeight - combinedTextHeight) / 2f, minimumTop, maximumTop);

        return new RectangleF(left, top, width, currentTextHeight);
    }
}
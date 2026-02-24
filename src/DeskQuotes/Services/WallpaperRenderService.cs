namespace DeskQuotes.Services;

public class WallpaperRenderService
{
    private const int FallbackWidth = 1920;
    private const int FallbackHeight = 1080;

    public virtual string RenderQuoteWallpaper(Quote quote, Size resolution)
    {
        ArgumentNullException.ThrowIfNull(quote);

        var width = resolution.Width > 0 ? resolution.Width : FallbackWidth;
        var height = resolution.Height > 0 ? resolution.Height : FallbackHeight;

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
        graphics.Clear(Color.FromArgb(24, 27, 36));

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

        var quoteBounds = new RectangleF(width * 0.1f, height * 0.14f, width * 0.8f, height * 0.58f);
        graphics.DrawString(quoteText, quoteFont, quoteBrush, quoteBounds, format);

        if (!string.IsNullOrEmpty(authorText))
        {
            var authorBounds = new RectangleF(width * 0.1f, height * 0.74f, width * 0.8f, height * 0.14f);
            graphics.DrawString(authorText, authorFont, authorBrush, authorBounds, format);
        }

        bitmap.Save(outputPath, ImageFormat.Bmp);
        return outputPath;
    }
}
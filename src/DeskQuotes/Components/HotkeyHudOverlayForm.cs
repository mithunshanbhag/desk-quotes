namespace DeskQuotes.Components;

public class HotkeyHudOverlayForm : Form
{
    private const int BottomMargin = 28;
    private const int CornerRadius = 18;
    private const int HorizontalPadding = 18;
    private const int IconDiameter = 40;
    private const int IconSpacing = 14;
    private const int MessageHorizontalPadding = 24;
    private const int MinimumHeight = 72;
    private const int MinimumWidth = 240;
    private const int TextVerticalPadding = 18;
    private const int WmMouseActivate = 0x0021;
    private const int WsExNoActivate = 0x08000000;
    private const int WsExToolWindow = 0x00000080;
    private static readonly IntPtr MaNoActivate = new(3);
    private readonly System.Windows.Forms.Timer _animationTimer;
    private readonly Font _glyphFont;
    private readonly Font _messageFont;
    private HotkeyHudOverlayContent? _content;
    private OverlayAnimationPhase _animationPhase = OverlayAnimationPhase.Hidden;
    private int _holdTickCount;

    public HotkeyHudOverlayForm()
    {
        AutoScaleMode = AutoScaleMode.None;
        BackColor = Color.FromArgb(26, 29, 36);
        FormBorderStyle = FormBorderStyle.None;
        Opacity = 0;
        ShowInTaskbar = false;
        StartPosition = FormStartPosition.Manual;
        TopMost = true;

        SetStyle(
            ControlStyles.AllPaintingInWmPaint |
            ControlStyles.OptimizedDoubleBuffer |
            ControlStyles.ResizeRedraw |
            ControlStyles.UserPaint,
            true);

        _glyphFont = new Font("Segoe UI", 18f, FontStyle.Bold, GraphicsUnit.Point);
        _messageFont = new Font("Segoe UI", 12f, FontStyle.Bold, GraphicsUnit.Point);
        _animationTimer = new System.Windows.Forms.Timer { Interval = 16 };
        _animationTimer.Tick += HandleAnimationTick;

        Size = new Size(MinimumWidth, MinimumHeight);
        UpdateRoundedRegion();
    }

    protected override bool ShowWithoutActivation => true;

    protected override CreateParams CreateParams
    {
        get
        {
            var createParams = base.CreateParams;
            createParams.ExStyle |= WsExNoActivate | WsExToolWindow;
            return createParams;
        }
    }

    public virtual void ShowOverlay(HotkeyHudOverlayContent content)
    {
        ArgumentNullException.ThrowIfNull(content);

        _content = content;
        _holdTickCount = 36;
        _animationPhase = OverlayAnimationPhase.FadingIn;

        UpdateBoundsForContent();
        Opacity = 0.18d;
        Invalidate();

        if (!Visible)
            Show();

        _animationTimer.Start();
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _animationTimer.Tick -= HandleAnimationTick;
            _animationTimer.Dispose();
            _glyphFont.Dispose();
            _messageFont.Dispose();
            Region?.Dispose();
        }

        base.Dispose(disposing);
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);

        if (_content is null)
            return;

        e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;

        var backgroundBounds = new Rectangle(0, 0, Width - 1, Height - 1);
        using var backgroundPath = CreateRoundedRectanglePath(backgroundBounds, CornerRadius);
        using var backgroundBrush = new SolidBrush(Color.FromArgb(235, 26, 29, 36));
        using var borderPen = new Pen(Color.FromArgb(72, 255, 255, 255));
        e.Graphics.FillPath(backgroundBrush, backgroundPath);
        e.Graphics.DrawPath(borderPen, backgroundPath);

        var iconBounds = new Rectangle(HorizontalPadding, (Height - IconDiameter) / 2, IconDiameter, IconDiameter);
        using var accentBrush = new SolidBrush(GetAccentColor(_content.Kind));
        e.Graphics.FillEllipse(accentBrush, iconBounds);

        TextRenderer.DrawText(
            e.Graphics,
            GetGlyph(_content.Kind),
            _glyphFont,
            iconBounds,
            Color.White,
            TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.SingleLine);

        var messageBounds = new Rectangle(
            iconBounds.Right + IconSpacing,
            0,
            Width - iconBounds.Right - IconSpacing - HorizontalPadding - MessageHorizontalPadding,
            Height);

        TextRenderer.DrawText(
            e.Graphics,
            _content.Message,
            _messageFont,
            messageBounds,
            Color.White,
            TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis | TextFormatFlags.SingleLine);
    }

    protected override void OnResize(EventArgs e)
    {
        base.OnResize(e);
        UpdateRoundedRegion();
    }

    protected override void WndProc(ref Message message)
    {
        if (message.Msg == WmMouseActivate)
        {
            message.Result = MaNoActivate;
            return;
        }

        base.WndProc(ref message);
    }

    private void HandleAnimationTick(object? sender, EventArgs e)
    {
        switch (_animationPhase)
        {
            case OverlayAnimationPhase.FadingIn:
                Opacity = Math.Min(0.94d, Opacity + 0.24d);
                if (Opacity >= 0.94d)
                    _animationPhase = OverlayAnimationPhase.Holding;

                break;

            case OverlayAnimationPhase.Holding:
                _holdTickCount--;
                if (_holdTickCount <= 0)
                    _animationPhase = OverlayAnimationPhase.FadingOut;

                break;

            case OverlayAnimationPhase.FadingOut:
                Opacity = Math.Max(0d, Opacity - 0.14d);
                if (Opacity <= 0d)
                {
                    _animationTimer.Stop();
                    _animationPhase = OverlayAnimationPhase.Hidden;
                    Hide();
                }

                break;

            case OverlayAnimationPhase.Hidden:
                _animationTimer.Stop();
                break;
        }
    }

    private void UpdateBoundsForContent()
    {
        var message = _content?.Message ?? string.Empty;
        var textSize = TextRenderer.MeasureText(
            message,
            _messageFont,
            Size.Empty,
            TextFormatFlags.SingleLine);
        var width = Math.Max(
            MinimumWidth,
            HorizontalPadding * 2 + IconDiameter + IconSpacing + MessageHorizontalPadding + textSize.Width);
        var height = Math.Max(MinimumHeight, TextVerticalPadding * 2 + Math.Max(IconDiameter, textSize.Height));

        Size = new Size(width, height);
        PositionWindow();
        UpdateRoundedRegion();
    }

    private void PositionWindow()
    {
        var workingArea = Screen.PrimaryScreen?.WorkingArea ?? Screen.FromPoint(Cursor.Position).WorkingArea;
        var x = workingArea.Left + (workingArea.Width - Width) / 2;
        var y = workingArea.Bottom - Height - BottomMargin;

        Location = new Point(
            Math.Max(workingArea.Left, x),
            Math.Max(workingArea.Top, y));
    }

    private void UpdateRoundedRegion()
    {
        Region?.Dispose();

        if (Width <= 0 || Height <= 0)
            return;

        using var path = CreateRoundedRectanglePath(new Rectangle(0, 0, Width, Height), CornerRadius);
        Region = new Region(path);
    }

    private static GraphicsPath CreateRoundedRectanglePath(Rectangle bounds, int cornerRadius)
    {
        var diameter = cornerRadius * 2;
        var path = new GraphicsPath();

        path.AddArc(bounds.Left, bounds.Top, diameter, diameter, 180, 90);
        path.AddArc(bounds.Right - diameter, bounds.Top, diameter, diameter, 270, 90);
        path.AddArc(bounds.Right - diameter, bounds.Bottom - diameter, diameter, diameter, 0, 90);
        path.AddArc(bounds.Left, bounds.Bottom - diameter, diameter, diameter, 90, 90);
        path.CloseFigure();

        return path;
    }

    private static Color GetAccentColor(HotkeyHudOverlayKind kind)
    {
        return kind switch
        {
            HotkeyHudOverlayKind.Refresh => Color.FromArgb(68, 168, 255),
            HotkeyHudOverlayKind.BackgroundDarker => Color.FromArgb(222, 137, 67),
            HotkeyHudOverlayKind.BackgroundLighter => Color.FromArgb(214, 184, 96),
            HotkeyHudOverlayKind.RandomBackground => Color.FromArgb(66, 194, 152),
            HotkeyHudOverlayKind.Font => Color.FromArgb(164, 108, 255),
            HotkeyHudOverlayKind.Settings => Color.FromArgb(112, 147, 179),
            _ => Color.FromArgb(68, 168, 255)
        };
    }

    private static string GetGlyph(HotkeyHudOverlayKind kind)
    {
        return kind switch
        {
            HotkeyHudOverlayKind.Refresh => "R",
            HotkeyHudOverlayKind.BackgroundDarker => "-",
            HotkeyHudOverlayKind.BackgroundLighter => "+",
            HotkeyHudOverlayKind.RandomBackground => "C",
            HotkeyHudOverlayKind.Font => "A",
            HotkeyHudOverlayKind.Settings => "S",
            _ => "?"
        };
    }

    private enum OverlayAnimationPhase
    {
        Hidden,
        FadingIn,
        Holding,
        FadingOut
    }
}

using Cairo;
using Vintagestory.API.Client;
using Vintagestory.API.Config;
using Vintagestory.API.MathTools;

namespace ImmersiveStats.Client;

internal sealed class SegmentedStatBarElement : GuiElement
{
    private const double BarX = 8;
    private const double BarHeight = 48;
    private const double IconSize = 28;
    private const double Radius = 5;

    private readonly ImmersiveStatsClientConfig _config;
    private StatBarState _state;
    private bool _editMode;
    private int _textureId;

    public SegmentedStatBarElement(ICoreClientAPI capi, ElementBounds bounds, StatBarState state, ImmersiveStatsClientConfig config)
        : base(capi, bounds)
    {
        _state = state;
        _config = config;
    }

    public void SetState(StatBarState state)
    {
        _state = state;
        Redraw();
    }

    public void SetEditMode(bool editMode)
    {
        if (_editMode == editMode)
        {
            return;
        }

        _editMode = editMode;
        Redraw();
    }

    public void RefreshStyle() => Redraw();

    public override void ComposeElements(Context ctxStatic, ImageSurface surfaceStatic)
    {
        Bounds.CalcWorldBounds();
        Redraw();
    }

    public override void RenderInteractiveElements(float deltaTime)
    {
        if (_textureId <= 0)
        {
            return;
        }

        Render2DTexture(_textureId, Bounds.renderX, Bounds.renderY, Bounds.OuterWidthInt, Bounds.OuterHeightInt, 50, new Vec4f(1, 1, 1, 1));
    }

    public override void OnMouseDownOnElement(ICoreClientAPI api, MouseEvent args)
    {
    }

    public override void Dispose()
    {
        if (_textureId > 0)
        {
            api.Render.GLDeleteTexture(_textureId);
            _textureId = 0;
        }

        base.Dispose();
    }

    private void Redraw()
    {
        Bounds.CalcWorldBounds();
        int width = Math.Max(1, Bounds.OuterWidthInt);
        int height = Math.Max(1, Bounds.OuterHeightInt);

        using ImageSurface surface = new(Format.Argb32, width, height);
        using Context ctx = GenContext(surface);

        double scale = RuntimeEnv.GUIScale;
        double barX = BarX * scale;
        double barHeight = Math.Min(BarHeight * scale, Math.Max(1, height - 12 * scale));
        double barY = Math.Max(0, (height - barHeight) / 2);
        double barWidth = Math.Max(1, width - (BarX * 2 * scale));
        double radius = Radius * scale;

        DrawFrame(ctx, barX, barY, barWidth, barHeight, radius);

        StatBarLayoutResult layout = StatBarLayout.Calculate(_state);
        foreach (StatBarSegment segment in layout.Segments)
        {
            DrawSegment(ctx, segment, barX, barY, barWidth, barHeight, radius);
        }

        DrawDividers(ctx, layout, barX, barY, barWidth, barHeight);
        DrawTickMarks(ctx, barX, barY, barWidth, barHeight, scale);
        DrawSegmentIcons(ctx, layout, barX, barY, barWidth, barHeight, scale);

        if (_editMode)
        {
            DrawEditChrome(ctx, width, height, scale);
        }

        generateTexture(surface, ref _textureId);
    }

    private static void DrawFrame(Context ctx, double x, double y, double width, double height, double radius)
    {
        RoundRectangle(ctx, x, y, width, height, radius);
        ctx.SetSourceRGBA(0.02, 0.025, 0.018, 0.92);
        ctx.FillPreserve();
        ctx.SetSourceRGBA(0, 0, 0, 1);
        ctx.LineWidth = Math.Max(1, 2 * RuntimeEnv.GUIScale);
        ctx.Stroke();

        RoundRectangle(ctx, x + 2, y + 2, Math.Max(1, width - 4), Math.Max(1, height - 4), Math.Max(1, radius - 2));
        ctx.SetSourceRGBA(0.12, 0.12, 0.1, 0.9);
        ctx.Fill();
    }

    private void DrawSegment(Context ctx, StatBarSegment segment, double barX, double barY, double barWidth, double barHeight, double radius)
    {
        double start = barX + barWidth * segment.StartFraction;
        double end = barX + barWidth * segment.EndFraction;
        double width = Math.Max(0, end - start);
        if (width <= 0.5)
        {
            return;
        }

        (double r, double g, double b) = ColorFor(segment.Kind);
        RoundRectangle(ctx, start + 2, barY + 2, Math.Max(1, width - 3), Math.Max(1, barHeight - 4), Math.Max(1, radius - 2));
        ctx.SetSourceRGB(r, g, b);
        ctx.FillPreserve();

        ctx.SetSourceRGBA(Math.Max(0, r - 0.28), Math.Max(0, g - 0.28), Math.Max(0, b - 0.28), 0.85);
        ctx.LineWidth = Math.Max(1, RuntimeEnv.GUIScale);
        ctx.Stroke();

        if (segment.Kind != StatBarSegmentKind.Energy)
        {
            DrawHatch(ctx, start + 4, barY + 3, Math.Max(0, width - 7), Math.Max(0, barHeight - 6), r, g, b);
        }
    }

    private static void DrawHatch(Context ctx, double x, double y, double width, double height, double r, double g, double b)
    {
        if (width <= 2 || height <= 2)
        {
            return;
        }

        ctx.Save();
        Rectangle(ctx, x, y, width, height);
        ctx.Clip();
        ctx.SetSourceRGBA(Math.Min(1, r + 0.18), Math.Min(1, g + 0.18), Math.Min(1, b + 0.18), 0.55);
        ctx.LineWidth = Math.Max(1, RuntimeEnv.GUIScale);

        double spacing = 7 * RuntimeEnv.GUIScale;
        for (double lineX = x - height; lineX < x + width + height; lineX += spacing)
        {
            ctx.MoveTo(lineX, y + height);
            ctx.LineTo(lineX + height, y);
            ctx.Stroke();
        }

        ctx.Restore();
    }

    private static void DrawDividers(Context ctx, StatBarLayoutResult layout, double barX, double barY, double barWidth, double barHeight)
    {
        ctx.SetSourceRGBA(0, 0, 0, 0.55);
        ctx.LineWidth = Math.Max(1, RuntimeEnv.GUIScale);

        foreach (StatBarSegment segment in layout.Segments.Skip(1))
        {
            double x = barX + barWidth * segment.StartFraction;
            ctx.MoveTo(x, barY + 3);
            ctx.LineTo(x, barY + barHeight - 3);
            ctx.Stroke();
        }
    }

    private static void DrawTickMarks(Context ctx, double barX, double barY, double barWidth, double barHeight, double scale)
    {
        ctx.LineWidth = Math.Max(1, scale);

        for (int percent = 5; percent < 100; percent += 5)
        {
            double x = barX + barWidth * percent / 100.0;
            double alpha = percent % 25 == 0 ? 0.42 : 0.26;
            ctx.SetSourceRGBA(0, 0, 0, alpha);
            ctx.MoveTo(x, barY + 4 * scale);
            ctx.LineTo(x, barY + barHeight - 4 * scale);
            ctx.Stroke();
            ctx.SetSourceRGBA(1, 1, 1, alpha * 0.28);
            ctx.MoveTo(x + scale, barY + 5 * scale);
            ctx.LineTo(x + scale, barY + barHeight - 5 * scale);
            ctx.Stroke();
        }
    }

    private static void DrawSegmentIcons(Context ctx, StatBarLayoutResult layout, double barX, double barY, double barWidth, double barHeight, double scale)
    {
        double iconSize = Math.Min(IconSize * scale, Math.Max(1, barHeight - 10 * scale));
        double centerY = barY + barHeight / 2;

        foreach (StatBarSegment segment in layout.Segments)
        {
            double start = barX + barWidth * segment.StartFraction;
            double end = barX + barWidth * segment.EndFraction;
            double width = Math.Max(0, end - start);
            SegmentIconRenderer.Draw(ctx, segment.Kind, start + width / 2, centerY, iconSize, width, scale);
        }
    }

    private static void DrawEditChrome(Context ctx, int width, int height, double scale)
    {
        double inset = Math.Max(1, 2 * scale);
        double handle = Math.Max(8, 10 * scale);
        double radius = Math.Max(3, 4 * scale);

        RoundRectangle(ctx, inset, inset, Math.Max(1, width - 2 * inset), Math.Max(1, height - 2 * inset), radius);
        ctx.SetSourceRGBA(0.85, 0.93, 1, 0.55);
        ctx.LineWidth = Math.Max(1, scale);
        ctx.Stroke();

        DrawHandle(ctx, inset, inset, handle);
        DrawHandle(ctx, width - inset - handle, inset, handle);
        DrawHandle(ctx, inset, height - inset - handle, handle);
        DrawHandle(ctx, width - inset - handle, height - inset - handle, handle);
    }

    private static void DrawHandle(Context ctx, double x, double y, double size)
    {
        RoundRectangle(ctx, x, y, size, size, Math.Max(1, size / 5));
        ctx.SetSourceRGBA(0.03, 0.04, 0.05, 0.78);
        ctx.FillPreserve();
        ctx.SetSourceRGBA(0.85, 0.93, 1, 0.72);
        ctx.LineWidth = Math.Max(1, RuntimeEnv.GUIScale);
        ctx.Stroke();
    }

    private (double R, double G, double B) ColorFor(StatBarSegmentKind kind) => _config.GetColor(kind).ToCairo();
}

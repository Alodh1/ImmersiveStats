using Cairo;
using Vintagestory.API.Client;
using Vintagestory.API.Config;
using Vintagestory.API.MathTools;

namespace ImmersiveStats.Client;

internal sealed class SegmentedStatBarElement : GuiElement
{
    private const double BarX = 8;
    private const double BarY = 24;
    private const double BarHeight = 16;
    private const double Radius = 4;

    private StatBarState _state;
    private int _textureId;

    public SegmentedStatBarElement(ICoreClientAPI capi, ElementBounds bounds, StatBarState state)
        : base(capi, bounds)
    {
        _state = state;
    }

    public void SetState(StatBarState state)
    {
        _state = state;
        Redraw();
    }

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
        double barY = BarY * scale;
        double barHeight = BarHeight * scale;
        double barWidth = Math.Max(1, width - (BarX * 2 * scale));
        double radius = Radius * scale;

        DrawFrame(ctx, barX, barY, barWidth, barHeight, radius);

        StatBarLayoutResult layout = StatBarLayout.Calculate(_state);
        foreach (StatBarSegment segment in layout.Segments)
        {
            DrawSegment(ctx, segment, barX, barY, barWidth, barHeight, radius);
        }

        DrawDividers(ctx, layout, barX, barY, barWidth, barHeight);
        DrawLabels(ctx, layout, barX, barY, barWidth, scale);

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

    private static void DrawSegment(Context ctx, StatBarSegment segment, double barX, double barY, double barWidth, double barHeight, double radius)
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

    private static void DrawLabels(Context ctx, StatBarLayoutResult layout, double barX, double barY, double barWidth, double scale)
    {
        ctx.SelectFontFace("sans-serif", FontSlant.Normal, FontWeight.Bold);
        ctx.SetFontSize(10 * scale);

        foreach (StatBarSegment segment in layout.Segments)
        {
            if (segment.Kind == StatBarSegmentKind.Energy || segment.RenderedAmount < 1)
            {
                continue;
            }

            string label = LabelFor(segment.Kind);
            double center = barX + barWidth * ((segment.StartFraction + segment.EndFraction) / 2);
            double x = center - (label.Length * 3.2 * scale);
            double y = Math.Max(10 * scale, barY - 7 * scale);
            (double r, double g, double b) = ColorFor(segment.Kind);

            ctx.SetSourceRGBA(0, 0, 0, 0.75);
            ctx.MoveTo(x + scale, y + scale);
            ctx.ShowText(label);

            ctx.SetSourceRGB(r, g, b);
            ctx.MoveTo(x, y);
            ctx.ShowText(label);
        }
    }

    private static string LabelFor(StatBarSegmentKind kind) => kind switch
    {
        StatBarSegmentKind.Damage => "DMG",
        StatBarSegmentKind.Cold => "COLD",
        StatBarSegmentKind.Heat => "HEAT",
        StatBarSegmentKind.Hunger => "HUN",
        _ => string.Empty,
    };

    private static (double R, double G, double B) ColorFor(StatBarSegmentKind kind) => kind switch
    {
        StatBarSegmentKind.Energy => (0.28, 0.7, 0.13),
        StatBarSegmentKind.Damage => (0.9, 0.2, 0.16),
        StatBarSegmentKind.Cold => (0.32, 0.52, 0.95),
        StatBarSegmentKind.Heat => (0.95, 0.55, 0.12),
        StatBarSegmentKind.Hunger => (0.63, 0.22, 0.85),
        _ => (0.8, 0.8, 0.8),
    };
}

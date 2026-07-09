using Cairo;
using Vintagestory.API.Config;

namespace ImmersiveStats.Client;

internal static class SegmentIconRenderer
{
    public static bool CanFitIcon(double segmentWidth, double iconSize, double scale)
    {
        return segmentWidth >= iconSize + 8 * scale;
    }

    public static void Draw(Context ctx, StatBarSegmentKind kind, double centerX, double centerY, double size, double segmentWidth, double scale)
    {
        if (kind == StatBarSegmentKind.Energy || !CanFitIcon(segmentWidth, size, scale))
        {
            return;
        }

        ctx.Save();
        ctx.LineJoin = LineJoin.Round;
        ctx.LineCap = LineCap.Round;
        ctx.LineWidth = Math.Max(1.2, 1.6 * scale);

        switch (kind)
        {
            case StatBarSegmentKind.PenetratingTrauma:
                DrawBrokenHeart(ctx, centerX, centerY, size, scale);
                break;
            case StatBarSegmentKind.BluntTrauma:
                DrawBrokenBone(ctx, centerX, centerY, size, scale);
                break;
            case StatBarSegmentKind.Burn:
                DrawFire(ctx, centerX, centerY, size, scale);
                break;
            case StatBarSegmentKind.CoreTemperature:
                DrawIcicle(ctx, centerX, centerY, size, scale);
                break;
            case StatBarSegmentKind.Toxic:
                DrawSkull(ctx, centerX, centerY, size, scale);
                break;
            case StatBarSegmentKind.Asphyxiation:
                DrawLungs(ctx, centerX, centerY, size, scale);
                break;
            case StatBarSegmentKind.Hunger:
                DrawStomach(ctx, centerX, centerY, size, scale);
                break;
        }

        ctx.Restore();
    }

    private static void DrawBrokenHeart(Context ctx, double cx, double cy, double size, double scale)
    {
        double s = size / 2;
        BeginHeartPath(ctx, cx, cy + 0.05 * s, s);
        FillAndStroke(ctx);

        ctx.SetSourceRGBA(0.05, 0.035, 0.03, 0.92);
        ctx.LineWidth = Math.Max(1.2, 1.9 * scale);
        ctx.MoveTo(cx + 0.05 * s, cy - 0.70 * s);
        ctx.LineTo(cx - 0.10 * s, cy - 0.18 * s);
        ctx.LineTo(cx + 0.16 * s, cy + 0.03 * s);
        ctx.LineTo(cx - 0.05 * s, cy + 0.63 * s);
        ctx.Stroke();
    }

    private static void DrawIcicle(Context ctx, double cx, double cy, double size, double scale)
    {
        double s = size / 2;
        ctx.MoveTo(cx - 0.38 * s, cy - 0.74 * s);
        ctx.LineTo(cx + 0.38 * s, cy - 0.74 * s);
        ctx.LineTo(cx + 0.16 * s, cy + 0.08 * s);
        ctx.LineTo(cx, cy + 0.82 * s);
        ctx.LineTo(cx - 0.16 * s, cy + 0.08 * s);
        ctx.ClosePath();
        FillAndStroke(ctx);

        ctx.SetSourceRGBA(1, 1, 1, 0.45);
        ctx.LineWidth = Math.Max(1, scale);
        ctx.MoveTo(cx - 0.08 * s, cy - 0.52 * s);
        ctx.LineTo(cx - 0.02 * s, cy + 0.28 * s);
        ctx.Stroke();
    }

    private static void DrawFire(Context ctx, double cx, double cy, double size, double scale)
    {
        double s = size / 2;
        ctx.MoveTo(cx, cy + 0.80 * s);
        ctx.CurveTo(cx - 0.74 * s, cy + 0.34 * s, cx - 0.18 * s, cy - 0.25 * s, cx - 0.36 * s, cy - 0.78 * s);
        ctx.CurveTo(cx + 0.18 * s, cy - 0.40 * s, cx + 0.78 * s, cy + 0.02 * s, cx + 0.30 * s, cy + 0.80 * s);
        ctx.ClosePath();
        FillAndStroke(ctx);

        ctx.MoveTo(cx, cy + 0.48 * s);
        ctx.CurveTo(cx - 0.24 * s, cy + 0.20 * s, cx + 0.05 * s, cy - 0.12 * s, cx + 0.02 * s, cy - 0.46 * s);
        ctx.CurveTo(cx + 0.28 * s, cy - 0.08 * s, cx + 0.38 * s, cy + 0.24 * s, cx, cy + 0.48 * s);
        ctx.ClosePath();
        ctx.SetSourceRGBA(0.05, 0.035, 0.03, 0.32);
        ctx.Fill();
    }

    private static void DrawSkull(Context ctx, double cx, double cy, double size, double scale)
    {
        double s = size / 2;
        ctx.Arc(cx, cy - 0.16 * s, 0.55 * s, 0, Math.PI * 2);
        FillAndStroke(ctx);
        RoundRect(ctx, cx - 0.36 * s, cy + 0.16 * s, 0.72 * s, 0.48 * s, 0.12 * s);
        FillAndStroke(ctx);

        SetDark(ctx);
        ctx.Arc(cx - 0.18 * s, cy - 0.12 * s, 0.11 * s, 0, Math.PI * 2);
        ctx.Fill();
        ctx.Arc(cx + 0.18 * s, cy - 0.12 * s, 0.11 * s, 0, Math.PI * 2);
        ctx.Fill();
        ctx.MoveTo(cx, cy + 0.02 * s);
        ctx.LineTo(cx - 0.08 * s, cy + 0.18 * s);
        ctx.LineTo(cx + 0.08 * s, cy + 0.18 * s);
        ctx.ClosePath();
        ctx.Fill();
        ctx.LineWidth = Math.Max(0.9, scale);
        for (int i = -1; i <= 1; i++)
        {
            double x = cx + i * 0.12 * s;
            ctx.MoveTo(x, cy + 0.32 * s);
            ctx.LineTo(x, cy + 0.56 * s);
        }

        ctx.Stroke();
    }

    private static void DrawBrokenBone(Context ctx, double cx, double cy, double size, double scale)
    {
        double s = size / 2;
        DrawBoneHalf(ctx, cx - 0.14 * s, cy + 0.10 * s, -0.72 * s, -0.38 * s, scale);
        DrawBoneHalf(ctx, cx + 0.18 * s, cy - 0.06 * s, 0.68 * s, 0.36 * s, scale);

        SetDark(ctx);
        ctx.LineWidth = Math.Max(1.1, 1.6 * scale);
        ctx.MoveTo(cx - 0.03 * s, cy - 0.20 * s);
        ctx.LineTo(cx - 0.20 * s, cy + 0.04 * s);
        ctx.LineTo(cx + 0.02 * s, cy + 0.22 * s);
        ctx.Stroke();
    }

    private static void DrawLungs(Context ctx, double cx, double cy, double size, double scale)
    {
        double s = size / 2;
        ctx.MoveTo(cx, cy - 0.78 * s);
        ctx.LineTo(cx, cy - 0.18 * s);
        ctx.MoveTo(cx, cy - 0.20 * s);
        ctx.LineTo(cx - 0.28 * s, cy - 0.02 * s);
        ctx.MoveTo(cx, cy - 0.20 * s);
        ctx.LineTo(cx + 0.28 * s, cy - 0.02 * s);
        ctx.SetSourceRGBA(0.95, 0.98, 1, 0.90);
        ctx.Stroke();

        ctx.Arc(cx - 0.28 * s, cy + 0.16 * s, 0.34 * s, Math.PI * 0.55, Math.PI * 1.85);
        ctx.CurveTo(cx - 0.08 * s, cy - 0.10 * s, cx - 0.04 * s, cy + 0.62 * s, cx - 0.42 * s, cy + 0.64 * s);
        ctx.ClosePath();
        FillAndStroke(ctx);
        ctx.Arc(cx + 0.28 * s, cy + 0.16 * s, 0.34 * s, Math.PI * 1.15, Math.PI * 2.45);
        ctx.CurveTo(cx + 0.08 * s, cy - 0.10 * s, cx + 0.04 * s, cy + 0.62 * s, cx + 0.42 * s, cy + 0.64 * s);
        ctx.ClosePath();
        FillAndStroke(ctx);
    }

    private static void DrawCollapsedRocks(Context ctx, double cx, double cy, double size, double scale)
    {
        double s = size / 2;
        DrawRock(ctx, cx - 0.36 * s, cy + 0.18 * s, 0.44 * s);
        DrawRock(ctx, cx + 0.22 * s, cy + 0.22 * s, 0.50 * s);
        DrawRock(ctx, cx - 0.02 * s, cy - 0.24 * s, 0.42 * s);
    }

    private static void DrawLightning(Context ctx, double cx, double cy, double size, double scale)
    {
        double s = size / 2;
        ctx.MoveTo(cx + 0.18 * s, cy - 0.84 * s);
        ctx.LineTo(cx - 0.34 * s, cy + 0.05 * s);
        ctx.LineTo(cx + 0.02 * s, cy + 0.05 * s);
        ctx.LineTo(cx - 0.18 * s, cy + 0.84 * s);
        ctx.LineTo(cx + 0.48 * s, cy - 0.18 * s);
        ctx.LineTo(cx + 0.10 * s, cy - 0.18 * s);
        ctx.ClosePath();
        FillAndStroke(ctx);
    }

    private static void DrawStripedDrop(Context ctx, double cx, double cy, double size, double scale)
    {
        double s = size / 2;
        BeginDropPath(ctx, cx, cy, s);
        FillAndStroke(ctx);

        ctx.Save();
        BeginDropPath(ctx, cx, cy, s);
        ctx.Clip();
        SetDark(ctx);
        ctx.LineWidth = Math.Max(1, 1.2 * scale);
        for (double y = cy - 0.46 * s; y <= cy + 0.62 * s; y += 0.24 * s)
        {
            ctx.MoveTo(cx - 0.55 * s, y);
            ctx.LineTo(cx + 0.55 * s, y + 0.22 * s);
        }

        ctx.Stroke();
        ctx.Restore();
    }

    private static void DrawStomach(Context ctx, double cx, double cy, double size, double scale)
    {
        double s = size / 2;
        ctx.MoveTo(cx - 0.02 * s, cy - 0.78 * s);
        ctx.CurveTo(cx + 0.20 * s, cy - 0.46 * s, cx + 0.12 * s, cy - 0.20 * s, cx - 0.12 * s, cy - 0.10 * s);
        ctx.CurveTo(cx - 0.62 * s, cy + 0.10 * s, cx - 0.28 * s, cy + 0.78 * s, cx + 0.20 * s, cy + 0.62 * s);
        ctx.CurveTo(cx + 0.72 * s, cy + 0.44 * s, cx + 0.62 * s, cy - 0.18 * s, cx + 0.20 * s, cy - 0.12 * s);
        ctx.CurveTo(cx + 0.02 * s, cy - 0.10 * s, cx + 0.04 * s, cy - 0.50 * s, cx - 0.08 * s, cy - 0.78 * s);
        ctx.ClosePath();
        FillAndStroke(ctx);
    }

    private static void DrawBoneHalf(Context ctx, double cx, double cy, double dx, double dy, double scale)
    {
        ctx.SetSourceRGBA(0.95, 0.98, 1, 0.90);
        ctx.LineWidth = Math.Max(3, 4.2 * scale);
        ctx.MoveTo(cx, cy);
        ctx.LineTo(cx + dx, cy + dy);
        ctx.Stroke();

        double r = Math.Max(2.5, 3.3 * scale);
        ctx.Arc(cx + dx, cy + dy, r, 0, Math.PI * 2);
        ctx.FillPreserve();
        StrokeDark(ctx);
    }

    private static void DrawRock(Context ctx, double cx, double cy, double size)
    {
        ctx.MoveTo(cx - 0.55 * size, cy + 0.28 * size);
        ctx.LineTo(cx - 0.28 * size, cy - 0.48 * size);
        ctx.LineTo(cx + 0.30 * size, cy - 0.42 * size);
        ctx.LineTo(cx + 0.58 * size, cy + 0.26 * size);
        ctx.LineTo(cx, cy + 0.56 * size);
        ctx.ClosePath();
        FillAndStroke(ctx);
    }

    private static void BeginHeartPath(Context ctx, double cx, double cy, double s)
    {
        ctx.MoveTo(cx, cy + 0.72 * s);
        ctx.CurveTo(cx - 1.00 * s, cy + 0.10 * s, cx - 0.90 * s, cy - 0.72 * s, cx - 0.28 * s, cy - 0.62 * s);
        ctx.CurveTo(cx - 0.08 * s, cy - 0.58 * s, cx, cy - 0.36 * s, cx, cy - 0.22 * s);
        ctx.CurveTo(cx, cy - 0.36 * s, cx + 0.08 * s, cy - 0.58 * s, cx + 0.28 * s, cy - 0.62 * s);
        ctx.CurveTo(cx + 0.90 * s, cy - 0.72 * s, cx + 1.00 * s, cy + 0.10 * s, cx, cy + 0.72 * s);
        ctx.ClosePath();
    }

    private static void BeginDropPath(Context ctx, double cx, double cy, double s)
    {
        ctx.MoveTo(cx, cy - 0.84 * s);
        ctx.CurveTo(cx - 0.58 * s, cy - 0.08 * s, cx - 0.66 * s, cy + 0.24 * s, cx - 0.42 * s, cy + 0.52 * s);
        ctx.CurveTo(cx - 0.18 * s, cy + 0.82 * s, cx + 0.18 * s, cy + 0.82 * s, cx + 0.42 * s, cy + 0.52 * s);
        ctx.CurveTo(cx + 0.66 * s, cy + 0.24 * s, cx + 0.58 * s, cy - 0.08 * s, cx, cy - 0.84 * s);
        ctx.ClosePath();
    }

    private static void RoundRect(Context ctx, double x, double y, double width, double height, double radius)
    {
        ctx.NewSubPath();
        ctx.Arc(x + width - radius, y + radius, radius, -Math.PI / 2, 0);
        ctx.Arc(x + width - radius, y + height - radius, radius, 0, Math.PI / 2);
        ctx.Arc(x + radius, y + height - radius, radius, Math.PI / 2, Math.PI);
        ctx.Arc(x + radius, y + radius, radius, Math.PI, Math.PI * 1.5);
        ctx.ClosePath();
    }

    private static void FillAndStroke(Context ctx)
    {
        ctx.SetSourceRGBA(0.95, 0.98, 1, 0.90);
        ctx.FillPreserve();
        StrokeDark(ctx);
    }

    private static void StrokeDark(Context ctx)
    {
        SetDark(ctx);
        ctx.Stroke();
    }

    private static void SetDark(Context ctx)
    {
        ctx.SetSourceRGBA(0.05, 0.035, 0.03, 0.78);
    }
}

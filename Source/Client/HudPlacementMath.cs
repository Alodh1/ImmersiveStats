namespace ImmersiveStats.Client;

internal enum HudResizeHandle
{
    None,
    TopLeft,
    TopRight,
    BottomLeft,
    BottomRight,
}

internal readonly record struct HudRect(double X, double Y, double Width, double Height);

internal static class HudPlacementMath
{
    public static HudRect Clamp(HudRect rect, double viewportWidth, double viewportHeight, double minWidth, double minHeight)
    {
        viewportWidth = PositiveOrDefault(viewportWidth, minWidth);
        viewportHeight = PositiveOrDefault(viewportHeight, minHeight);
        minWidth = PositiveOrDefault(minWidth, 1);
        minHeight = PositiveOrDefault(minHeight, 1);

        double effectiveMinWidth = Math.Min(minWidth, viewportWidth);
        double effectiveMinHeight = Math.Min(minHeight, viewportHeight);
        double width = ClampValue(FiniteOrDefault(rect.Width, effectiveMinWidth), effectiveMinWidth, viewportWidth);
        double height = ClampValue(FiniteOrDefault(rect.Height, effectiveMinHeight), effectiveMinHeight, viewportHeight);
        double x = ClampValue(FiniteOrDefault(rect.X, (viewportWidth - width) / 2), 0, Math.Max(0, viewportWidth - width));
        double y = ClampValue(FiniteOrDefault(rect.Y, (viewportHeight - height) / 2), 0, Math.Max(0, viewportHeight - height));

        return new HudRect(x, y, width, height);
    }

    public static HudRect Move(HudRect rect, double deltaX, double deltaY, double viewportWidth, double viewportHeight, double minWidth, double minHeight)
    {
        return Clamp(rect with { X = rect.X + deltaX, Y = rect.Y + deltaY }, viewportWidth, viewportHeight, minWidth, minHeight);
    }

    public static HudRect Resize(HudRect rect, HudResizeHandle handle, double deltaX, double deltaY, double viewportWidth, double viewportHeight, double minWidth, double minHeight)
    {
        if (handle == HudResizeHandle.None)
        {
            return Clamp(rect, viewportWidth, viewportHeight, minWidth, minHeight);
        }

        viewportWidth = PositiveOrDefault(viewportWidth, minWidth);
        viewportHeight = PositiveOrDefault(viewportHeight, minHeight);
        minWidth = Math.Min(PositiveOrDefault(minWidth, 1), viewportWidth);
        minHeight = Math.Min(PositiveOrDefault(minHeight, 1), viewportHeight);

        double left = FiniteOrDefault(rect.X, 0);
        double top = FiniteOrDefault(rect.Y, 0);
        double right = left + FiniteOrDefault(rect.Width, minWidth);
        double bottom = top + FiniteOrDefault(rect.Height, minHeight);

        switch (handle)
        {
            case HudResizeHandle.TopLeft:
                left = ClampValue(left + deltaX, 0, right - minWidth);
                top = ClampValue(top + deltaY, 0, bottom - minHeight);
                break;
            case HudResizeHandle.TopRight:
                right = ClampValue(right + deltaX, left + minWidth, viewportWidth);
                top = ClampValue(top + deltaY, 0, bottom - minHeight);
                break;
            case HudResizeHandle.BottomLeft:
                left = ClampValue(left + deltaX, 0, right - minWidth);
                bottom = ClampValue(bottom + deltaY, top + minHeight, viewportHeight);
                break;
            case HudResizeHandle.BottomRight:
                right = ClampValue(right + deltaX, left + minWidth, viewportWidth);
                bottom = ClampValue(bottom + deltaY, top + minHeight, viewportHeight);
                break;
        }

        return Clamp(new HudRect(left, top, right - left, bottom - top), viewportWidth, viewportHeight, minWidth, minHeight);
    }

    private static double PositiveOrDefault(double value, double fallback)
    {
        return double.IsFinite(value) && value > 0 ? value : fallback;
    }

    private static double FiniteOrDefault(double value, double fallback)
    {
        return double.IsFinite(value) ? value : fallback;
    }

    private static double ClampValue(double value, double min, double max)
    {
        if (max < min)
        {
            max = min;
        }

        return Math.Min(max, Math.Max(min, value));
    }
}

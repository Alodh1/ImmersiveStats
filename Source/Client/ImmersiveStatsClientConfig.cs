namespace ImmersiveStats.Client;

internal sealed class ImmersiveStatsClientConfig
{
    public const string FileName = "immersivestats.client.json";
    public const double DefaultBarWidth = 440;
    public const double DefaultBarHeight = 58;
    public const double MinimumBarWidth = 160;
    public const double MinimumBarHeight = 36;
    public const int MaximumDebugValue = 100;

    public double? BarX { get; set; }

    public double? BarY { get; set; }

    public double BarWidth { get; set; } = DefaultBarWidth;

    public double BarHeight { get; set; } = DefaultBarHeight;

    public bool DebugModeEnabled { get; set; }

    public int DebugDamage { get; set; } = 18;

    public int DebugCold { get; set; } = 9;

    public int DebugHeat { get; set; } = 6;

    public int DebugHunger { get; set; } = 22;

    public ImmersiveStatsRgbColor? EnergyColor { get; set; } = ImmersiveStatsRgbColor.FromRgb(71, 179, 33);

    public ImmersiveStatsRgbColor? DamageColor { get; set; } = ImmersiveStatsRgbColor.FromRgb(230, 51, 41);

    public ImmersiveStatsRgbColor? ColdColor { get; set; } = ImmersiveStatsRgbColor.FromRgb(82, 133, 242);

    public ImmersiveStatsRgbColor? HeatColor { get; set; } = ImmersiveStatsRgbColor.FromRgb(242, 140, 31);

    public ImmersiveStatsRgbColor? HungerColor { get; set; } = ImmersiveStatsRgbColor.FromRgb(161, 56, 217);

    public void Normalize(double viewportWidth, double viewportHeight)
    {
        HudRect centered = new(
            (viewportWidth - DefaultBarWidth) / 2,
            (viewportHeight - DefaultBarHeight) / 2,
            BarWidth,
            BarHeight);

        HudRect source = new(
            BarX ?? centered.X,
            BarY ?? centered.Y,
            BarWidth,
            BarHeight);

        SetBarRect(HudPlacementMath.Clamp(source, viewportWidth, viewportHeight, MinimumBarWidth, MinimumBarHeight));

        DebugDamage = ClampDebugValue(DebugDamage);
        DebugCold = ClampDebugValue(DebugCold);
        DebugHeat = ClampDebugValue(DebugHeat);
        DebugHunger = ClampDebugValue(DebugHunger);

        EnergyColor = NormalizeColor(EnergyColor, 71, 179, 33);
        DamageColor = NormalizeColor(DamageColor, 230, 51, 41);
        ColdColor = NormalizeColor(ColdColor, 82, 133, 242);
        HeatColor = NormalizeColor(HeatColor, 242, 140, 31);
        HungerColor = NormalizeColor(HungerColor, 161, 56, 217);
    }

    public HudRect GetBarRect()
    {
        return new HudRect(BarX ?? 0, BarY ?? 0, BarWidth, BarHeight);
    }

    public void SetBarRect(HudRect rect)
    {
        BarX = rect.X;
        BarY = rect.Y;
        BarWidth = rect.Width;
        BarHeight = rect.Height;
    }

    public StatBarState ToState()
    {
        return new StatBarState(StatBarLayout.DefaultCapacity, DebugDamage, DebugCold, DebugHeat, DebugHunger);
    }

    public ImmersiveStatsRgbColor GetColor(StatBarSegmentKind kind)
    {
        return kind switch
        {
            StatBarSegmentKind.Energy => EnergyColor ?? ImmersiveStatsRgbColor.FromRgb(71, 179, 33),
            StatBarSegmentKind.Damage => DamageColor ?? ImmersiveStatsRgbColor.FromRgb(230, 51, 41),
            StatBarSegmentKind.Cold => ColdColor ?? ImmersiveStatsRgbColor.FromRgb(82, 133, 242),
            StatBarSegmentKind.Heat => HeatColor ?? ImmersiveStatsRgbColor.FromRgb(242, 140, 31),
            StatBarSegmentKind.Hunger => HungerColor ?? ImmersiveStatsRgbColor.FromRgb(161, 56, 217),
            _ => ImmersiveStatsRgbColor.FromRgb(204, 204, 204),
        };
    }

    public void SetColor(StatBarSegmentKind kind, ImmersiveStatsRgbColor color)
    {
        color.Normalize();
        switch (kind)
        {
            case StatBarSegmentKind.Energy:
                EnergyColor = color;
                break;
            case StatBarSegmentKind.Damage:
                DamageColor = color;
                break;
            case StatBarSegmentKind.Cold:
                ColdColor = color;
                break;
            case StatBarSegmentKind.Heat:
                HeatColor = color;
                break;
            case StatBarSegmentKind.Hunger:
                HungerColor = color;
                break;
        }
    }

    public int GetDebugValue(StatBarSegmentKind kind)
    {
        return kind switch
        {
            StatBarSegmentKind.Damage => DebugDamage,
            StatBarSegmentKind.Cold => DebugCold,
            StatBarSegmentKind.Heat => DebugHeat,
            StatBarSegmentKind.Hunger => DebugHunger,
            _ => 0,
        };
    }

    public void SetDebugValue(StatBarSegmentKind kind, int value)
    {
        value = ClampDebugValue(value);
        switch (kind)
        {
            case StatBarSegmentKind.Damage:
                DebugDamage = value;
                break;
            case StatBarSegmentKind.Cold:
                DebugCold = value;
                break;
            case StatBarSegmentKind.Heat:
                DebugHeat = value;
                break;
            case StatBarSegmentKind.Hunger:
                DebugHunger = value;
                break;
        }
    }

    private static ImmersiveStatsRgbColor NormalizeColor(ImmersiveStatsRgbColor? color, int defaultR, int defaultG, int defaultB)
    {
        color ??= ImmersiveStatsRgbColor.FromRgb(defaultR, defaultG, defaultB);
        color.Normalize();
        return color;
    }

    private static int ClampDebugValue(int value)
    {
        return Math.Min(MaximumDebugValue, Math.Max(0, value));
    }
}

internal sealed class ImmersiveStatsRgbColor
{
    public int R { get; set; }

    public int G { get; set; }

    public int B { get; set; }

    public static ImmersiveStatsRgbColor FromRgb(int r, int g, int b)
    {
        return new ImmersiveStatsRgbColor { R = r, G = g, B = b };
    }

    public void Normalize()
    {
        R = ClampChannel(R);
        G = ClampChannel(G);
        B = ClampChannel(B);
    }

    public (double R, double G, double B) ToCairo()
    {
        Normalize();
        return (R / 255.0, G / 255.0, B / 255.0);
    }

    public ImmersiveStatsRgbColor Copy()
    {
        return FromRgb(R, G, B);
    }

    private static int ClampChannel(int value)
    {
        return Math.Min(255, Math.Max(0, value));
    }
}

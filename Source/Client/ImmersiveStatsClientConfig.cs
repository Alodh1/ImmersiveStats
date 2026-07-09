namespace ImmersiveStats.Client;

internal sealed class ImmersiveStatsClientConfig
{
    public const string FileName = "immersivestats.client.json";
    public const double DefaultBarWidth = 440;
    public const double DefaultBarHeight = 96;
    public const double MinimumBarWidth = 160;
    public const double MinimumBarHeight = 72;
    public const int MaximumDebugValue = 5000;

    public double? BarX { get; set; }

    public double? BarY { get; set; }

    public double BarWidth { get; set; } = DefaultBarWidth;

    public double BarHeight { get; set; } = DefaultBarHeight;

    public bool DebugModeEnabled { get; set; }

    public int DebugPenetratingTrauma { get; set; } = 750;

    public int DebugBluntTrauma { get; set; } = 500;

    public int DebugBurn { get; set; } = 250;

    public int DebugCoreTemperature { get; set; }

    public int DebugToxic { get; set; }

    public int DebugAsphyxiation { get; set; }

    public int DebugHunger { get; set; } = 2000;

    public int DebugDamage { get; set; }

    public int DebugCold { get; set; }

    public int DebugHeat { get; set; }

    public int DebugPoison { get; set; }

    public int DebugFall { get; set; }

    public int DebugSuffocation { get; set; }

    public int DebugCrushing { get; set; }

    public int DebugElectricity { get; set; }

    public int DebugAcid { get; set; }

    public ImmersiveStatsRgbColor? EnergyColor { get; set; }

    public ImmersiveStatsRgbColor? PenetratingTraumaColor { get; set; }

    public ImmersiveStatsRgbColor? BluntTraumaColor { get; set; }

    public ImmersiveStatsRgbColor? BurnColor { get; set; }

    public ImmersiveStatsRgbColor? CoreTemperatureColor { get; set; }

    public ImmersiveStatsRgbColor? ToxicColor { get; set; }

    public ImmersiveStatsRgbColor? AsphyxiationColor { get; set; }

    public ImmersiveStatsRgbColor? HungerColor { get; set; }

    public ImmersiveStatsRgbColor? DamageColor { get; set; }

    public ImmersiveStatsRgbColor? ColdColor { get; set; }

    public ImmersiveStatsRgbColor? HeatColor { get; set; }

    public ImmersiveStatsRgbColor? PoisonColor { get; set; }

    public ImmersiveStatsRgbColor? FallColor { get; set; }

    public ImmersiveStatsRgbColor? SuffocationColor { get; set; }

    public ImmersiveStatsRgbColor? CrushingColor { get; set; }

    public ImmersiveStatsRgbColor? ElectricityColor { get; set; }

    public ImmersiveStatsRgbColor? AcidColor { get; set; }

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

        DebugPenetratingTrauma = ClampDebugValue(DebugPenetratingTrauma);
        DebugBluntTrauma = ClampDebugValue(DebugBluntTrauma);
        DebugBurn = ClampDebugValue(DebugBurn);
        DebugCoreTemperature = ClampDebugValue(DebugCoreTemperature);
        DebugToxic = ClampDebugValue(DebugToxic);
        DebugAsphyxiation = ClampDebugValue(DebugAsphyxiation);
        DebugHunger = ClampDebugValue(DebugHunger);

        DebugDamage = ClampDebugValue(DebugDamage);
        DebugCold = ClampDebugValue(DebugCold);
        DebugHeat = ClampDebugValue(DebugHeat);
        DebugPoison = ClampDebugValue(DebugPoison);
        DebugFall = ClampDebugValue(DebugFall);
        DebugSuffocation = ClampDebugValue(DebugSuffocation);
        DebugCrushing = ClampDebugValue(DebugCrushing);
        DebugElectricity = ClampDebugValue(DebugElectricity);
        DebugAcid = ClampDebugValue(DebugAcid);

        EnergyColor = NormalizeColor(EnergyColor, null, 71, 179, 33);
        PenetratingTraumaColor = NormalizeColor(PenetratingTraumaColor, null, 186, 35, 63);
        BluntTraumaColor = NormalizeColor(BluntTraumaColor, DamageColor, 207, 84, 55);
        BurnColor = NormalizeColor(BurnColor, HeatColor, 242, 140, 31);
        CoreTemperatureColor = NormalizeColor(CoreTemperatureColor, ColdColor, 82, 133, 242);
        ToxicColor = NormalizeColor(ToxicColor, PoisonColor, 86, 184, 69);
        AsphyxiationColor = NormalizeColor(AsphyxiationColor, SuffocationColor, 67, 188, 204);
        HungerColor = NormalizeColor(HungerColor, null, 161, 56, 217);
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
        return new StatBarState(StatBarLayout.DefaultCapacity, new Dictionary<StatBarSegmentKind, float>
        {
            [StatBarSegmentKind.PenetratingTrauma] = DebugPenetratingTrauma,
            [StatBarSegmentKind.BluntTrauma] = DebugBluntTrauma,
            [StatBarSegmentKind.Burn] = DebugBurn,
            [StatBarSegmentKind.CoreTemperature] = DebugCoreTemperature,
            [StatBarSegmentKind.Toxic] = DebugToxic,
            [StatBarSegmentKind.Asphyxiation] = DebugAsphyxiation,
            [StatBarSegmentKind.Hunger] = DebugHunger,
        });
    }

    public ImmersiveStatsRgbColor GetColor(StatBarSegmentKind kind)
    {
        return kind switch
        {
            StatBarSegmentKind.Energy => EnergyColor ?? ImmersiveStatsRgbColor.FromRgb(71, 179, 33),
            StatBarSegmentKind.PenetratingTrauma => PenetratingTraumaColor ?? ImmersiveStatsRgbColor.FromRgb(186, 35, 63),
            StatBarSegmentKind.BluntTrauma => BluntTraumaColor ?? ImmersiveStatsRgbColor.FromRgb(207, 84, 55),
            StatBarSegmentKind.Burn => BurnColor ?? ImmersiveStatsRgbColor.FromRgb(242, 140, 31),
            StatBarSegmentKind.CoreTemperature => CoreTemperatureColor ?? ImmersiveStatsRgbColor.FromRgb(82, 133, 242),
            StatBarSegmentKind.Toxic => ToxicColor ?? ImmersiveStatsRgbColor.FromRgb(86, 184, 69),
            StatBarSegmentKind.Asphyxiation => AsphyxiationColor ?? ImmersiveStatsRgbColor.FromRgb(67, 188, 204),
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
            case StatBarSegmentKind.PenetratingTrauma:
                PenetratingTraumaColor = color;
                break;
            case StatBarSegmentKind.BluntTrauma:
                BluntTraumaColor = color;
                break;
            case StatBarSegmentKind.Burn:
                BurnColor = color;
                break;
            case StatBarSegmentKind.CoreTemperature:
                CoreTemperatureColor = color;
                break;
            case StatBarSegmentKind.Toxic:
                ToxicColor = color;
                break;
            case StatBarSegmentKind.Asphyxiation:
                AsphyxiationColor = color;
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
            StatBarSegmentKind.PenetratingTrauma => DebugPenetratingTrauma,
            StatBarSegmentKind.BluntTrauma => DebugBluntTrauma,
            StatBarSegmentKind.Burn => DebugBurn,
            StatBarSegmentKind.CoreTemperature => DebugCoreTemperature,
            StatBarSegmentKind.Toxic => DebugToxic,
            StatBarSegmentKind.Asphyxiation => DebugAsphyxiation,
            StatBarSegmentKind.Hunger => DebugHunger,
            _ => 0,
        };
    }

    public void SetDebugValue(StatBarSegmentKind kind, int value)
    {
        value = ClampDebugValue(value);
        switch (kind)
        {
            case StatBarSegmentKind.PenetratingTrauma:
                DebugPenetratingTrauma = value;
                break;
            case StatBarSegmentKind.BluntTrauma:
                DebugBluntTrauma = value;
                break;
            case StatBarSegmentKind.Burn:
                DebugBurn = value;
                break;
            case StatBarSegmentKind.CoreTemperature:
                DebugCoreTemperature = value;
                break;
            case StatBarSegmentKind.Toxic:
                DebugToxic = value;
                break;
            case StatBarSegmentKind.Asphyxiation:
                DebugAsphyxiation = value;
                break;
            case StatBarSegmentKind.Hunger:
                DebugHunger = value;
                break;
        }
    }

    private static ImmersiveStatsRgbColor NormalizeColor(ImmersiveStatsRgbColor? color, ImmersiveStatsRgbColor? legacy, int defaultR, int defaultG, int defaultB)
    {
        color ??= legacy?.Copy() ?? ImmersiveStatsRgbColor.FromRgb(defaultR, defaultG, defaultB);
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
        R = Clamp(R);
        G = Clamp(G);
        B = Clamp(B);
    }

    public ImmersiveStatsRgbColor Copy()
    {
        return FromRgb(R, G, B);
    }

    public (double R, double G, double B) ToCairo()
    {
        return (R / 255.0, G / 255.0, B / 255.0);
    }

    private static int Clamp(int value)
    {
        return Math.Min(255, Math.Max(0, value));
    }
}

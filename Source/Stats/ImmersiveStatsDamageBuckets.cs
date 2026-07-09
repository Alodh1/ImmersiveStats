namespace ImmersiveStats.Stats;

internal sealed class ImmersiveStatsDamageBuckets
{
    private readonly Dictionary<StatBarSegmentKind, float> _amounts = new();

    public float PenetratingTrauma => Get(StatBarSegmentKind.PenetratingTrauma);

    public float BluntTrauma => Get(StatBarSegmentKind.BluntTrauma);

    public float Burn => Get(StatBarSegmentKind.Burn);

    public float CoreTemperature => Get(StatBarSegmentKind.CoreTemperature);

    public float Toxic => Get(StatBarSegmentKind.Toxic);

    public float Asphyxiation => Get(StatBarSegmentKind.Asphyxiation);

    public float Hunger => Get(StatBarSegmentKind.Hunger);

    public IReadOnlyDictionary<StatBarSegmentKind, float> Amounts => _amounts;

    public void Add(StatBarSegmentKind kind, float energy)
    {
        AddDelta(kind, energy);
    }

    public void AddDelta(StatBarSegmentKind kind, float energyDelta)
    {
        if (!StatBarSegmentCatalog.IsReducer(kind) || !IsFinite(energyDelta))
        {
            return;
        }

        _amounts[kind] = Math.Max(0, Get(kind) + energyDelta);
    }

    public void Clear()
    {
        _amounts.Clear();
    }

    private float Get(StatBarSegmentKind kind)
    {
        return _amounts.TryGetValue(kind, out float amount) ? amount : 0;
    }

    private static bool IsFinite(float value)
    {
        return !float.IsNaN(value) && !float.IsInfinity(value);
    }
}

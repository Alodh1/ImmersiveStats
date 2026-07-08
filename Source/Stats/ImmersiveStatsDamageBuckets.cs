namespace ImmersiveStats.Stats;

internal sealed class ImmersiveStatsDamageBuckets
{
    private readonly Dictionary<StatBarSegmentKind, float> _amounts = new();

    public float Damage => Get(StatBarSegmentKind.Damage);

    public float Cold => Get(StatBarSegmentKind.Cold);

    public float Heat => Get(StatBarSegmentKind.Heat);

    public float Poison => Get(StatBarSegmentKind.Poison);

    public float Fall => Get(StatBarSegmentKind.Fall);

    public float Suffocation => Get(StatBarSegmentKind.Suffocation);

    public float Crushing => Get(StatBarSegmentKind.Crushing);

    public float Electricity => Get(StatBarSegmentKind.Electricity);

    public float Acid => Get(StatBarSegmentKind.Acid);

    public float Hunger => Get(StatBarSegmentKind.Hunger);

    public IReadOnlyDictionary<StatBarSegmentKind, float> Amounts => _amounts;

    public void Add(StatBarSegmentKind kind, float healthPoints)
    {
        if (!StatBarSegmentCatalog.IsReducer(kind) || !IsFinitePositive(healthPoints))
        {
            return;
        }

        _amounts[kind] = Get(kind) + healthPoints;
    }

    public void ReconcileToMissingHealth(float missingHealth)
    {
        missingHealth = SanitizeAmount(missingHealth);
        if (missingHealth <= 0)
        {
            Clear();
            return;
        }

        float tracked = Total;
        if (tracked <= 0)
        {
            _amounts[StatBarSegmentKind.Damage] = missingHealth;
            return;
        }

        if (tracked < missingHealth)
        {
            _amounts[StatBarSegmentKind.Damage] = Damage + missingHealth - tracked;
            return;
        }

        float scale = missingHealth / tracked;
        foreach (StatBarSegmentKind kind in StatBarSegmentCatalog.ReducerKinds)
        {
            _amounts[kind] = Get(kind) * scale;
        }
    }

    public void Clear()
    {
        _amounts.Clear();
    }

    private float Total => StatBarSegmentCatalog.ReducerKinds.Sum(Get);

    private float Get(StatBarSegmentKind kind)
    {
        return _amounts.TryGetValue(kind, out float amount) ? amount : 0;
    }

    private static float SanitizeAmount(float value)
    {
        return float.IsNaN(value) || float.IsInfinity(value) ? 0 : Math.Max(0, value);
    }

    private static bool IsFinitePositive(float value)
    {
        return !float.IsNaN(value) && !float.IsInfinity(value) && value > 0;
    }
}

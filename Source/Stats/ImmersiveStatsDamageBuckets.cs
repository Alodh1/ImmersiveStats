namespace ImmersiveStats.Stats;

internal sealed class ImmersiveStatsDamageBuckets
{
    public float Damage { get; private set; }

    public float Cold { get; private set; }

    public float Heat { get; private set; }

    public float Hunger { get; private set; }

    public void Add(StatBarSegmentKind kind, float healthPoints)
    {
        if (!IsFinitePositive(healthPoints))
        {
            return;
        }

        switch (kind)
        {
            case StatBarSegmentKind.Cold:
                Cold += healthPoints;
                break;
            case StatBarSegmentKind.Heat:
                Heat += healthPoints;
                break;
            case StatBarSegmentKind.Hunger:
                Hunger += healthPoints;
                break;
            default:
                Damage += healthPoints;
                break;
        }
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
            Damage = missingHealth;
            return;
        }

        if (tracked < missingHealth)
        {
            Damage += missingHealth - tracked;
            return;
        }

        float scale = missingHealth / tracked;
        Damage *= scale;
        Cold *= scale;
        Heat *= scale;
        Hunger *= scale;
    }

    public void Clear()
    {
        Damage = 0;
        Cold = 0;
        Heat = 0;
        Hunger = 0;
    }

    private float Total => Damage + Cold + Heat + Hunger;

    private static float SanitizeAmount(float value)
    {
        return float.IsNaN(value) || float.IsInfinity(value) ? 0 : Math.Max(0, value);
    }

    private static bool IsFinitePositive(float value)
    {
        return !float.IsNaN(value) && !float.IsInfinity(value) && value > 0;
    }
}

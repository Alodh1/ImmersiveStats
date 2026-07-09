using Vintagestory.API.Common;

namespace ImmersiveStats.Stats;

internal static class ImmersiveStatsDamageSourceClassifier
{
    public static bool TryClassifyImmediate(DamageSource? damageSource, out StatBarSegmentKind kind)
    {
        kind = StatBarSegmentKind.BluntTrauma;
        if (damageSource is null)
        {
            return true;
        }

        if (damageSource.Source == EnumDamageSource.Drown || damageSource.Type == EnumDamageType.Suffocation)
        {
            kind = StatBarSegmentKind.Asphyxiation;
            return true;
        }

        if (damageSource.Type == EnumDamageType.Frost && damageSource.Source == EnumDamageSource.Weather)
        {
            return false;
        }

        switch (damageSource.Type)
        {
            case EnumDamageType.PiercingAttack:
            case EnumDamageType.SlashingAttack:
                kind = StatBarSegmentKind.PenetratingTrauma;
                return true;
            case EnumDamageType.BluntAttack:
            case EnumDamageType.Gravity:
            case EnumDamageType.Crushing:
            case EnumDamageType.Injury:
                kind = StatBarSegmentKind.BluntTrauma;
                return true;
            case EnumDamageType.Fire:
            case EnumDamageType.Frost:
            case EnumDamageType.Heat:
                kind = StatBarSegmentKind.Burn;
                return true;
            case EnumDamageType.Poison:
                kind = StatBarSegmentKind.Toxic;
                return true;
            case EnumDamageType.Hunger:
                kind = StatBarSegmentKind.Hunger;
                return true;
            case EnumDamageType.Electricity:
            case EnumDamageType.Acid:
                return false;
        }

        if (damageSource.Source == EnumDamageSource.Fall)
        {
            kind = StatBarSegmentKind.BluntTrauma;
            return true;
        }

        kind = StatBarSegmentKind.BluntTrauma;
        return true;
    }

    public static StatBarSegmentKind Classify(DamageSource? damageSource)
    {
        return TryClassifyImmediate(damageSource, out StatBarSegmentKind kind)
            ? kind
            : StatBarSegmentKind.BluntTrauma;
    }
}

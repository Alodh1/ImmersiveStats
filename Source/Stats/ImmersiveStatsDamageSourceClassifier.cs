using Vintagestory.API.Common;

namespace ImmersiveStats.Stats;

internal static class ImmersiveStatsDamageSourceClassifier
{
    public static StatBarSegmentKind Classify(DamageSource? damageSource)
    {
        if (damageSource is null)
        {
            return StatBarSegmentKind.Damage;
        }

        return damageSource.Type switch
        {
            EnumDamageType.Frost => StatBarSegmentKind.Cold,
            EnumDamageType.Fire or EnumDamageType.Heat => StatBarSegmentKind.Heat,
            EnumDamageType.Poison => StatBarSegmentKind.Poison,
            EnumDamageType.Gravity => StatBarSegmentKind.Fall,
            EnumDamageType.Suffocation => StatBarSegmentKind.Suffocation,
            EnumDamageType.Crushing => StatBarSegmentKind.Crushing,
            EnumDamageType.Electricity => StatBarSegmentKind.Electricity,
            EnumDamageType.Acid => StatBarSegmentKind.Acid,
            EnumDamageType.Hunger => StatBarSegmentKind.Hunger,
            _ when damageSource.Source == EnumDamageSource.Fall => StatBarSegmentKind.Fall,
            _ when damageSource.Source == EnumDamageSource.Drown => StatBarSegmentKind.Suffocation,
            _ => StatBarSegmentKind.Damage,
        };
    }
}

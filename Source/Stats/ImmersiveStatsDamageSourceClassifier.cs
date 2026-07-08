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
            EnumDamageType.Hunger => StatBarSegmentKind.Hunger,
            _ => StatBarSegmentKind.Damage,
        };
    }
}

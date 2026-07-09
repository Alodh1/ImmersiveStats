using ImmersiveStats.Stats;
using Vintagestory.API.Common;
using Vintagestory.API.Common.Entities;
using Vintagestory.API.Datastructures;

namespace ImmersiveStats.Server;

internal sealed class ImmersiveStatsDamageTrackerBehavior : EntityBehavior
{
    private const float SyncIntervalSeconds = 0.25f;
    private const float SnapshotEpsilon = 0.001f;
    private const float BleedingTriggerHealthDamage = 2f;
    private const float BleedingPoolStartHealthDamage = 2.5f;
    private const float BleedingDurationSeconds = 120f;
    private const float FractureTriggerHealthDamage = 3f;
    private const float FractureDurationSeconds = 240f;
    private const float BurnTriggerHealthDamage = 1f;
    private const float BurnDurationSeconds = 60f;
    private const float FrostbiteThresholdCelsius = 35f;
    private const float HypothermiaThresholdCelsius = 33f;
    private const float FrostbiteEnergyPerDegreeSecond = 2f;
    private const float HypothermiaEnergyPerDegreeSecond = 3f;

    private readonly ImmersiveStatsDamageBuckets _buckets = new();
    private readonly ImmersiveStatsTimedEnergyCondition _bleeding = new();
    private readonly ImmersiveStatsTimedEnergyCondition _fracture = new();
    private readonly ImmersiveStatsTimedEnergyCondition _thermalBurn = new();
    private readonly ImmersiveStatsThermalExposureCondition _frostbite = new(FrostbiteThresholdCelsius, FrostbiteEnergyPerDegreeSecond);
    private readonly ImmersiveStatsThermalExposureCondition _hypothermia = new(HypothermiaThresholdCelsius, HypothermiaEnergyPerDegreeSecond);
    private readonly ImmersiveStatsServerVitalsTracker _tracker;

    private float _syncTimer;
    private bool _hasLastSnapshot;
    private ImmersiveStatsVitalsSnapshot _lastSnapshot;

    public ImmersiveStatsDamageTrackerBehavior(Entity entity, ImmersiveStatsServerVitalsTracker tracker)
        : base(entity)
    {
        _tracker = tracker;
    }

    public override string PropertyName() => "immersivestatsdamage";

    public override void OnGameTick(float deltaTime)
    {
        EnsureTargetHungerCapacity();
        TickTimedConditions(deltaTime);
        TickThermalConditions(deltaTime);

        _syncTimer += deltaTime;
        if (_syncTimer < SyncIntervalSeconds)
        {
            return;
        }

        _syncTimer = 0;
        Sync(force: false);
    }

    public override void OnEntityReceiveDamage(DamageSource damageSource, ref float damage)
    {
        if (damageSource.Duration > TimeSpan.Zero)
        {
            return;
        }

        if (damageSource.Type == EnumDamageType.Heal)
        {
            Sync(force: true);
            return;
        }

        if (!entity.Alive || damage <= 0)
        {
            return;
        }

        if (!ImmersiveStatsDamageSourceClassifier.TryClassifyImmediate(damageSource, out StatBarSegmentKind kind))
        {
            return;
        }

        _buckets.Add(kind, ImmersiveStatsVitalsMapper.HealthDamageToEnergy(damage));
        TriggerChildCondition(kind, damageSource, damage);
        Sync(force: true);
    }

    public override void OnEntityDeath(DamageSource damageSourceForDeath)
    {
        ClearState();
        Sync(force: true);
    }

    public override void OnEntityRevive()
    {
        ClearState();
        EnsureTargetHungerCapacity();
        Sync(force: true);
    }

    public void SyncNow()
    {
        EnsureTargetHungerCapacity();
        Sync(force: true);
    }

    private void TriggerChildCondition(StatBarSegmentKind kind, DamageSource damageSource, float healthDamage)
    {
        if (kind == StatBarSegmentKind.PenetratingTrauma && healthDamage > BleedingTriggerHealthDamage)
        {
            float pool = ImmersiveStatsVitalsMapper.HealthDamageToEnergy(Math.Max(0, healthDamage - BleedingPoolStartHealthDamage));
            _bleeding.Trigger(pool, BleedingDurationSeconds);
            return;
        }

        if (kind == StatBarSegmentKind.BluntTrauma && healthDamage > FractureTriggerHealthDamage)
        {
            float pool = ImmersiveStatsVitalsMapper.HealthDamageToEnergy(Math.Max(0, healthDamage - FractureTriggerHealthDamage));
            _fracture.Trigger(pool, FractureDurationSeconds);
            return;
        }

        if (kind == StatBarSegmentKind.Burn && damageSource.Type == EnumDamageType.Fire && healthDamage > BurnTriggerHealthDamage)
        {
            float pool = ImmersiveStatsVitalsMapper.HealthDamageToEnergy(Math.Max(0, healthDamage - BurnTriggerHealthDamage));
            _thermalBurn.Trigger(pool, BurnDurationSeconds);
        }
    }

    private void TickTimedConditions(float deltaTime)
    {
        _buckets.Add(StatBarSegmentKind.PenetratingTrauma, _bleeding.Tick(deltaTime));
        _buckets.Add(StatBarSegmentKind.BluntTrauma, _fracture.Tick(deltaTime));
        _buckets.Add(StatBarSegmentKind.Burn, _thermalBurn.Tick(deltaTime));
    }

    private void TickThermalConditions(float deltaTime)
    {
        ITreeAttribute? bodyTemp = entity.WatchedAttributes.GetTreeAttribute("bodyTemp");
        float currentBodyTemperature = ReadFloat(bodyTemp, "bodytemp");
        if (!IsFinite(currentBodyTemperature))
        {
            return;
        }

        _buckets.AddDelta(StatBarSegmentKind.Burn, _frostbite.Update(currentBodyTemperature, deltaTime));
        _buckets.AddDelta(StatBarSegmentKind.CoreTemperature, _hypothermia.Update(currentBodyTemperature, deltaTime));
    }

    private void EnsureTargetHungerCapacity()
    {
        ITreeAttribute? hunger = entity.WatchedAttributes.GetTreeAttribute("hunger");
        ImmersiveStatsHungerCapacity.EnsureTargetCapacity(hunger, () => entity.WatchedAttributes.MarkPathDirty("hunger"));
    }

    private void Sync(bool force)
    {
        ImmersiveStatsVitalsSnapshot snapshot = CreateSnapshot();
        if (!force && _hasLastSnapshot && SnapshotsEqual(_lastSnapshot, snapshot))
        {
            return;
        }

        _hasLastSnapshot = true;
        _lastSnapshot = snapshot;
        _tracker.Send(entity, snapshot);
    }

    private ImmersiveStatsVitalsSnapshot CreateSnapshot()
    {
        ITreeAttribute? health = entity.WatchedAttributes.GetTreeAttribute("health");
        ITreeAttribute? hunger = entity.WatchedAttributes.GetTreeAttribute("hunger");

        return ImmersiveStatsVitalsMapper.CreateCategorizedSnapshot(
            ReadFloat(health, "currenthealth"),
            ReadFloat(health, "maxhealth"),
            ReadFloat(hunger, "currentsaturation"),
            ReadFloat(hunger, "maxsaturation"),
            _buckets.Amounts,
            ActiveConditions());
    }

    private IEnumerable<StatBarSegmentKind> ActiveConditions()
    {
        if (_bleeding.Active)
        {
            yield return StatBarSegmentKind.PenetratingTrauma;
        }

        if (_fracture.Active)
        {
            yield return StatBarSegmentKind.BluntTrauma;
        }

        if (_thermalBurn.Active || _frostbite.Active)
        {
            yield return StatBarSegmentKind.Burn;
        }

        if (_hypothermia.Active)
        {
            yield return StatBarSegmentKind.CoreTemperature;
        }
    }

    private void ClearState()
    {
        _buckets.Clear();
        _bleeding.Clear();
        _fracture.Clear();
        _thermalBurn.Clear();
        _frostbite.Clear();
        _hypothermia.Clear();
    }

    private static bool SnapshotsEqual(ImmersiveStatsVitalsSnapshot left, ImmersiveStatsVitalsSnapshot right)
    {
        return Close(left.CurrentHealth, right.CurrentHealth)
            && Close(left.MaxHealth, right.MaxHealth)
            && Close(left.CurrentSaturation, right.CurrentSaturation)
            && Close(left.MaxSaturation, right.MaxSaturation)
            && Close(left.Capacity, right.Capacity)
            && Close(left.PenetratingTraumaReducer, right.PenetratingTraumaReducer)
            && Close(left.BluntTraumaReducer, right.BluntTraumaReducer)
            && Close(left.BurnReducer, right.BurnReducer)
            && Close(left.CoreTemperatureReducer, right.CoreTemperatureReducer)
            && Close(left.ToxicReducer, right.ToxicReducer)
            && Close(left.AsphyxiationReducer, right.AsphyxiationReducer)
            && Close(left.HungerReducer, right.HungerReducer)
            && left.PenetratingTraumaActive == right.PenetratingTraumaActive
            && left.BluntTraumaActive == right.BluntTraumaActive
            && left.BurnActive == right.BurnActive
            && left.CoreTemperatureActive == right.CoreTemperatureActive;
    }

    private static float ReadFloat(ITreeAttribute? tree, string key)
    {
        return tree?.TryGetFloat(key) ?? float.NaN;
    }

    private static bool IsFinite(float value)
    {
        return !float.IsNaN(value) && !float.IsInfinity(value);
    }

    private static bool Close(float left, float right)
    {
        return Math.Abs(left - right) <= SnapshotEpsilon;
    }
}

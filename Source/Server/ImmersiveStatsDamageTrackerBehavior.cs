using ImmersiveStats.Stats;
using Vintagestory.API.Common;
using Vintagestory.API.Common.Entities;
using Vintagestory.API.Datastructures;

namespace ImmersiveStats.Server;

internal sealed class ImmersiveStatsDamageTrackerBehavior : EntityBehavior
{
    private const float SyncIntervalSeconds = 0.25f;
    private const float SnapshotEpsilon = 0.001f;

    private readonly ImmersiveStatsDamageBuckets _buckets = new();
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
        _syncTimer += deltaTime;
        if (_syncTimer < SyncIntervalSeconds)
        {
            return;
        }

        _syncTimer = 0;
        ReconcileBucketsToHealth();
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
            ReconcileBucketsToHealth();
            Sync(force: true);
            return;
        }

        if (!entity.Alive || damage <= 0)
        {
            return;
        }

        _buckets.Add(ImmersiveStatsDamageSourceClassifier.Classify(damageSource), damage);
        ReconcileBucketsToHealth();
        Sync(force: true);
    }

    public override void OnEntityDeath(DamageSource damageSourceForDeath)
    {
        _buckets.Clear();
        Sync(force: true);
    }

    public override void OnEntityRevive()
    {
        _buckets.Clear();
        Sync(force: true);
    }

    public void SyncNow()
    {
        ReconcileBucketsToHealth();
        Sync(force: true);
    }

    private void ReconcileBucketsToHealth()
    {
        ITreeAttribute? health = entity.WatchedAttributes.GetTreeAttribute("health");
        float currentHealth = ReadFloat(health, "currenthealth");
        float maxHealth = ReadFloat(health, "maxhealth");
        float missingHealth = IsValidMax(maxHealth) && IsFinite(currentHealth)
            ? Math.Max(0, maxHealth - Math.Min(maxHealth, Math.Max(0, currentHealth)))
            : 0;

        _buckets.ReconcileToMissingHealth(missingHealth);
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
            _buckets.Damage,
            _buckets.Cold,
            _buckets.Heat,
            _buckets.Hunger);
    }

    private static bool SnapshotsEqual(ImmersiveStatsVitalsSnapshot left, ImmersiveStatsVitalsSnapshot right)
    {
        return Close(left.CurrentHealth, right.CurrentHealth)
            && Close(left.MaxHealth, right.MaxHealth)
            && Close(left.CurrentSaturation, right.CurrentSaturation)
            && Close(left.MaxSaturation, right.MaxSaturation)
            && Close(left.Capacity, right.Capacity)
            && Close(left.DamageReducer, right.DamageReducer)
            && Close(left.ColdReducer, right.ColdReducer)
            && Close(left.HeatReducer, right.HeatReducer)
            && Close(left.HungerReducer, right.HungerReducer);
    }

    private static float ReadFloat(ITreeAttribute? tree, string key)
    {
        return tree?.TryGetFloat(key) ?? float.NaN;
    }

    private static bool IsValidMax(float value)
    {
        return IsFinite(value) && value > 0;
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

namespace ImmersiveStats;

public sealed class StatBarState : IEquatable<StatBarState>
{
    private readonly Dictionary<StatBarSegmentKind, float> _reducers;
    private readonly HashSet<StatBarSegmentKind> _activeConditions;

    public StatBarState(float capacity, float penetratingTrauma, float bluntTrauma, float burn, float hunger)
        : this(capacity, new Dictionary<StatBarSegmentKind, float>
        {
            [StatBarSegmentKind.PenetratingTrauma] = penetratingTrauma,
            [StatBarSegmentKind.BluntTrauma] = bluntTrauma,
            [StatBarSegmentKind.Burn] = burn,
            [StatBarSegmentKind.Hunger] = hunger,
        })
    {
    }

    public StatBarState(
        float capacity,
        IReadOnlyDictionary<StatBarSegmentKind, float> reducers,
        IEnumerable<StatBarSegmentKind>? activeConditions = null)
    {
        Capacity = capacity;
        _reducers = new Dictionary<StatBarSegmentKind, float>();
        foreach (StatBarSegmentKind kind in StatBarSegmentCatalog.ReducerKinds)
        {
            _reducers[kind] = reducers.TryGetValue(kind, out float value) ? value : 0;
        }

        _activeConditions = new HashSet<StatBarSegmentKind>(
            activeConditions?.Where(StatBarSegmentCatalog.IsReducer) ?? []);
    }

    public float Capacity { get; }

    public float PenetratingTrauma => GetReducer(StatBarSegmentKind.PenetratingTrauma);

    public float BluntTrauma => GetReducer(StatBarSegmentKind.BluntTrauma);

    public float Burn => GetReducer(StatBarSegmentKind.Burn);

    public float CoreTemperature => GetReducer(StatBarSegmentKind.CoreTemperature);

    public float Toxic => GetReducer(StatBarSegmentKind.Toxic);

    public float Asphyxiation => GetReducer(StatBarSegmentKind.Asphyxiation);

    public float Hunger => GetReducer(StatBarSegmentKind.Hunger);

    public IReadOnlyDictionary<StatBarSegmentKind, float> Reducers => _reducers;

    public IReadOnlySet<StatBarSegmentKind> ActiveConditions => _activeConditions;

    public static StatBarState Empty { get; } = new(StatBarLayout.DefaultCapacity, 0, 0, 0, 0);

    public float GetReducer(StatBarSegmentKind kind)
    {
        return _reducers.TryGetValue(kind, out float amount) ? amount : 0;
    }

    public bool IsConditionActive(StatBarSegmentKind kind)
    {
        return _activeConditions.Contains(kind);
    }

    public bool HasActiveConditions => _activeConditions.Count > 0;

    public bool Equals(StatBarState? other)
    {
        if (other is null || Capacity != other.Capacity)
        {
            return false;
        }

        foreach (StatBarSegmentKind kind in StatBarSegmentCatalog.ReducerKinds)
        {
            if (GetReducer(kind) != other.GetReducer(kind)
                || IsConditionActive(kind) != other.IsConditionActive(kind))
            {
                return false;
            }
        }

        return true;
    }

    public override bool Equals(object? obj)
    {
        return obj is StatBarState other && Equals(other);
    }

    public override int GetHashCode()
    {
        HashCode hash = new();
        hash.Add(Capacity);
        foreach (StatBarSegmentKind kind in StatBarSegmentCatalog.ReducerKinds)
        {
            hash.Add(GetReducer(kind));
            hash.Add(IsConditionActive(kind));
        }

        return hash.ToHashCode();
    }
}

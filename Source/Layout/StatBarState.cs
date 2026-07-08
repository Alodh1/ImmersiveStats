namespace ImmersiveStats;

public sealed class StatBarState : IEquatable<StatBarState>
{
    private readonly Dictionary<StatBarSegmentKind, float> _reducers;

    public StatBarState(float capacity, float damage, float cold, float heat, float hunger)
        : this(capacity, new Dictionary<StatBarSegmentKind, float>
        {
            [StatBarSegmentKind.Damage] = damage,
            [StatBarSegmentKind.Cold] = cold,
            [StatBarSegmentKind.Heat] = heat,
            [StatBarSegmentKind.Hunger] = hunger,
        })
    {
    }

    public StatBarState(float capacity, IReadOnlyDictionary<StatBarSegmentKind, float> reducers)
    {
        Capacity = capacity;
        _reducers = new Dictionary<StatBarSegmentKind, float>();
        foreach (StatBarSegmentKind kind in StatBarSegmentCatalog.ReducerKinds)
        {
            _reducers[kind] = reducers.TryGetValue(kind, out float amount) ? amount : 0;
        }
    }

    public float Capacity { get; }

    public float Damage => GetReducer(StatBarSegmentKind.Damage);

    public float Cold => GetReducer(StatBarSegmentKind.Cold);

    public float Heat => GetReducer(StatBarSegmentKind.Heat);

    public float Poison => GetReducer(StatBarSegmentKind.Poison);

    public float Fall => GetReducer(StatBarSegmentKind.Fall);

    public float Suffocation => GetReducer(StatBarSegmentKind.Suffocation);

    public float Crushing => GetReducer(StatBarSegmentKind.Crushing);

    public float Electricity => GetReducer(StatBarSegmentKind.Electricity);

    public float Acid => GetReducer(StatBarSegmentKind.Acid);

    public float Hunger => GetReducer(StatBarSegmentKind.Hunger);

    public IReadOnlyDictionary<StatBarSegmentKind, float> Reducers => _reducers;

    public static StatBarState Empty { get; } = new(StatBarLayout.DefaultCapacity, 0, 0, 0, 0);

    public float GetReducer(StatBarSegmentKind kind)
    {
        return _reducers.TryGetValue(kind, out float amount) ? amount : 0;
    }

    public bool Equals(StatBarState? other)
    {
        if (other is null || Capacity != other.Capacity)
        {
            return false;
        }

        foreach (StatBarSegmentKind kind in StatBarSegmentCatalog.ReducerKinds)
        {
            if (GetReducer(kind) != other.GetReducer(kind))
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
            hash.Add(kind);
            hash.Add(GetReducer(kind));
        }

        return hash.ToHashCode();
    }

    public static bool operator ==(StatBarState? left, StatBarState? right)
    {
        return EqualityComparer<StatBarState>.Default.Equals(left, right);
    }

    public static bool operator !=(StatBarState? left, StatBarState? right)
    {
        return !(left == right);
    }
}

namespace ImmersiveStats.Debug;

/// <summary>
/// Holds the client-side debug values used by the prototype HUD.
/// </summary>
internal sealed class DebugStatBarState
{
    private StatBarState _current = new(StatBarLayout.DefaultCapacity, Damage: 18, Cold: 9, Heat: 6, Hunger: 22);

    public event Action? Changed;

    public StatBarState Current
    {
        get => _current;
        set
        {
            if (_current == value)
            {
                return;
            }

            _current = value;
            Changed?.Invoke();
        }
    }
}

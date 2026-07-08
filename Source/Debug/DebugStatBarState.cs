namespace ImmersiveStats.Debug;

/// <summary>
/// Holds the client-side debug values used by the prototype HUD.
/// </summary>
internal sealed class DebugStatBarState
{
    private StatBarState _current = new(StatBarLayout.DefaultCapacity, 18, 9, 6, 22);

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

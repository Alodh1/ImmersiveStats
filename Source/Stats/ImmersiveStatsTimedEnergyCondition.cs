namespace ImmersiveStats.Stats;

internal sealed class ImmersiveStatsTimedEnergyCondition
{
    private const float Epsilon = 0.0001f;
    private float _remainingEnergy;
    private float _remainingSeconds;
    private float _tickAccumulator;

    public float RemainingEnergy => _remainingEnergy;

    public float RemainingSeconds => _remainingSeconds;

    public bool Active => _remainingSeconds > Epsilon;

    public void Trigger(float energyPool, float durationSeconds)
    {
        if (!IsFinite(durationSeconds) || durationSeconds <= 0)
        {
            return;
        }

        _remainingEnergy = Math.Max(0, _remainingEnergy) + SanitizeAmount(energyPool);
        _remainingSeconds = durationSeconds;
        _tickAccumulator = 0;
    }

    public float Tick(float deltaTime)
    {
        if (!Active || !IsFinite(deltaTime) || deltaTime <= 0)
        {
            return 0;
        }

        _tickAccumulator += deltaTime;
        int wholeSeconds = (int)Math.Floor(_tickAccumulator);
        if (wholeSeconds <= 0)
        {
            return 0;
        }

        _tickAccumulator -= wholeSeconds;
        float tickSeconds = Math.Min(wholeSeconds, _remainingSeconds);
        if (tickSeconds <= 0)
        {
            Clear();
            return 0;
        }

        float appliedEnergy = _remainingSeconds > Epsilon
            ? _remainingEnergy * tickSeconds / _remainingSeconds
            : _remainingEnergy;

        _remainingEnergy = Math.Max(0, _remainingEnergy - appliedEnergy);
        _remainingSeconds = Math.Max(0, _remainingSeconds - tickSeconds);

        if (_remainingSeconds <= Epsilon)
        {
            _remainingEnergy = 0;
            _remainingSeconds = 0;
            _tickAccumulator = 0;
        }

        return appliedEnergy;
    }

    public void Clear()
    {
        _remainingEnergy = 0;
        _remainingSeconds = 0;
        _tickAccumulator = 0;
    }

    private static float SanitizeAmount(float value)
    {
        return IsFinite(value) ? Math.Max(0, value) : 0;
    }

    private static bool IsFinite(float value)
    {
        return !float.IsNaN(value) && !float.IsInfinity(value);
    }
}

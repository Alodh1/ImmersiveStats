namespace ImmersiveStats.Stats;

internal sealed class ImmersiveStatsThermalExposureCondition
{
    private const float SafeDelaySeconds = 10f;
    private const float RecoverySeconds = 120f;
    private const float Epsilon = 0.0001f;

    private readonly float _thresholdCelsius;
    private readonly float _energyPerDegreeSecond;
    private float _amount;
    private float _safeSeconds;
    private float _recoveryRate;

    public ImmersiveStatsThermalExposureCondition(float thresholdCelsius, float energyPerDegreeSecond)
    {
        _thresholdCelsius = thresholdCelsius;
        _energyPerDegreeSecond = energyPerDegreeSecond;
    }

    public float Amount => _amount;

    public bool Active => _amount > Epsilon;

    public float Update(float bodyTemperature, float deltaTime)
    {
        if (!IsFinite(bodyTemperature) || !IsFinite(deltaTime) || deltaTime <= 0)
        {
            return 0;
        }

        float previous = _amount;
        if (bodyTemperature < _thresholdCelsius)
        {
            _safeSeconds = 0;
            _recoveryRate = 0;
            _amount += (_thresholdCelsius - bodyTemperature) * _energyPerDegreeSecond * deltaTime;
        }
        else if (_amount > Epsilon)
        {
            _safeSeconds += deltaTime;
            if (_safeSeconds >= SafeDelaySeconds)
            {
                if (_recoveryRate <= Epsilon)
                {
                    _recoveryRate = _amount / RecoverySeconds;
                }

                _amount = Math.Max(0, _amount - _recoveryRate * deltaTime);
            }
        }
        else
        {
            Clear();
        }

        if (_amount <= Epsilon)
        {
            _amount = 0;
            _recoveryRate = 0;
        }

        return _amount - previous;
    }

    public void Clear()
    {
        _amount = 0;
        _safeSeconds = 0;
        _recoveryRate = 0;
    }

    private static bool IsFinite(float value)
    {
        return !float.IsNaN(value) && !float.IsInfinity(value);
    }
}

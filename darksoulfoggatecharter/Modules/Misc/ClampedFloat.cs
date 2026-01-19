using System;

public class ClampedFloat
{
    private float min, max, value;

    public float Min => min;
    public float Max => max;
    public float Value => value;
    public float Percentage => GetPercentage();
    public bool IsAtMin { get; private set; }
    public bool IsAtMax { get; private set; }

    public Action OnValueChanged, OnMin, OnMax;

    public ClampedFloat(float min, float max, float value)
    {
        this.min = min;
        this.max = max;
        this.value = value;
    }

    public void SetValue(float value)
    {
        this.value = Math.Clamp(value, min, max);

        if (this.value == min && !IsAtMin)
        {
            IsAtMin = true;
            OnMin?.Invoke();
        }
        else if (this.value == max && !IsAtMax)
        {
            IsAtMax = true;
            OnMax?.Invoke();
        }

        OnValueChanged?.Invoke();
    }

    public void AdjustValue(float adjust)
    {
        SetValue(value + adjust);
    }

    public void SetValueToMin() => SetValue(min);
    public void SetValueToMax() => SetValue(max);

    private float GetPercentage()
    {
        if (max <= 0) return 0;
        return (value - min) / (max - min);
    }
}

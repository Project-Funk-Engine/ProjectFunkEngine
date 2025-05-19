using System;
using System.Linq;
using Godot;

/**
 * <summary>Generic health bar, instantiate from scene.</summary>
 */
public partial class HealthBar : TextureProgressBar
{
    public override void _Ready()
    {
        Value = MaxValue;
        UpdateBarColor();
    }

    //initializes health
    public void SetHealth(int max, int current)
    {
        MaxValue = max;
        Value = current;
        UpdateBarColor();
    }

    //For effects changes max hp, and changes hp by a similar amount
    public void ChangeMax(int change)
    {
        MaxValue += change;
        Value += change;
        UpdateBarColor();
    }

    //Changes hp value, for damage or heal, returns resulting hp.
    public int ChangeHP(int amount)
    {
        Value += amount;
        UpdateBarColor();
        return (int)Value;
    }

    private void UpdateBarColor()
    {
        if (MaxValue == 0)
            return;
        float healthRatio = Mathf.Clamp((float)Value / (float)MaxValue, 0f, 1f);
        float nearestStep = (healthRatio - healthRatio % 0.20f);
        if (
            TextureProgress is GradientTexture2D gradientTexture
            && gradientTexture.Gradient.GetPointCount() > 0
        )
        {
            Color lerpColor = Colors.Red.Lerp(Colors.Green, nearestStep);
            gradientTexture.Gradient.SetColor(0, lerpColor);
        }
    }
}

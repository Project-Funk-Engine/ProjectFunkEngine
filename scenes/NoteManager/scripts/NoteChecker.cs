using System;
using Godot;

/**
 * <summary>Visual input handlers, glow and emit particles on note hits/player input.</summary>
 */
public partial class NoteChecker : Sprite2D
{
    private bool _isPressed;
    private Color _color;
    private float _fadeTime = 2.0f;
    public HitParticles Particles;

    public override void _Process(double delta)
    {
        SelfModulate = _isPressed
            ? Modulate.Lerp(_color, _fadeTime)
            : SelfModulate.Lerp(
                new Color(_color.R * 0.5f, _color.G * 0.5f, _color.B * 0.5f, 1),
                (float)delta * _fadeTime
            );
    }

    public void SetPressed(bool pressed)
    {
        _isPressed = pressed;
    }

    public void SetColor(Color color)
    {
        _color = color;
        Particles = GetNode<HitParticles>("HitParticles");
        Particles.Modulate = color;
        SelfModulate = new Color(_color.R * 0.5f, _color.G * 0.5f, _color.B * 0.5f, 1);
    }
}

using System;
using Godot;

public partial class GlitchScript : Node
{
    private ShaderMaterial _glitchMaterial;
    private Timer _glitchTimer;

    [Export]
    public Sprite2D Sprite;

    public override void _Ready()
    {
        var shader = GD.Load<Shader>(
            "res://Scenes/Puppets/Enemies/CyberFox/Assets/GlitchEffect.gdshader"
        );
        _glitchMaterial = new ShaderMaterial { Shader = shader };
        Sprite.Material = _glitchMaterial;

        DisableGlitch();
        _glitchTimer = new Timer { OneShot = true, Autostart = false };
        AddChild(_glitchTimer);
        _glitchTimer.Timeout += OnGlitchTimerTimeout;
    }

    public void TriggerGlitch(float duration)
    {
        EnableGlitch();
        _glitchTimer.WaitTime = duration;
        _glitchTimer.Start();
    }

    private void OnGlitchTimerTimeout()
    {
        DisableGlitch();
    }

    private void EnableGlitch()
    {
        _glitchMaterial.SetShaderParameter("shake_rate", 0.8f);
    }

    private void DisableGlitch()
    {
        _glitchMaterial.SetShaderParameter("shake_rate", 0.0f);
    }
}

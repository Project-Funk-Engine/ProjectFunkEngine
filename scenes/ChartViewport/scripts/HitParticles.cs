using System;
using Godot;

/**
 * <summary>Particles on note checker from successful note hits.</summary>
 */
public partial class HitParticles : CpuParticles2D
{
    public void Emit(int particleAmount)
    {
        // Apply the particle amount and start emitting
        Amount = particleAmount;
        Emitting = true;

        // Stop particles after a short delay using a Timer
        Timer timer = new Timer();
        timer.WaitTime = 0.25f;
        timer.OneShot = true;
        timer.Timeout += () =>
        {
            Emitting = false;
            timer.QueueFree();
        };

        AddChild(timer);
        timer.Start();
    }
}

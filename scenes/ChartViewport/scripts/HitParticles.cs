using System;
using Godot;

public partial class HitParticles : CpuParticles2D
{
    public void Emit(int particleAmount)
    {
        // Apply the particle amount and start emitting
        Amount = particleAmount;
        Emitting = true;

        // Stop particles after a short delay using a Timer
        Timer timer = new Timer();
        timer.WaitTime = 0.25f; // Stop emitting after 0.5 seconds
        timer.OneShot = true;
        timer.Timeout += () =>
        {
            Emitting = false;
            timer.QueueFree(); // Clean up the timer
        };

        AddChild(timer);
        timer.Start();
    }
}

using System;
using Godot;

/**<summary>Particle effects on placing a note.</summary>
 */
public static class NoteQueueParticlesFactory
{
    private static PackedScene particlesScene = null;

    public static GpuParticles2D NoteParticles(
        Node parent,
        Texture2D texture,
        float amountModifier = 1
    )
    {
        particlesScene ??= GD.Load<PackedScene>(
            "res://Scenes/BattleDirector/NotePoofParticles.tscn"
        );

        GpuParticles2D particles = particlesScene.Instantiate<GpuParticles2D>();

        parent.AddChild(particles);
        particles.Amount = (int)(particles.Amount * amountModifier);
        particles.Texture = texture;
        particles.Emitting = true;
        particles.Finished += () => particles.QueueFree();

        return particles;
    }
}

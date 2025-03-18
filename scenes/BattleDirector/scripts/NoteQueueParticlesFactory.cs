using System;
using Godot;

/**<summary>Particle effects on placing a note.</summary>
 */
public static class NoteQueueParticlesFactory
{
    private static readonly PackedScene ParticlesScene = GD.Load<PackedScene>(
        "res://Scenes/BattleDirector/NotePoofParticles.tscn"
    );

    public static GpuParticles2D NoteParticles(
        Node parent,
        Texture2D texture,
        float amountModifier = 1
    )
    {
        GpuParticles2D particles = ParticlesScene.Instantiate<GpuParticles2D>();

        parent.AddChild(particles);
        particles.Amount = (int)(particles.Amount * amountModifier);
        particles.Texture = texture;
        particles.Emitting = true;
        particles.Finished += () => particles.QueueFree();

        return particles;
    }
}

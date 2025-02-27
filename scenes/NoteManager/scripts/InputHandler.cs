using System;
using System.Collections.Generic;
using FunkEngine;
using Godot;

/**
 * @class InputHandler
 * @brief InputHandler to handle input, and manage note checkers. WIP
 */
public partial class InputHandler : Node2D
{
    [Signal]
    public delegate void NotePressedEventHandler(ArrowType arrowType);

    [Signal]
    public delegate void NoteReleasedEventHandler(ArrowType arrowType);

    // Dictionary to store Particles2D nodes for each arrow
    private Dictionary<ArrowType, CpuParticles2D> hitParticles =
        new Dictionary<ArrowType, CpuParticles2D>();

    public ArrowData[] Arrows = new ArrowData[]
    {
        new ArrowData()
        {
            Color = Colors.Green,
            Key = "arrowUp",
            Type = ArrowType.Up,
        },
        new ArrowData()
        {
            Color = Colors.Aqua,
            Key = "arrowDown",
            Type = ArrowType.Down,
        },
        new ArrowData()
        {
            Color = Colors.HotPink,
            Key = "arrowLeft",
            Type = ArrowType.Left,
        },
        new ArrowData()
        {
            Color = Colors.Red,
            Key = "arrowRight",
            Type = ArrowType.Right,
        },
    };

    private void InitializeArrowCheckers()
    {
        //Set the color of the arrows
        for (int i = 0; i < Arrows.Length; i++)
        {
            Arrows[i].Node = GetNode<NoteChecker>("noteCheckers/" + Arrows[i].Key);
            Arrows[i].Node.SetColor(Arrows[i].Color);

            var particles = Arrows[i].Node.GetNode<CpuParticles2D>("HitParticles");
            particles.Emitting = false;
            hitParticles[Arrows[i].Type] = particles;
        }
    }

    public void FeedbackEffect(ArrowType arrow, string text)
    {
        if (hitParticles.ContainsKey(arrow))
        {
            // Get the particle node for this arrow
            var particles = hitParticles[arrow];

            // Set the particle amount based on timing
            int particleAmount;

            if (text == "Perfect")
            {
                particleAmount = 15; // A lot of particles for Perfect
            }
            else if (text == "Great")
            {
                particleAmount = 10; // Moderate amount for Great
            }
            else if (text == "Good")
            {
                particleAmount = 5; // Few particles for Good
            }
            else
            {
                return; // No particles for a miss
            }

            // Apply the particle amount and start emitting
            particles.Amount = particleAmount;
            particles.Emitting = true;

            // Stop particles after a short delay using a Timer
            Timer timer = new Timer();
            timer.WaitTime = 0.5f; // Stop emitting after 0.5 seconds
            timer.OneShot = true;
            timer.Timeout += () =>
            {
                particles.Emitting = false;
                timer.QueueFree(); // Clean up the timer
            };

            AddChild(timer);
            timer.Start();
        }
    }

    public override void _Ready()
    {
        InitializeArrowCheckers();
    }

    public override void _Process(double delta)
    {
        foreach (var arrow in Arrows)
        {
            if (Input.IsActionJustPressed(arrow.Key))
            {
                EmitSignal(nameof(NotePressed), (int)arrow.Type);
                arrow.Node.SetPressed(true);
            }
            else if (Input.IsActionJustReleased(arrow.Key))
            {
                EmitSignal(nameof(NoteReleased), (int)arrow.Type);
                arrow.Node.SetPressed(false);
            }
        }
    }
}

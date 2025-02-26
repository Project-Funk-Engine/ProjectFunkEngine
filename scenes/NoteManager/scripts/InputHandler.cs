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

    private Dictionary<ArrowType, Color> originalColors = new Dictionary<ArrowType, Color>();
    private Dictionary<ArrowType, Tween> colorTweens = new Dictionary<ArrowType, Tween>();

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

            originalColors[Arrows[i].Type] = Arrows[i].Color;
            GD.Print($"Initialized Arrow: {Arrows[i].Type}, Node: {Arrows[i].Node.Name}");
        }
    }

    public void FeedbackColor(ArrowType arrow, string text)
    {
        GD.Print($"FeedbackColor called for {arrow} with timing: {text}");

        foreach (var arrowData in Arrows)
        {
            if (arrow == arrowData.Type)
            {
                Color feedbackColor;
                float duration = 0.25f; // Short, snappy feedback duration

                // Determine vibrant feedback color based on timing
                if (text == "Perfect")
                {
                    feedbackColor = new Color(1.0f, 0.85f, 0.2f, 1.0f); // Vibrant Gold/Yellow
                }
                else if (text == "Good")
                {
                    feedbackColor = new Color(0.0f, 1.0f, 1.0f, 1.0f); // Bright Cyan
                }
                else if (text == "Okay")
                {
                    feedbackColor = new Color(1.0f, 0.0f, 1.0f, 0.9f); // Vibrant Magenta
                }
                else
                {
                    feedbackColor = arrowData.Color; // Default to original color if no match
                }

                // Apply the feedback color instantly
                arrowData.Node.SetColor(feedbackColor);
                GD.Print($"Setting {arrowData.Type} to {feedbackColor}");

                // Create a Tween and reset to original color smoothly
                var tween = CreateTween();
                tween.SetTrans(Tween.TransitionType.Sine);
                tween.SetEase(Tween.EaseType.Out);

                tween.TweenProperty(arrowData.Node, "modulate", originalColors[arrow], duration);
            }
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

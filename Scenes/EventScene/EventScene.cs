using System;
using FunkEngine;
using Godot;

public partial class EventScene : Node
{
    public static readonly string LoadPath = "res://Scenes/ChestScene/ChestScene.tscn";
    private PlayerPuppet _player;

    [Export]
    public Marker2D PlayerMarker;

    [Export]
    public Sprite2D EventSprite;

    [Export]
    private Label _eventDescription;

    [Export]
    private VBoxContainer _buttonContainer;

    private static Theme _buttonTheme = GD.Load<Theme>("res://Scenes/UI/Assets/GeneralTheme.tres"); // Store the theme

    public static EventScene NewEventScene(int eventId = 0)
    {
        EventScene result = GD.Load<PackedScene>(LoadPath).Instantiate<EventScene>();
        result._player = GD.Load<PackedScene>(PlayerPuppet.LoadPath).Instantiate<PlayerPuppet>();
        result.PlayerMarker.AddChild(result._player);
        result.DisplayEvent(eventId);
        return result;
    }

    //Eventually remove this, once integrated into game
    public override void _Ready()
    {
        _player = GD.Load<PackedScene>(PlayerPuppet.LoadPath).Instantiate<PlayerPuppet>();
        PlayerMarker.AddChild(_player);

        DisplayEvent(0);
    }

    public override void _Process(double delta)
    {
        if (GetViewport().GuiGetFocusOwner() == null && _buttonContainer != null)
        {
            _buttonContainer.GetChild<Control>(0).GrabFocus();
        }
    }

    /// <summary>
    /// Displays the event with the given index.
    /// </summary>
    /// <param name="eventIndex">Index of the event in EventDatabase.</param>
    public void DisplayEvent(int eventIndex)
    {
        var eventTemplate = EventDatabase.EventDictionary[eventIndex];

        _eventDescription.Text = eventTemplate.EventDescription;
        EventSprite.Texture = eventTemplate.Texture;

        for (int i = 0; i < eventTemplate.ButtonDescriptions.Length; i++)
        {
            var button = new Button
            {
                Text = eventTemplate.ButtonDescriptions[i],
                Theme = _buttonTheme,
            };

            button.SizeFlagsVertical = Control.SizeFlags.Expand | Control.SizeFlags.Fill;

            int capturedIndex = i; // was given a warning to capture the index
            button.Pressed += () =>
            {
                GD.Print($"Selected option: {eventTemplate.ButtonDescriptions[capturedIndex]}");
                eventTemplate.OptionActions[capturedIndex]?.Invoke();
                StageProducer.LiveInstance.TransitionStage(Stages.Map);
            };
            if (capturedIndex == 0)
            {
                button.GrabFocus();
            }
            _buttonContainer.AddChild(button);
        }
    }
}

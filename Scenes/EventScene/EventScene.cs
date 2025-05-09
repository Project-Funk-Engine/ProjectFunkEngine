using System;
using FunkEngine;
using Godot;

public partial class EventScene : Node
{
    public static readonly string LoadPath = "res://Scenes/EventScene/EventScene.tscn";
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

    private EventTemplate _eventReference;

    public static EventScene NewEventScene()
    {
        EventScene result = GD.Load<PackedScene>(LoadPath).Instantiate<EventScene>();
        result._player = GD.Load<PackedScene>(PlayerPuppet.LoadPath).Instantiate<PlayerPuppet>();
        result.PlayerMarker.AddChild(result._player);
        result.DisplayEvent();
        return result;
    }

    //Eventually remove this, once integrated into game
    public override void _Ready()
    {
        GD.Print("loaded event");
        _player = GD.Load<PackedScene>(PlayerPuppet.LoadPath).Instantiate<PlayerPuppet>();
        PlayerMarker.AddChild(_player);

        //int eventIndex = StageProducer.GlobalRng.RandiRange(0, EventDatabase.EventDictionary.Length);
        _eventReference = EventDatabase.EventDictionary[0];
        DisplayEvent();
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
    public void DisplayEvent()
    {
        _eventDescription.Text = _eventReference.EventDescription;
        EventSprite.Texture = _eventReference.Texture;

        for (int i = 0; i < _eventReference.ButtonDescriptions.Length; i++)
        {
            var button = new Button
            {
                Text = _eventReference.ButtonDescriptions[i],
                Theme = _buttonTheme,
                SizeFlagsVertical = Control.SizeFlags.Expand | Control.SizeFlags.Fill,
            };

            // Check if the button should be enabled
            bool isEnabled =
                _eventReference.OptionEnabledConditions == null
                || _eventReference.OptionEnabledConditions.Length <= i
                || _eventReference.OptionEnabledConditions[i]?.Invoke() != false;

            button.Disabled = !isEnabled;

            int capturedIndex = i;
            button.Pressed += () =>
            {
                GD.Print($"Selected option: {_eventReference.ButtonDescriptions[capturedIndex]}");
                _eventReference.OptionActions[capturedIndex]?.Invoke();
                AnyButtonPressed(capturedIndex);
            };

            // Automatically focus the first *enabled* button
            if (capturedIndex == 0 && isEnabled)
            {
                button.CallDeferred("grab_focus");
            }

            _buttonContainer.AddChild(button);
        }
    }

    private void AnyButtonPressed(int capturedIndex)
    {
        foreach (var choices in _buttonContainer.GetChildren())
        {
            if (choices is not Button)
                continue;
            choices.QueueFree();
        }

        _eventDescription.Text = _eventReference.OutcomeDescriptions[capturedIndex];
        var button = new Button { Text = "EVENT_CONTINUE_BUTTON", Theme = _buttonTheme };

        button.Pressed += ContinuePressed;

        button.SizeFlagsVertical = Control.SizeFlags.Expand | Control.SizeFlags.Fill;
        button.CallDeferred("grab_focus");
        _buttonContainer.AddChild(button);
    }

    private void ContinuePressed()
    {
        StageProducer.LiveInstance.TransitionStage(Stages.Map);
    }
}

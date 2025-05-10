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

    [Export]
    private Button _continueButton;

    [Export]
    private MarginContainer _continueContainer;

    private static readonly Theme ButtonTheme = GD.Load<Theme>(
        "res://Scenes/UI/Assets/GeneralTheme.tres"
    ); // Store the theme

    private EventTemplate _eventReference;
    private int _updateOutputIndex;

    public override void _Ready()
    {
        GD.Print("loaded event");
        _player = GD.Load<PackedScene>(PlayerPuppet.LoadPath).Instantiate<PlayerPuppet>();
        PlayerMarker.AddChild(_player);

        int eventIndex = StageProducer.GlobalRng.RandiRange(
            0,
            EventDatabase.EventDictionary.Length - 1
        );
        _eventReference = EventDatabase.EventDictionary[eventIndex];
        DisplayEvent();
    }

    public override void _Process(double delta)
    {
        if (GetViewport().GuiGetFocusOwner() == null && _buttonContainer != null)
        {
            _buttonContainer.GetChild<Control>(0).GrabFocus();
        }

        if (_eventDescription.Text == "")
        {
            _eventDescription.Text = _eventReference.OutcomeDescriptions[_updateOutputIndex];
        }
    }

    /// <summary>
    /// Displays the set event.
    /// </summary>
    public void DisplayEvent()
    {
        _eventDescription.Text = _eventReference.EventDescription;
        EventSprite.Texture = _eventReference.Texture;

        for (int i = 0; i < _eventReference.ButtonDescriptions.Length; i++)
        {
            var button = new Button
            {
                Text = _eventReference.ButtonDescriptions[i],
                Theme = ButtonTheme,
                SizeFlagsVertical = Control.SizeFlags.Expand | Control.SizeFlags.Fill,
            };

            bool isEnabled;
            if (
                _eventReference.OptionEnabledConditions == null
                || _eventReference.OptionEnabledConditions.Length <= i
            )
            {
                GD.PushWarning("Event Conditions are invalid for event: " + _eventReference.Id);
                isEnabled = true;
            }
            else
            { // Check if the button should be enabled
                isEnabled =
                    _eventReference.OptionEnabledConditions[i] == null
                    || _eventReference.OptionEnabledConditions[i].Invoke();
            }

            button.Disabled = !isEnabled;

            int capturedIndex = i;
            button.Pressed += () =>
            {
                GD.Print($"Selected option: {_eventReference.ButtonDescriptions[capturedIndex]}");
                _eventReference.OptionActions[capturedIndex]?.Invoke(_eventReference, this);
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

    public void AnyButtonPressed(int capturedIndex)
    {
        _updateOutputIndex = capturedIndex;
        _buttonContainer.GetParent<Control>().Visible = false;
        _continueContainer.Visible = _eventReference.OutcomeDescriptions[capturedIndex] != "";
        _eventDescription.Text = _eventReference.OutcomeDescriptions[capturedIndex];
        _continueButton.GrabFocus();
    }
}

using System;
using System.Collections.Generic;
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

    public static List<int> EventPool;

    private static readonly Theme ButtonTheme = GD.Load<Theme>(
        "res://Scenes/UI/Assets/GeneralTheme.tres"
    ); // Store the theme

    private EventTemplate _eventReference;
    private int _updateOutputIndex;

    public override void _Ready()
    {
        _player = GD.Load<PackedScene>(PlayerPuppet.LoadPath).Instantiate<PlayerPuppet>();
        PlayerMarker.AddChild(_player);

        if (EventPool == null || EventPool.Count == 0)
            RefreshPool();

        RandomNumberGenerator stageRng = new RandomNumberGenerator();
        stageRng.SetSeed(StageProducer.GlobalRng.Seed + (ulong)StageProducer.Config.BattleRoom.Idx);
        int eventIndex = stageRng.RandiRange(0, EventPool.Count - 1);
        _eventReference = EventDatabase.EventDictionary[EventPool[eventIndex]];

        EventPool.RemoveAt(eventIndex);
        DisplayEvent();
    }

    public override void _Input(InputEvent @event)
    {
        if (@event is InputEventKey || @event is InputEventJoypadButton)
        {
            if (GetViewport().GuiGetFocusOwner() == null)
            {
                if (_continueContainer.Visible)
                    _continueButton.GrabFocus();
                else
                    _buttonContainer.GetChild<Control>(0).GrabFocus();
            }
        }
    }

    private void RefreshPool()
    {
        EventPool = new List<int>();
        for (int i = 0; i < EventDatabase.EventDatabaseSize; i++)
        {
            EventPool.Add(i);
        }
        for (int i = 0; i < EventPool.Count - 2; i++)
        {
            int randIdx = StageProducer.GlobalRng.RandiRange(0, EventPool.Count - 1);
            (EventPool[i], EventPool[randIdx]) = (EventPool[randIdx], EventPool[i]); //rad
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

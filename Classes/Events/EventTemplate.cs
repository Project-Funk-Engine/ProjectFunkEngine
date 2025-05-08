using System;
using FunkEngine;
using Godot;

public delegate void EventAction();

public partial class EventTemplate
{
    public int Id;
    public string EventDescription;
    public string[] ButtonDescriptions;
    public string[] OutcomeDescriptions;
    public Texture2D Texture;

    // Note: Actions are NOT exported since delegates cannot be serialized
    public EventAction[] OptionActions;

    public EventTemplate() { }

    public EventTemplate(
        int id,
        string eventDescription,
        string[] buttonDescriptions,
        string[] outcomeDescriptions,
        EventAction[] optionActions,
        Texture2D texture
    )
    {
        Id = id;
        EventDescription = eventDescription;
        ButtonDescriptions = buttonDescriptions;
        OutcomeDescriptions = outcomeDescriptions;
        OptionActions = optionActions;
        Texture = texture;
    }
}

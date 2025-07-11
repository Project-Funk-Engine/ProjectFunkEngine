using System;
using Godot;

public delegate void EventAction(EventTemplate self, EventScene contextNode);
public delegate bool EventCondition();

//TODO: Consider making event option struct for better parallelizing of option parameters

public class EventTemplate
{
    public int Id;
    public string EventDescription;
    public string[] ButtonDescriptions;
    public string[] OutcomeDescriptions;
    public Texture2D Texture;
    public EventAction[] OptionActions;
    public EventCondition[] OptionEnabledConditions;

    public EventTemplate() { }

    public EventTemplate(
        int id,
        string eventDescription,
        string[] buttonDescriptions,
        string[] outcomeDescriptions,
        EventAction[] optionActions,
        Texture2D texture,
        EventCondition[] optionEnabledConditions = null
    )
    {
        Id = id;
        EventDescription = eventDescription;
        ButtonDescriptions = buttonDescriptions;
        OutcomeDescriptions = outcomeDescriptions;
        OptionActions = optionActions;
        Texture = texture;
        OptionEnabledConditions = optionEnabledConditions;
    }
}

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
    public Func<EventTemplate, EventScene, string>[] OutcomeResolver;
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
        EventCondition[] optionEnabledConditions = null,
        Func<EventTemplate, EventScene, string>[] outcomeResolvers = null
    )
    {
        Id = id;
        EventDescription = eventDescription;
        ButtonDescriptions = buttonDescriptions;
        OutcomeDescriptions = outcomeDescriptions;
        OptionActions = optionActions;
        Texture = texture;
        OptionEnabledConditions = optionEnabledConditions;
        OutcomeResolver = outcomeResolvers;
    }

    public string GetOutcomeText(int index, EventScene context)
    {
        if (
            OutcomeResolver != null
            && index >= 0
            && index < OutcomeResolver.Length
            && OutcomeResolver[index] != null
        )
        {
            return OutcomeResolver[index](this, context);
        }

        // Fallback to static string
        if (OutcomeDescriptions != null && index >= 0 && index < OutcomeDescriptions.Length)
        {
            return TranslationServer.Translate(OutcomeDescriptions[index]);
        }

        return "[Missing outcome text]";
    }
}

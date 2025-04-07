using Godot;

/**
 * <summary>Generic button for holding and handling displayable objects.</summary>
 */
public partial class DisplayButton : Button
{
    //TODO: Make various menus change descriptions when focus changes, instead of on click.
    public static readonly string LoadPath = "res://Scenes/UI/DisplayButton.tscn";

    [Export]
    public Texture2D Texture;

    [Export]
    public string Description;

    [Export]
    public string DisplayName;

    public void Display(
        Texture2D texture,
        string description,
        string name,
        bool focusHandling = false
    )
    {
        Texture = texture;
        Description = description;
        DisplayName = name;
        Icon = Texture;

        if (focusHandling)
            FocusEntered += Selected;
    }

    //Signal pressed on focus enter, if changing focus is equivalent to pressing
    private void Selected()
    {
        EmitSignal(BaseButton.SignalName.Pressed);
        SetPressed(true);
    }
}

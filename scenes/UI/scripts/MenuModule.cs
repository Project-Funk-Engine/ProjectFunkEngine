using FunkEngine;
using Godot;

/**
 * <summary>Simple system for any scene which should display pause and inventory screen.</summary>
 */
public partial class MenuModule : CanvasLayer, IFocusableMenu
{
    [Export]
    private Node CurSceneNode { get; set; }

    private Control _lastFocused { get; set; }

    public void ResumeFocus()
    {
        CurSceneNode.ProcessMode = ProcessModeEnum.Inherit;
        _lastFocused?.GrabFocus();
    }

    public void PauseFocus()
    {
        _lastFocused = GetViewport().GuiGetFocusOwner();
        CurSceneNode.ProcessMode = ProcessModeEnum.Disabled;
    }

    public IFocusableMenu Prev { get; set; }

    public void OpenMenu(IFocusableMenu prev)
    {
        GD.PushWarning("Undefined behaviour, MenuModule should not be opened!");
    }

    public void ReturnToPrev()
    {
        GD.PushWarning("Undefined behaviour, MenuModule should not return to previous menu!");
    }

    public override void _Input(InputEvent @event)
    {
        if (@event.IsActionPressed("Pause"))
        {
            var pauseMenu = GD.Load<PackedScene>(PauseMenu.LoadPath).Instantiate<PauseMenu>();
            AddChild(pauseMenu);
            pauseMenu.OpenMenu(this);
        }
        if (@event.IsActionPressed("Inventory"))
        {
            var invenMenu = GD.Load<PackedScene>(Inventory.LoadPath).Instantiate<Inventory>();
            AddChild(invenMenu);
            invenMenu.OpenMenu(this);
        }
    }
}

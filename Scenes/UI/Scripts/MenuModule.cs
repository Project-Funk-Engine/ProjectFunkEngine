using FunkEngine;
using Godot;
using GodotSteam;

/**
 * <summary>Simple system for any scene which should display pause and inventory screen.</summary>
 */
public partial class MenuModule : CanvasLayer, IFocusableMenu
{
    [Export]
    private Node CurSceneNode { get; set; }

    private Control _lastFocused { get; set; }

    [Export]
    bool _ignorePause = false;

    public override void _Ready()
    {
        Input.JoyConnectionChanged += (device, connected) =>
        {
            if (!connected)
                OpenPauseMenu(); //Pause on disconnection
        };
        GetTree().GetRoot().FocusExited += OpenPauseMenu;
        Steam.OverlayToggled += OpenPauseMenu;
    }

    public override void _ExitTree()
    {
        Steam.OverlayToggled -= OpenPauseMenu;
    }

    public void ResumeFocus()
    {
        CurSceneNode.ProcessMode = ProcessModeEnum.Inherit;
        if (CurSceneNode is BattleDirector { HasPlayed: true } bd && !GetTree().IsPaused())
            bd.StartCountdown();
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
        if (ControlSettings.IsOutOfFocus(this))
        {
            GetViewport().SetInputAsHandled();
            return;
        }
        if (@event.IsActionPressed("Pause"))
        {
            OpenPauseMenu();
        }
        if (
            @event.IsActionPressed("WASD_inventory")
            || @event.IsActionPressed("CONTROLLER_inventory")
        )
        {
            var invenMenu = GD.Load<PackedScene>(Inventory.LoadPath).Instantiate<Inventory>();
            AddChild(invenMenu);
            invenMenu.OpenMenu(this);
        }
    }

    private void OpenPauseMenu(bool active, bool initiated = false, uint id = 0)
    {
        if (active)
            OpenPauseMenu();
    }

    private void OpenPauseMenu()
    {
        if (_ignorePause)
            return;
        if (CurSceneNode.ProcessMode == ProcessModeEnum.Disabled)
            return;
        var pauseMenu = GD.Load<PackedScene>(PauseMenu.LoadPath).Instantiate<PauseMenu>();
        AddChild(pauseMenu);
        pauseMenu.OpenMenu(this);
    }
}

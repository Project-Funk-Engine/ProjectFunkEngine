using System;
using Godot;

public partial class OptionsMenu : CanvasLayer
{
    private Node _previousScene;

    [Export]
    private Control _focused;
    private ProcessModeEnum _previousProcessMode;

    [Export]
    private HSlider _volumeSlider;

    [Export]
    private Button _closeButton;

    [Export]
    private Button _controlsButton;

    private const float MinVolumeVal = 50f;

    public override void _Ready()
    {
        _focused.GrabFocus();
        _volumeSlider.MinValue = MinVolumeVal;
        _volumeSlider.Value = AudioServer.GetBusVolumeDb(AudioServer.GetBusIndex("Master")) + 80;
        _volumeSlider.DragEnded += VolumeChanged;
        _closeButton.Pressed += CloseMenu;
        _controlsButton.Pressed += OpenControls;
    }

    public override void _Process(double delta) //TODO: Better method for returning focus
    {
        if (GetViewport().GuiGetFocusOwner() == null)
        {
            _focused.GrabFocus();
        }
    }

    public override void _Input(InputEvent @event)
    {
        if (@event.IsActionPressed("Pause"))
        {
            CloseMenu();
            GetViewport().SetInputAsHandled();
        }
    }

    public void OpenMenu(Node prevScene)
    {
        _previousScene = prevScene;
        _previousProcessMode = _previousScene.GetProcessMode();
        prevScene.ProcessMode = ProcessModeEnum.Disabled;
    }

    private void CloseMenu()
    {
        _previousScene.ProcessMode = _previousProcessMode;
        QueueFree();
    }

    private void OpenControls()
    {
        ControlSettings controlSettings = GD.Load<PackedScene>("res://scenes/Remapping/Remap.tscn")
            .Instantiate<ControlSettings>();
        AddChild(controlSettings);
        controlSettings.OpenMenu(this);
    }

    private void VolumeChanged(bool valueChanged)
    {
        if (!valueChanged)
            return;
        ChangeVolume((float)_volumeSlider.Value);
        SaveSystem.UpdateConfig(nameof(ConfigData.Volume), _volumeSlider.Value);
    }

    public static void ChangeVolume(float value)
    {
        AudioServer.SetBusVolumeDb(AudioServer.GetBusIndex("Master"), (float)value - 80);
        AudioServer.SetBusMute(
            AudioServer.GetBusIndex("Master"),
            Math.Abs(value - MinVolumeVal) < .1
        );
    }
}

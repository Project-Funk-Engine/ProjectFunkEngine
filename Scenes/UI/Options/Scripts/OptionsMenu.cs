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

    [Export]
    private CheckBox _highContrastToggle;

    [Export]
    private Button _howToPlayButton;

    private const float MinVolumeVal = 50f;

    public override void _Ready()
    {
        _focused.GrabFocus();

        _volumeSlider.MinValue = MinVolumeVal;
        _volumeSlider.Value = AudioServer.GetBusVolumeDb(AudioServer.GetBusIndex("Master")) + 80;

        _highContrastToggle.ButtonPressed = SaveSystem
            .GetConfigValue(SaveSystem.ConfigSettings.HighContrast)
            .AsBool();

        _volumeSlider.DragEnded += VolumeChanged;
        _volumeSlider.ValueChanged += ChangeVolume;

        _closeButton.Pressed += CloseMenu;
        _controlsButton.Pressed += OpenControls;
        _highContrastToggle.Toggled += HighContrastChanged;
        _howToPlayButton.Pressed += OpenHowToPlay;
    }

    //TODO: Menu subclass/interface
    public override void _Process(double delta)
    {
        if (GetViewport().GuiGetFocusOwner() == null)
        {
            _focused.GrabFocus();
        }
    }

    public override void _Input(InputEvent @event)
    {
        if (@event.IsActionPressed("ui_cancel"))
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
        ControlSettings controlSettings = GD.Load<PackedScene>("res://Scenes/UI/Remapping/Remap.tscn")
            .Instantiate<ControlSettings>();
        AddChild(controlSettings);
        controlSettings.OpenMenu(this);
    }

    private void VolumeChanged(bool valueChanged)
    {
        if (!valueChanged)
            return;
        ChangeVolume((float)_volumeSlider.Value);
        SaveSystem.UpdateConfig(SaveSystem.ConfigSettings.Volume, _volumeSlider.Value);
    }

    public static void ChangeVolume(double value)
    {
        AudioServer.SetBusVolumeDb(AudioServer.GetBusIndex("Master"), (float)value - 80);
        AudioServer.SetBusMute(
            AudioServer.GetBusIndex("Master"),
            Math.Abs(value - MinVolumeVal) < .1
        );
    }

    private void HighContrastChanged(bool toggled)
    {
        StageProducer.ContrastFilter.Visible = toggled;
        SaveSystem.UpdateConfig(SaveSystem.ConfigSettings.HighContrast, toggled);
    }

    private void OpenHowToPlay()
    {
        HowToPlay howtoPlay = GD.Load<PackedScene>("res://Scenes/UI/Options/HowToPlay.tscn")
            .Instantiate<HowToPlay>();
        AddChild(howtoPlay);
        howtoPlay.OpenMenu(this);
    }
}

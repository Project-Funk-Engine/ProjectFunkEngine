using System.Collections.Generic;
using FunkEngine;
using Godot;

public partial class ControlSettings : Node2D, IFocusableMenu
{
    public static readonly string LoadPath = "res://Scenes/UI/Remapping/Remap.tscn";

    [Export]
    public Sprite2D LeftKey;

    [Export]
    public Sprite2D RightKey;

    [Export]
    public Sprite2D UpKey;

    [Export]
    public Sprite2D DownKey;

    public IFocusableMenu Prev { get; set; }

    [Export]
    private Button _closeButton;

    private Button _controllerButton;

    private static readonly Dictionary<string, Dictionary<string, string>> SpriteMappings = new()
    {
        {
            "WASD",
            new Dictionary<string, string>
            {
                { "left", "res://Scenes/UI/Remapping/Assets/A_Key_Light.png" },
                { "right", "res://Scenes/UI/Remapping/Assets/D_Key_Light.png" },
                { "up", "res://Scenes/UI/Remapping/Assets/W_Key_Light.png" },
                { "down", "res://Scenes/UI/Remapping/Assets/S_Key_Light.png" },
            }
        },
        {
            "ARROWS",
            new Dictionary<string, string>
            {
                { "left", "res://Scenes/UI/Remapping/Assets/Arrow_Left_Key_Light.png" },
                { "right", "res://Scenes/UI/Remapping/Assets/Arrow_Right_Key_Light.png" },
                { "up", "res://Scenes/UI/Remapping/Assets/Arrow_Up_Key_Light.png" },
                { "down", "res://Scenes/UI/Remapping/Assets/Arrow_Down_Key_Light.png" },
            }
        },
        {
            "QWERT",
            new Dictionary<string, string>
            {
                { "left", "res://Scenes/UI/Remapping/Assets/Q_Key_Light.png" },
                { "right", "res://Scenes/UI/Remapping/Assets/R_Key_Light.png" },
                { "up", "res://Scenes/UI/Remapping/Assets/W_Key_Light.png" },
                { "down", "res://Scenes/UI/Remapping/Assets/E_Key_Light.png" },
            }
        },
        {
            "CONTROLLER",
            new Dictionary<string, string>
            {
                { "left", "res://Scenes/UI/Remapping/Assets/Positional_Prompts_Left.png" },
                { "right", "res://Scenes/UI/Remapping/Assets/Positional_Prompts_Right.png" },
                { "up", "res://Scenes/UI/Remapping/Assets/Positional_Prompts_Up.png" },
                { "down", "res://Scenes/UI/Remapping/Assets/Positional_Prompts_Down.png" },
            }
        },
    };

    public override void _Ready()
    {
        GetNode<Button>("Panel/WASDButton").Connect("pressed", Callable.From(OnWASDButtonPressed));
        GetNode<Button>("Panel/ArrowButton")
            .Connect("pressed", Callable.From(OnArrowButtonPressed));
        GetNode<Button>("Panel/QWERTButton")
            .Connect("pressed", Callable.From(OnQWERTButtonPressed));
        _controllerButton = GetNode<Button>("Panel/ControllerButton");
        _controllerButton.Connect("pressed", Callable.From(OnControllerButtonPressed));

        ControllerConnectionChanged(-1, false);
        Input.JoyConnectionChanged += ControllerConnectionChanged;

        _closeButton.Pressed += ReturnToPrev;
    }

    public void ResumeFocus()
    {
        ProcessMode = ProcessModeEnum.Inherit;
        GetCurrentSelection();
    }

    public void PauseFocus()
    {
        ProcessMode = ProcessModeEnum.Disabled;
    }

    public void OpenMenu(IFocusableMenu prev)
    {
        Prev = prev;
        Prev.PauseFocus();

        GetCurrentSelection();
    }

    public void ReturnToPrev()
    {
        Prev.ResumeFocus();
        QueueFree();
    }

    private void GetCurrentSelection()
    {
        string scheme = SaveSystem.GetConfigValue(SaveSystem.ConfigSettings.InputKey).As<string>();
        switch (scheme)
        {
            case "ARROWS":
                OnArrowButtonPressed();
                GetNode<Button>("Panel/ArrowButton").GrabFocus();
                break;
            case "QWERT":
                OnQWERTButtonPressed();
                GetNode<Button>("Panel/QWERTButton").GrabFocus();
                break;
            case "WASD":
                OnWASDButtonPressed();
                GetNode<Button>("Panel/WASDButton").GrabFocus();
                break;
            case "CONTROLLER":
                OnControllerButtonPressed();
                GetNode<Button>("Panel/ControllerButton").GrabFocus();
                break;
            default:
                _closeButton.GrabFocus();
                break;
        }
    }

    public override void _Input(InputEvent @event)
    {
        if (!GetWindow().HasFocus())
        {
            GetViewport().SetInputAsHandled();
            return;
        }
        if (@event.IsActionPressed("ui_cancel"))
        {
            ReturnToPrev();
            GetViewport().SetInputAsHandled();
        }
    }

    private void OnWASDButtonPressed()
    {
        GetNode<Label>("Panel/Label").Text =
            Tr("CONTROLS_TITLE_TYPE_WASD") + " " + Tr("CONTROLS_TITLE_SELECTED");
        SaveSystem.UpdateConfig(SaveSystem.ConfigSettings.InputKey, "WASD");
        ChangeKeySprites("WASD");
    }

    private void OnArrowButtonPressed()
    {
        GetNode<Label>("Panel/Label").Text =
            Tr("CONTROLS_TITLE_TYPE_ARROW") + " " + Tr("CONTROLS_TITLE_SELECTED");
        SaveSystem.UpdateConfig(SaveSystem.ConfigSettings.InputKey, "ARROWS");
        ChangeKeySprites("ARROWS");
    }

    private void OnQWERTButtonPressed()
    {
        GetNode<Label>("Panel/Label").Text =
            Tr("CONTROLS_TITLE_TYPE_QWER") + " " + Tr("CONTROLS_TITLE_SELECTED");
        SaveSystem.UpdateConfig(SaveSystem.ConfigSettings.InputKey, "QWERT");
        ChangeKeySprites("QWERT");
    }

    private void OnControllerButtonPressed()
    {
        GetNode<Label>("Panel/Label").Text = "Controller Selected";
        SaveSystem.UpdateConfig(SaveSystem.ConfigSettings.InputKey, "CONTROLLER");
        ChangeKeySprites("CONTROLLER");
    }

    private void ControllerConnectionChanged(long id, bool connected)
    {
        _controllerButton.Visible = Input.GetConnectedJoypads().Count > 0;
        if (
            (string)SaveSystem.GetConfigValue(SaveSystem.ConfigSettings.InputKey) == "CONTROLLER"
            && !_controllerButton.Visible
        )
        {
            OnArrowButtonPressed();
        }
    }

    private void ChangeKeySprites(string scheme)
    {
        var selectedScheme = SpriteMappings[scheme];
        LeftKey.Texture = GD.Load<Texture2D>(selectedScheme["left"]);
        RightKey.Texture = GD.Load<Texture2D>(selectedScheme["right"]);
        UpKey.Texture = GD.Load<Texture2D>(selectedScheme["up"]);
        DownKey.Texture = GD.Load<Texture2D>(selectedScheme["down"]);
    }
}

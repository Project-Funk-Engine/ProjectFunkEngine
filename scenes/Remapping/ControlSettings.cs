using System;
using Godot;

public partial class ControlSettings : Node2D
{
    [Export]
    public Sprite2D leftKey;

    [Export]
    public Sprite2D rightKey;

    [Export]
    public Sprite2D upKey;

    [Export]
    public Sprite2D downKey;

    public override void _Ready()
    {
        GetNode<Button>("Panel/WASDButton").Connect("pressed", Callable.From(OnWASDButtonPressed));
        GetNode<Button>("Panel/ArrowButton")
            .Connect("pressed", Callable.From(OnArrowButtonPressed));
        GetNode<Button>("Panel/QWERTButton")
            .Connect("pressed", Callable.From(OnQWERTButtonPressed));

        string scheme = ProjectSettings.HasSetting("game/input_scheme")
            ? (string)ProjectSettings.GetSetting("game/input_scheme")
            : "ARROWS";
        ChangeKeySprites(scheme);
    }

    private void OnWASDButtonPressed()
    {
        GetNode<Label>("Panel/Label").Text = "WASD Selected";
        ProjectSettings.SetSetting("game/input_scheme", "WASD");
        ProjectSettings.Save();
        ChangeKeySprites("WASD");
    }

    private void OnArrowButtonPressed()
    {
        GetNode<Label>("Panel/Label").Text = "Arrow Selected";
        ProjectSettings.SetSetting("game/input_scheme", "ARROWS");
        ProjectSettings.Save();
        ChangeKeySprites("ARROWS");
    }

    private void OnQWERTButtonPressed()
    {
        GetNode<Label>("Panel/Label").Text = "QWERT Selected";
        ProjectSettings.SetSetting("game/input_scheme", "QWERT");
        ProjectSettings.Save();
        ChangeKeySprites("QWERT");
    }

    private void ChangeKeySprites(string scheme)
    {
        var selectedScheme = ControlSchemes.SpriteMappings[scheme];
        GD.Print(scheme);
        leftKey.Texture = GD.Load<Texture2D>(selectedScheme["left"]);
        rightKey.Texture = GD.Load<Texture2D>(selectedScheme["right"]);
        upKey.Texture = GD.Load<Texture2D>(selectedScheme["up"]);
        downKey.Texture = GD.Load<Texture2D>(selectedScheme["down"]);
    }
}

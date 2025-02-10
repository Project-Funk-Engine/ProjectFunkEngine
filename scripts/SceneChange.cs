using System;
using Godot;

public partial class SceneChange : Button
{
    [Export]
    public string ScenePath = "";

    public override void _Ready()
    {
        Pressed += OnButtonPressed;
        GD.Print($"[DEBUG] Scene Path: '{ScenePath}'");
    }

    private void OnButtonPressed()
    {
        //ScenePath = ScenePath.Trim('\"');
        if (ScenePath.ToLower() == "exit")
        {
            GD.Print("Exiting game");
            GetTree().Quit();
            return;
        }

        if (string.IsNullOrEmpty(ScenePath) || !ResourceLoader.Exists(ScenePath))
        {
            GD.PrintErr($"❌ Scene not found: {ScenePath}");
            GD.Print($"[DEBUG] Trying to load: '{ScenePath}'");
            return;
        }

        GD.Print($"✅ Loading scene: {ScenePath}");
        GetTree().ChangeSceneToFile(ScenePath);
    }
}
